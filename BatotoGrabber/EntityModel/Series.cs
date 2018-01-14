using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace BatotoGrabber.EntityModel
{
    [Table("Series")]
    public class Series
    {
        [Key]
        public string PrimaryName { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }

        public byte[] Image { get; set; }

        public virtual List<Creator> Authors { get; set; }
        public virtual List<Creator> Artists { get; set; }
        public virtual List<Genre> Genres { get; set; }
        public virtual List<Chapter> Chapters { get; set; }

        public class SeriesConfiguration : EntityTypeConfiguration<Series>
        {
            public SeriesConfiguration()
            {
                HasMany(x => x.Authors)
                    .WithMany()
                    .Map(
                        x =>
                        {
                            x.MapLeftKey("SeriesName");
                            x.MapRightKey("CreatorName");
                            x.ToTable("SeriesAuthor");
                        });
                HasMany(x => x.Artists)
                    .WithMany()
                    .Map(
                        x =>
                        {
                            x.MapLeftKey("SeriesName");
                            x.MapRightKey("CreatorName");
                            x.ToTable("SeriesArtist");
                        });
                HasMany(x => x.Genres)
                    .WithMany()
                    .Map(
                        x =>
                        {
                            x.MapLeftKey("SeriesName");
                            x.MapRightKey("GenreName");
                            x.ToTable("SeriesGenre");
                        });
                HasMany(x => x.Chapters)
                    .WithRequired()
                    .Map(x => x.MapKey("Series"));
            }
        }
    }
}
