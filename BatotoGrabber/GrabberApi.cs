using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using BatotoGrabber.Model;
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
            var response = await _browserControl.EvaluateScriptAsyncEx(Scripts.Script.GetFollowedSeries);

            return JsonConvert.DeserializeObject<FollowedSeries[]>((string)response);
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
            } while (httpStatus > 500);

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

            var result = await _browserControl.EvaluateScriptAsyncEx(Scripts.Script.GetSeriesInfo);

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
            await _browserControl.LoadUrl(groupRef.Url);

            var result = await _browserControl.EvaluateScriptAsyncEx(Scripts.Script.GetGroupInfo);

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