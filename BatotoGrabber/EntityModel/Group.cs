using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BatotoGrabber.EntityModel
{
    [Table("Groups")]
    public class Group
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Website { get; set; }
        public string Description { get; set; }
        public string Delay { get; set; }
    }
}
