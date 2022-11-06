using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TvShows.Info.DAL.Models
{
    [Table("scrapes")]
    public class Scrape
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        [Required(ErrorMessage = "TvShowId is required")]
        public int TvShowId { get; set; }
        [Timestamp]
        public DateTime ScrapeDate { get; set; }

    }
}
