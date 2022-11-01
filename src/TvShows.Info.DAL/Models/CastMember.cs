namespace TvShows.Info.DAL.Context.Models
{
    public class CastMember
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Bitrthday { get; set; }

        public CastMember(string name, DateTime bitrthday)
        {
            Name = name;
            Bitrthday = bitrthday;
        }

    }
}
