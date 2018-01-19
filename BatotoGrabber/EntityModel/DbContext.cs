using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using BatotoGrabber.Model;

namespace BatotoGrabber.EntityModel
{
    public class DbContext : System.Data.Entity.DbContext
    {
        public DbContext(DbConnection dbConnection) 
            : base(dbConnection, false)
        {
        }

        public DbSet<Chapter> Chapters { get; set; }
        public DbSet<Creator> Creators { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Series> Series { get; set; }

        protected override void OnModelCreating(DbModelBuilder mb)
        {
            mb.Configurations.AddFromAssembly(typeof(DbContext).Assembly);
        }

        public async Task SaveToDatabase(
            IReadOnlyCollection<SeriesInfo> seriesInfos,
            IEnumerable<GroupInfo> groupInfos,
            Dictionary<string, FollowedSeriesLastRead> lastReads)
        {
            var genres = seriesInfos.SelectMany(x => x.Genres)
                .Distinct()
                .ToDictionary(x => x, x => new Genre { Name = x });
            var groups = groupInfos.ToDictionary(
                x => x.Url,
                x => new Group { Name = x.Name, Delay = x.Delay, Description = x.Description, Website = x.Website });
            var creators = seriesInfos.SelectMany(x => x.Authors)
                .Concat(seriesInfos.SelectMany(x => x.Artists))
                .Distinct()
                .ToDictionary(x => x, x => new Creator { Name = x });

            var series = seriesInfos.Select(
                s => new Series
                {
                    PrimaryName = s.Name,
                    Type = s.Type,
                    Status = s.Status,
                    Description = s.Description,
                    Genres = genres.GetMany(s.Genres).ToList(),
                    Artists = creators.GetMany(s.Artists).ToList(),
                    Authors = creators.GetMany(s.Authors).ToList(),
                    Chapters = s.Chapters.Select(
                            c => new Chapter
                            {
                                Title = c.Title,
                                Contributor = c.Contributor,
                                Date = c.Date,
                                Language = c.Language,
                                Groups = groups.GetMany(c.Groups.Select(g => g.Url)).ToList(),
                                LastRead = c.Url != null && lastReads.TryGetValue(c.Url, out var lr)
                                    ? lr.LastReadDate
                                    : null
                            })
                        .ToList()
                });

            Genres.AddRange(genres.Values);
            Creators.AddRange(creators.Values);
            Groups.AddRange(groups.Values);
            Series.AddRange(series);

            await SaveChangesAsync();
        }
    }
}
