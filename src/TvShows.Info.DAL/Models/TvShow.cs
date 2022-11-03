using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TvShows.Info.DAL.Context.Models
{
    [Table("tvshow")]
    public class TvShow
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Name is required")]
        public string? Name { get; set; }
        public List<CastMember>? Cast { get; set; }

        [Timestamp]
        public DateTime LastUpdates { get; set; }
    }
}
