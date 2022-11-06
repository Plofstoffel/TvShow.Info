using System.Text.Json.Serialization;
using TvShows.Info.DAL.Context.Models;

namespace TvShows.Info.ScrapeWorkerService.Dto
{
    public class CastMembersDto
    {
        [JsonPropertyName("person")]
        public CastMember? CastMember { get; set; }
    }
}
