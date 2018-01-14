using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BatotoGrabber.EntityModel
{
    [Table("Genres")]
    public class Genre
    {
        [Key]
        public string Name { get; set; }
    }
}