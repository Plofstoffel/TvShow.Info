namespace TvShows.Info.DAL.Repository
{
    public abstract class RepositoryBase<T> : IRepositoryBase<T> where T : class
    {
        protected TvShowDbContext TvShowDbContext { get; set; }
        protected RepositoryBase(TvShowDbContext tvShowDbContext)
        {
            TvShowDbContext = tvShowDbContext;
        }

        public T AddOrUpdate(T entity)
        {
            if (TvShowDbContext.Set<T>().Any(x => x == entity))
            {
                return TvShowDbContext.Set<T>().Update(entity).Entity;
            }
            else
            {
                return TvShowDbContext.Set<T>().Add(entity).Entity;
            }
        }
    }
}
