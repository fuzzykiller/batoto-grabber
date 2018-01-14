using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BatotoGrabber.EntityModel
{
    [Table("Creators")]
    public class Creator
    {
        [Key]
        public string Name { get; set; }
    }
}