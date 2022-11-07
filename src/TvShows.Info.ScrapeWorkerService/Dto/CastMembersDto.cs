using Newtonsoft.Json;
using TvShows.Info.DAL.Context.Models;

namespace TvShows.Info.ScrapeWorkerService.Dto
{
    public class CastMembersDto
    {
        [JsonProperty("person")]
        public CastMember? CastMember { get; set; }
    }
}
