using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SQLite;
using System.Linq;
using BatotoGrabber.Model;
using BatotoGrabber.Scripts;

namespace BatotoGrabber.EntityModel
{
    public class DbContext : System.Data.Entity.DbContext
    {
        private readonly SQLiteConnection _dbConnection;

        public DbContext(string databaseFile)
            : this(CreateConnection(databaseFile))
        {
        }

        private DbContext(SQLiteConnection dbConnection) 
            : base(dbConnection, true)
        {
            _dbConnection = dbConnection;
        }

        public DbSet<Chapter> Chapters { get; set; }
        public DbSet<Creator> Creators { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Series> Series { get; set; }

        public void CreateDatabase()
        {
            var previousState = _dbConnection.State;
            if (previousState != ConnectionState.Open)
            {
                _dbConnection.Open();
            }

            using (var cmd = _dbConnection.CreateCommand())
            {
                cmd.CommandText = Script.CreateDatabbase;
                cmd.ExecuteNonQuery();
            }

            if (previousState == ConnectionState.Closed)
            {
                _dbConnection.Close();
            }
        }

        protected override void OnModelCreating(DbModelBuilder mb)
        {
            mb.Configurations.AddFromAssembly(typeof(DbContext).Assembly);
        }

        private static SQLiteConnection CreateConnection(string databaseFile)
        {
            var csb = new SQLiteConnectionStringBuilder { DataSource = databaseFile };

            return new SQLiteConnection(csb.ToString());
        }

        public static void SaveToDatabase(
            DbContext ctx,
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
                        .ToList(),
                    Image = s.Image
                });

            ctx.Genres.AddRange(genres.Values);
            ctx.Creators.AddRange(creators.Values);
            ctx.Groups.AddRange(groups.Values);
            ctx.Series.AddRange(series);

            ctx.SaveChanges();
        }
    }
}
