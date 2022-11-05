using TvShows.Info.DAL.Context.Models;

namespace TvShows.Info.DAL.Repository
{
    public interface ITvShowRepository: IRepositoryBase<TvShow>
    {        
        IQueryable<TvShow> GetOutDatedTvShows(DateTime fromDate);
        IQueryable<TvShow> GetTvShows(int pageSize, int pageNumber);
        IQueryable<int> GetTvShowIds();
    }
}
