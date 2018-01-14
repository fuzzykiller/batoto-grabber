using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace BatotoGrabber.EntityModel
{
    [Table("Chapters")]
    public class Chapter
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Language { get; set; }
        public string Contributor { get; set; }
        public string Date { get; set; }

        public virtual List<Group> Groups { get; set; }

        public class ChapterConfiguration : EntityTypeConfiguration<Chapter>
        {
            public ChapterConfiguration()
            {
                HasMany(x => x.Groups)
                    .WithMany()
                    .Map(
                        x =>
                        {
                            x.MapLeftKey("ChapterId");
                            x.MapRightKey("GroupId");
                            x.ToTable("ChapterGroup");
                        });
            }
        }
    }
}
