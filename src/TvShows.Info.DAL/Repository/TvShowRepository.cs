using Microsoft.EntityFrameworkCore;
using TvShows.Info.DAL.Context.Models;

namespace TvShows.Info.DAL.Repository
{
    public class TvShowRepository : RepositoryBase<TvShow>, ITvShowRepository
    {
        public TvShowRepository(TvShowDbContext tvShowDbContext) : base(tvShowDbContext)
        {            
        }        

        public IQueryable<TvShow> GetOutDatedTvShows(DateTime fromDate) => TvShowDbContext.Set<TvShow>().AsNoTracking().Where(x => x.LastUpdates <= fromDate);

        public IQueryable<int> GetTvShowIds() => TvShowDbContext.Set<TvShow>().AsNoTracking().Select(x => x.Id);

        public IQueryable<TvShow> GetTvShows(int pageSize, int pageNumber) => TvShowDbContext.Set<TvShow>().AsNoTracking().Skip(pageSize * (pageNumber-1)).Take(pageSize);
      
    }
}
