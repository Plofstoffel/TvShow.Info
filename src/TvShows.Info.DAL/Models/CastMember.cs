using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TvShows.Info.DAL.Context.Models
{
    [Table("castmember")]
    public class CastMember
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Name is required")]
        public string? Name { get; set; }
        [Required(ErrorMessage = "Bitrthday is required")]
        public DateTime Bitrthday { get; set; }
    }
}
