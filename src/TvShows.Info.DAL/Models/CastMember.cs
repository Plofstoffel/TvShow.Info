using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TvShows.Info.DAL.Context.Models
{
    [Table("castmember")]
    public class CastMember
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Name is required")]
        public string? Name { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? Birthday { get; set; }
        public List<TvShow> TvShows { get; set; }

        public CastMember()
        {
            TvShows = new List<TvShow>();
        }
    }
}
