using TvShows.Info.DAL.Models;
using TvShows.Info.DAL.Repository;

namespace TvShows.Info.ScrapeWorkerService
{
    public class ScrapeWorker : BackgroundService
    {
        private readonly ILogger<ScrapeWorker> _logger;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private List<Scrape> _scrapeList;

        public ScrapeWorker(ILogger<ScrapeWorker> logger, IRepositoryWrapper repositoryWrapper)
        {
            _logger = logger;
            _repositoryWrapper = repositoryWrapper;
            _scrapeList = new List<Scrape>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _scrapeList = GetScrapes();
                while (_scrapeList.Any())
                {
                    foreach (var scrape in _scrapeList)
                    {
                        await ScrapeTvShow(stoppingToken);
                    }
                    _scrapeList = GetScrapes();
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Canceled!");
            }
            finally
            {
                await Finalize();
            }
        }

        private async Task ScrapeTvShow(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            await _repositoryWrapper.SaveAsync();
            await Task.Delay(1000, stoppingToken);
        }

        protected List<Scrape> GetScrapes()
        {
            return _repositoryWrapper.ScrapesRepository.GetScrapes().ToList();
        }

        protected async Task Finalize()
        {
            //Reomve Scrapes.
            _repositoryWrapper.ScrapesRepository.RemoveScrapes(_scrapeList);
            _ = await _repositoryWrapper.SaveAsync();
        }
    }
}