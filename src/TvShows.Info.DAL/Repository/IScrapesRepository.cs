using TvShows.Info.DAL.Models;

namespace TvShows.Info.DAL.Repository
{
    public interface IScrapesRepository : IRepositoryBase<Scrape>
    {
        IQueryable<Scrape> GetScrapes();
        void RemoveScrape(Scrape scrape);
        void RemoveScrapes(List<Scrape> scrapes);
    }
}
