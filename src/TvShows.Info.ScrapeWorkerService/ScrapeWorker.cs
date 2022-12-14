using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Diagnostics.CodeAnalysis;
using TvShows.Info.DAL;
using TvShows.Info.DAL.Context.Models;
using TvShows.Info.DAL.Models;
using TvShows.Info.DAL.Repository;
using TvShows.Info.ScrapeWorkerService.Dto;
using TvShows.Info.ScrapeWorkerService.Enums;

namespace TvShows.Info.ScrapeWorkerService
{
    public class ScrapeWorker : BackgroundService
    {
        private readonly ILogger<ScrapeWorker> _logger;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private List<Scrape> _scrapeList;
        private List<ShowUpdateDto> _completeShowList;

        public ScrapeWorker(IServiceScopeFactory serviceScopeFactory, ILogger<ScrapeWorker> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _logger = logger;
            _repositoryWrapper = new RepositoryWrapper(serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<TvShowDbContext>());
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _scrapeList = new List<Scrape>();
            _completeShowList = new List<ShowUpdateDto>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                // Were we already canceled?
                stoppingToken.ThrowIfCancellationRequested();

                //Get the complete show list.
                //This needs to be cached in a distributed cache for multiple scrapers to use at once.
                await GetCompleteTvShowList(null);
                await StartScraping();

                _logger.LogInformation("No more TvShows to scrape. Checking stale shows.");

                StaleTvShowUpdate staleTvShowUpdateFrequency;
                if (!Enum.TryParse<StaleTvShowUpdate>(_configuration["StaleTvShowUpdateFrequency"], out staleTvShowUpdateFrequency))
                {
                    staleTvShowUpdateFrequency = StaleTvShowUpdate.Day;
                    _logger.LogWarning($"Setting StaleTvShowUpdateFrequencyInDays not found using default value {staleTvShowUpdateFrequency}.");
                }

                while (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Processing stale shows.");
                    await GetCompleteTvShowList(staleTvShowUpdateFrequency);
                    if (_completeShowList.Any())
                    {
                        await StartScraping();
                        _logger.LogInformation($"Stale shows processed. Sleeping for a {staleTvShowUpdateFrequency}.");
                        await GetMinutesFromStaleTvShowUpdateFrequency(staleTvShowUpdateFrequency, stoppingToken);
                    }
                    else
                    {
                        _logger.LogInformation($"No Stale shows found. Sleeping for a {staleTvShowUpdateFrequency}.");
                        await GetMinutesFromStaleTvShowUpdateFrequency(staleTvShowUpdateFrequency, stoppingToken);
                    }
                }

            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Scrapes Canceled!");
            }
            finally
            {
                await Finalize();
            }
        }

        private async Task DelayForAHour(CancellationToken stoppingToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(3600), stoppingToken);
        }

        private async Task GetMinutesFromStaleTvShowUpdateFrequency(StaleTvShowUpdate staleTvShowUpdate, CancellationToken stoppingToken)
        {
            switch (staleTvShowUpdate)
            {
                case StaleTvShowUpdate.Month:
                    for (int i = 0; i < 730; i++)
                    {
                        await DelayForAHour(stoppingToken);
                    }
                    break;
                case StaleTvShowUpdate.Week:
                    for (int i = 0; i < 168; i++)
                    {
                        await DelayForAHour(stoppingToken);
                    }
                    break;
                case StaleTvShowUpdate.None:
                    _logger.LogCritical("StaleTvShowUpdateFrequency should not be set on None, it is for testing only!!");
                    await Task.Delay(1000);
                    break;
                case StaleTvShowUpdate.Day:
                    for (int i = 0; i < 24; i++)
                    {
                        await DelayForAHour(stoppingToken);
                    }
                    break;
            }
        }

        private async Task StartScraping()
        {
            //Get a certain amount of scrapes using the TvShowScrapeLimit setting
            int tvShowScrapeLimit;
            if (!int.TryParse(_configuration["TvShowScrapeLimit"], out tvShowScrapeLimit))
            {
                tvShowScrapeLimit = 10;
                _logger.LogWarning($"Setting TvShowScrapeLimit not found using default value {tvShowScrapeLimit}.");
            }
            _scrapeList = await GetScrapes(tvShowScrapeLimit);
            while (_scrapeList.Any())
            {
                foreach (var scrape in _scrapeList.ToList())
                {
                    await ScrapeTvShow(scrape);
                    _scrapeList.Remove(scrape);
                }
                _scrapeList = await GetScrapes(tvShowScrapeLimit);
            }
        }

        [ExcludeFromCodeCoverage(Justification = "Can not cover day, week month delay in unit tests")]
        private async Task GetCompleteTvShowList(StaleTvShowUpdate? staleTvShowUpdate)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"updates/shows{(staleTvShowUpdate == null ? "" : $"?since={staleTvShowUpdate.ToString().ToLower()}")}");

            var client = _httpClientFactory.CreateClient("ScrapeHttpClient");

            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"TvShow service resonded with error {response.StatusCode}.");
            }
            else
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var updateShowsDictionary = JsonConvert.DeserializeObject<Dictionary<int, int>>(responseString) ?? new Dictionary<int, int>();
                _completeShowList = updateShowsDictionary.Select(x => new ShowUpdateDto { Id = x.Key, LastUpdated = DateTimeOffset.FromUnixTimeMilliseconds(x.Value).DateTime }).ToList();
            }
        }

        private async Task ScrapeTvShow(Scrape scrape)
        {
            try
            {
                _logger.LogInformation($"Scraping TvShow with Id : {scrape.TvShowId}");

                //Get TvShow
                var tvShowRequest = new HttpRequestMessage(HttpMethod.Get, $"shows/{scrape.TvShowId}");

                var client = _httpClientFactory.CreateClient("ScrapeHttpClient");

                var tvShowResponse = await client.SendAsync(tvShowRequest);

                if (!tvShowResponse.IsSuccessStatusCode)
                {
                    _logger.LogError($"TvShow service resonded with error {tvShowResponse.StatusCode}.");
                }
                else
                {
                    var tvShowResponseString = await tvShowResponse.Content.ReadAsStringAsync();
                    var tvShow = JsonConvert.DeserializeObject<TvShow>(tvShowResponseString);
                    if (tvShow != null)
                    {
                        _repositoryWrapper.TvShowRepository.AddOrUpdate(tvShow);
                        await _repositoryWrapper.SaveAsync();

                        //Get cast members
                        var castRequest = new HttpRequestMessage(HttpMethod.Get, $"shows/{scrape.TvShowId}/cast");

                        var castResponse = await client.SendAsync(castRequest);
                        if (!castResponse.IsSuccessStatusCode)
                        {
                            _logger.LogError($"TvShow service resonded with error {tvShowResponse.StatusCode} when getting cast member.");
                        }
                        else
                        {
                            var castResponseString = await castResponse.Content.ReadAsStringAsync();
                            var castMembers = JsonConvert.DeserializeObject<List<CastMembersDto>>(castResponseString, new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-dd" });
                            if (castMembers != null && castMembers.Any())
                            {
                                foreach (var castMember in castMembers)
                                {
                                    castMember.CastMember.TvShows.Add(tvShow);
                                    if (castMember?.CastMember != null)
                                    {
                                        _repositoryWrapper.CastMemberRepository.AddOrUpdate(castMember.CastMember);
                                        await _repositoryWrapper.SaveAsync();
                                    }
                                }
                            }
                        }
                        _logger.LogInformation($"TvShow with Id {scrape.TvShowId} scraped Successfully.");
                        _repositoryWrapper.ScrapesRepository.RemoveScrape(scrape);
                    }
                    else
                    {
                        _logger.LogInformation($"TvShow with Id {scrape.TvShowId} not found. Removing Scrape.");
                        _repositoryWrapper.ScrapesRepository.RemoveScrape(scrape);
                    }
                    await _repositoryWrapper.SaveAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Scrape with Id {scrape.TvShowId} threw an error. It will be rescraped. Error : {ex.Message}");
                _repositoryWrapper.TvShowRepository.RemoveById(scrape.TvShowId);
                _repositoryWrapper.ScrapesRepository.RemoveScrape(scrape);
            }
        }

        private async Task<List<Scrape>> GetScrapes(int tvShowScrapeLimit)
        {
            var scrapeList = _repositoryWrapper.ScrapesRepository.GetScrapes()?.Select(x => x.TvShowId).ToList() ?? new List<int>();

            var tvShowList = _repositoryWrapper.TvShowRepository.GetTvShowIds()?.ToList() ?? new List<int>();

            var newScrapeList =  _completeShowList
                .Where(x => !scrapeList.Contains(x.Id))?
                .Where(x => !tvShowList.Contains(x.Id))?
                .Take(tvShowScrapeLimit)?
                .Select(x => new Scrape
                {
                    Id = x.Id,
                    TvShowId = x.Id,
                    ScrapeDate = DateTime.Now
                }).ToList() ?? new List<Scrape>();
            foreach (var scrape in newScrapeList)
            {
                _repositoryWrapper.ScrapesRepository.AddOrUpdate(scrape);
                await _repositoryWrapper.SaveAsync();
            }
            return newScrapeList;
        }

        protected async Task Finalize()
        {
            if (_scrapeList.Any())
            {
                //Reomve Scrapes.
                _repositoryWrapper.ScrapesRepository.RemoveScrapes(_scrapeList);
                _ = await _repositoryWrapper.SaveAsync();
            }
        }
    }
}