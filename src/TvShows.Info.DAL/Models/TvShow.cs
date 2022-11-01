using System.ComponentModel.DataAnnotations;

namespace TvShows.Info.DAL.Context.Models
{
    public class TvShow
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<CastMember> Cast { get; set; }
        [Timestamp]
        public DateTime LastUpdates { get; set; }

        public TvShow(string name, ICollection<CastMember> cast)
        {
            Name = name;
            Cast = cast;
        }
    }
}
