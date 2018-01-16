using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using BatotoGrabber.Model;
using BatotoGrabber.Scripts;
using CefSharp.WinForms;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace BatotoGrabber
{
    public class GrabberApi
    {
        private readonly ChromiumWebBrowser _browserControl;

        public GrabberApi(ChromiumWebBrowser browserControl)
        {
            _browserControl = browserControl;
        }

        public async Task<FollowedSeries[]> GetFollowedSeries()
        {
            await _browserControl.LoadUrl("https://bato.to/myfollows");

            var response = await _browserControl.EvaluateScriptAsyncEx(Script.GetFollowedSeries);
            var followedSeries = JsonConvert.DeserializeObject<FollowedSeries[]>((string)response);

            followedSeries = followedSeries.Where(
                    x => !string.IsNullOrWhiteSpace(x.Name) && x.Url != "http://bato.to/comic/_/comics/-r")
                .ToArray();

            return followedSeries;
        }

        public async Task<Dictionary<string, FollowedSeriesLastRead>> GetFollowedSeriesLastRead()
        {
            await _browserControl.LoadUrl("https://bato.to/follows_comics");

            var response = await _browserControl.EvaluateScriptAsyncEx(Script.GetFollowedSeriesLastRead);
            var lastReads = JsonConvert.DeserializeObject<FollowedSeriesLastRead[]>((string)response);

            // Work around quirks: One type of deleted series all have the same URL as "last read"
            var lastReadsByChapterUrl = lastReads.GroupBy(x => x.LastReadChapterUrl)
                .Where(x => x.Count() == 1 && x.Key != null)
                .ToDictionary(x => x.Key, x => x.Single());

            return lastReadsByChapterUrl;
        }

        public async Task<SeriesInfo> GetSeriesInfo(FollowedSeries series)
        {
            int httpStatus;
            do
            {
                httpStatus = await _browserControl.LoadUrl(series.Url);
                if (httpStatus > 500)
                {
                    await Task.Delay(10000);
                }
            } while (httpStatus > 500 || httpStatus < 200);

            if (httpStatus > 300)
            {
                return new SeriesInfo
                {
                    Name = series.Name,
                    AltNames = new string[0],
                    Artists = new string[0],
                    Authors = new string[0],
                    Status = "Not Found",
                    Type = "Unknown",
                    Genres = new string[0],
                    Description = "This series has been removed from Batoto. Your “My Follows” list contains a dead link. :(",
                    Chapters = new ChapterInfo[0],
                    Image = null
                };
            }

            var result = await _browserControl.EvaluateScriptAsyncEx(Script.GetSeriesInfo);

            var rawInfo = JsonConvert.DeserializeObject<ProtoSeriesInfo>((string)result);

            var metaDataDict = rawInfo.MetaData.ToDictionary(x => x.Key, x => x.Value);

            var altNames = metaDataDict.TryGetValue("Alt Names", out var encodedAltNames)
                ? JsonConvert.DeserializeObject<string[]>(encodedAltNames)
                : new string[0];

            var genres = metaDataDict.TryGetValue("Genres", out var encodedGenres)
                ? JsonConvert.DeserializeObject<string[]>(encodedGenres)
                : new string[0];

            var artists = metaDataDict.TryGetValue("Artist", out var encodedArtists)
                ? JsonConvert.DeserializeObject<string[]>(encodedArtists)
                : new string[0];

            var authors = metaDataDict.TryGetValue("Author", out var encodedAuthors)
                ? JsonConvert.DeserializeObject<string[]>(encodedAuthors)
                : new string[0];

            metaDataDict.TryGetValue("Description", out var description);
            metaDataDict.TryGetValue("Type", out var type);
            metaDataDict.TryGetValue("Status", out var status);

            var wc = new WebClient { Headers = { ["Referer"] = series.Url } };
            byte[] image = null;
            try
            {
                image = await wc.DownloadDataTaskAsync(rawInfo.Image);
            }
            catch
            {
                // ignore
            }
            finally
            {
                wc.Dispose();
            }

            return new SeriesInfo
            {
                Name = series.Name,
                AltNames = altNames,
                Authors = authors,
                Artists = artists,
                Genres = genres,
                Type = type,
                Status = status,
                Description = description,
                Chapters = rawInfo.Chapters,
                Image = image
            };
        }
        
        public async Task<GroupInfo> GetGroupInfo(GroupRef groupRef)
        {
            int httpStatus;
            do
            {
                httpStatus = await _browserControl.LoadUrl(groupRef.Url);
                if (httpStatus > 500)
                {
                    await Task.Delay(10000);
                }
            } while (httpStatus > 500 || httpStatus < 200);

            if (httpStatus > 300)
            {
                return new GroupInfo
                {
                    Name = groupRef.Name,
                    Description = "This group has been removed from Batoto. :(",
                    Delay = "Unknown",
                    Url = groupRef.Url,
                    Website = "Unknown"
                };
            }

            var result = await _browserControl.EvaluateScriptAsyncEx(Script.GetGroupInfo);

            var rawInfo = JsonConvert.DeserializeObject<KeyValuePair<string, string>[]>((string)result);

            var metaDataDict = rawInfo.ToDictionary(x => x.Key, x => x.Value);

            metaDataDict.TryGetValue("Website", out var website);
            metaDataDict.TryGetValue("Description", out var description);
            metaDataDict.TryGetValue("Delay", out var delay);

            return new GroupInfo
            {
                Url = groupRef.Url,
                Name = groupRef.Name,
                Website = website,
                Description = description,
                Delay = delay
            };
        }

        [UsedImplicitly]
        private class ProtoSeriesInfo
        {
            [UsedImplicitly]
            public KeyValuePair<string, string>[] MetaData { get; set; }

            [UsedImplicitly]
            public ChapterInfo[] Chapters { get; set; }

            [UsedImplicitly]
            public string Image { get; set; }
        }
    }
}