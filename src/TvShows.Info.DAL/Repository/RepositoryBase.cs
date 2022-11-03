using Microsoft.EntityFrameworkCore;

namespace TvShows.Info.DAL.Repository
{
    public abstract class RepositoryBase<T> : IRepositoryBase<T> where T : class
    {
        protected TvShowDbContext TvShowDbContext { get; set; }
        public RepositoryBase(TvShowDbContext tvShowDbContext)
        {
            TvShowDbContext = tvShowDbContext;
        }
        public IQueryable<T> FindAll() => TvShowDbContext.Set<T>().AsNoTracking();
        
        public T AddOrUpdate(T entity) => TvShowDbContext.Set<T>().Update(entity).Entity;
    }
}
