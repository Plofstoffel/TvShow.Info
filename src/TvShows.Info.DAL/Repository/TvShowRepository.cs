using Microsoft.EntityFrameworkCore;
using TvShows.Info.DAL.Context.Models;

namespace TvShows.Info.DAL.Repository
{
    public class TvShowRepository : RepositoryBase<TvShow>, ITvShowRepository
    {
        public TvShowRepository(TvShowDbContext tvShowDbContext) : base(tvShowDbContext)
        {
        }

        public IQueryable<int> GetTvShowIds() => TvShowDbContext.Set<TvShow>().AsNoTracking().Select(x => x.Id);

        public IQueryable<TvShow> GetTvShows(int pageSize, int pageNumber) => TvShowDbContext.Set<TvShow>().Include(t =>t.Cast.OrderByDescending(child => child.Birthday)).AsNoTracking().Skip(pageSize * (pageNumber - 1)).Take(pageSize);

        public void RemoveById(int tvShowId)
        {
            var tvShow = TvShowDbContext.Set<TvShow>().FirstOrDefault(x => x.Id == tvShowId);
            if (tvShow != null)
            {
                TvShowDbContext.Remove(tvShow);
            }
        }
    }
}
