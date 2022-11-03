using TvShows.Info.DAL.Models;

namespace TvShows.Info.DAL.Repository
{
    public interface IScrapesRepository : IRepositoryBase<Scrape>
    {
        IQueryable<Scrape> GetScrapes();
    }
}
