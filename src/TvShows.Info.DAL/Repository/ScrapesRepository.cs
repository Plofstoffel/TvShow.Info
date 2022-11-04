using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TvShows.Info.DAL.Context.Models;
using TvShows.Info.DAL.Models;

namespace TvShows.Info.DAL.Repository
{
    public class ScrapesRepository : RepositoryBase<Scrape>, IScrapesRepository
    {
        public ScrapesRepository(TvShowDbContext tvShowDbContext) : base(tvShowDbContext)
        {
        }

        public IQueryable<Scrape> GetScrapes() => TvShowDbContext.Set<Scrape>().AsNoTracking();

        public void RemoveScrapes(List<Scrape> scrapes) => TvShowDbContext.Set<Scrape>().RemoveRange(scrapes);
    }
}
