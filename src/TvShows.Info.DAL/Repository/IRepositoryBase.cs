namespace TvShows.Info.DAL.Repository
{
    public interface IRepositoryBase<T>
    {
        T AddOrUpdate(T entity);
    }
}
