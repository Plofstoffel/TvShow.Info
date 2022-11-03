using TvShows.Info.DAL.Context.Models;

namespace TvShows.Info.DAL.Repository
{
    public class TvShowRepository : RepositoryBase<TvShow>, ITvShowRepository
    {
        public TvShowRepository(TvShowDbContext tvShowDbContext) : base(tvShowDbContext)
        {
        }
    }
}
