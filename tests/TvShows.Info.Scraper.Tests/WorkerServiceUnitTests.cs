using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Protected;
using System.Net;
using TvShows.Info.DAL;
using TvShows.Info.DAL.Models;
using TvShows.Info.DAL.Repository;
using TvShows.Info.ScrapeWorkerService.Enums;

namespace TvShows.Info.ScrapeWorkerService.Tests
{
    [TestClass]
    public class WorkerServiceUnitTests
    {
        protected static IQueryable<Scrape> _scrapeList = new List<Scrape>().AsQueryable();
        protected static Dictionary<string, string>? _inMemorySettings;
        protected static Mock<ILogger<ScrapeWorker>>? _loggerMock;

        [TestMethod]
        public async Task StartScrapingWithNoMoreScrapesWorksAndLogs()
        {
            //Arrange
            _inMemorySettings = new Dictionary<string, string> {
                {"TvShowScrapeLimit", "15"}
            };
            ServiceProvider serviceProvider = GetServiceProvider(GetHttpClientFactory(HttpStatusCode.OK));

            var backgroundService = serviceProvider.GetService<IHostedService>() as ScrapeWorker;

            //Act
            await backgroundService.StartAsync(CancellationToken.None);
            await backgroundService.StopAsync(CancellationToken.None);

            //Assert
            _loggerMock.Verify(
                m => m.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => v.ToString().Contains("No more TvShows to scrape")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [TestMethod]
        public async Task StartScrapingWithFaultySettingsForTvShowScrapeWorks()
        {
            //Arrange
            _inMemorySettings = new Dictionary<string, string> {
                {"TvShowScrapeLimit", "NotANUmmericValue"},
                {"StaleTvShowUpdateFrequency", "SomeRandomValue"},
            };
            ServiceProvider serviceProvider = GetServiceProvider(GetHttpClientFactory(HttpStatusCode.OK));

            var backgroundService = serviceProvider.GetService<IHostedService>() as ScrapeWorker;

            //Act
            await backgroundService.StartAsync(CancellationToken.None);
            await backgroundService.StopAsync(CancellationToken.None);

            //Assert
            _loggerMock.Verify(
                m => m.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => v.ToString().Contains("Setting TvShowScrapeLimit not found ")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }

        [TestMethod]
        public async Task StartScrapingWithCancelationTokenCanceldDoesNothingButFinalize()
        {
            //Arrange
            ServiceProvider serviceProvider = GetServiceProvider(GetHttpClientFactory(HttpStatusCode.OK));

            var repoWrapper = Mock.Get(serviceProvider.GetService<IRepositoryWrapper>());
            var backgroundService = serviceProvider.GetService<IHostedService>() as ScrapeWorker;
            var cancelSource = new CancellationTokenSource();

            //Act
            cancelSource.Cancel();
            await backgroundService.StartAsync(cancelSource.Token);

            //Assert
            repoWrapper.Verify(c => c.ScrapesRepository.RemoveScrapes(It.IsAny<List<Scrape>>()), Times.Never);
            await backgroundService.StopAsync(cancelSource.Token);

        }

        [TestMethod]
        public async Task StartScrapingCancelationTokenSentLogsWarning()
        {
            //Arrange
            ServiceProvider serviceProvider = GetServiceProvider(GetHttpClientFactory(HttpStatusCode.OK));

            var backgroundService = serviceProvider.GetService<IHostedService>() as ScrapeWorker;
            var cancelSource = new CancellationTokenSource();

            //Act
            cancelSource.Cancel();
            await backgroundService.StartAsync(cancelSource.Token);

            //Assert
            _loggerMock.Verify(
                m => m.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => v.ToString().Contains("Scrapes Canceled!")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

        }

        [TestMethod]
        public async Task StartAndStopScrapingFinalizesAndRemovesScrapes()
        {
            //Arrange
            ServiceProvider serviceProvider = GetServiceProvider(GetHttpClientFactory(HttpStatusCode.OK));

            var repoWrapper = Mock.Get(serviceProvider.GetService<IRepositoryWrapper>());
            var backgroundService = serviceProvider.GetService<IHostedService>() as ScrapeWorker;
            var cancelSource = new CancellationTokenSource();

            //Act
            await backgroundService.StartAsync(cancelSource.Token);
            await backgroundService.StopAsync(cancelSource.Token);

            //Assert
            repoWrapper.Verify(c => c.ScrapesRepository.RemoveScrapes(It.IsAny<List<Scrape>>()), Times.Never);
        }

        [TestMethod]
        public async Task StartScrapingWithSomeScrapesWorks()
        {
            //Arrange
            _inMemorySettings = new Dictionary<string, string> {
                {"StaleTvShowUpdateFrequency", StaleTvShowUpdate.None.ToString()},
            };
            ServiceProvider serviceProvider = GetServiceProvider(GetHttpClientFactory(HttpStatusCode.OK), false);

            var repoWrapper = Mock.Get(serviceProvider.GetService<IRepositoryWrapper>());
            var backgroundService = serviceProvider.GetService<IHostedService>() as ScrapeWorker;
            var cancelSource = new CancellationTokenSource();

            //Act
            await backgroundService.StartAsync(cancelSource.Token);
            await Task.Delay(1000);
            await backgroundService.StopAsync(cancelSource.Token);

            //Assert
            _loggerMock.Verify(
                m => m.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => v.ToString().Contains("Scraping TvShow with Id")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);

        }

        [TestMethod]
        public async Task StartScrapingButCantReachTvShowServiceLogsAnError()
        {
            //Arrange
            ServiceProvider serviceProvider = GetServiceProvider(GetHttpClientFactory(HttpStatusCode.ServiceUnavailable));

            var backgroundService = serviceProvider.GetService<IHostedService>() as ScrapeWorker;
            var cancelSource = new CancellationTokenSource();

            //Act
            await backgroundService.StartAsync(cancelSource.Token);
            await Task.Delay(1000);
            await backgroundService.StopAsync(cancelSource.Token);

            //Assert
            _loggerMock.Verify(
                m => m.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => v.ToString().Contains("TvShow service resonded with error")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);

        }


        private static ServiceProvider GetServiceProvider(IHttpClientFactory httpClientFactory, bool populateScrapeList = true)
        {
            if (populateScrapeList)
            {
                _scrapeList = new List<Scrape>()
                {
                    new Scrape
                    {
                        Id = 1,
                        ScrapeDate = DateTime.Now,
                        TvShowId = 1
                    },
                    new Scrape
                    {
                        Id = 2,
                        ScrapeDate = DateTime.Now,
                        TvShowId = 2
                    },
                    new Scrape
                    {
                        Id = 3,
                        ScrapeDate = DateTime.Now,
                        TvShowId = 3
                    },

                }.AsQueryable();
            }
            else
            {
                _scrapeList = new List<Scrape>().AsQueryable();
            }

            var options = new DbContextOptionsBuilder<TvShowDbContext>()
            .UseInMemoryDatabase(databaseName: "MovieListDatabase")
            .Options;

            var context = new TvShowDbContext(options, new NullLoggerFactory());
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            _loggerMock = new Mock<ILogger<ScrapeWorker>>();
            _loggerMock.Setup(x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                )
            );
            IServiceCollection services = new ServiceCollection();
            services.AddSingleton(_loggerMock.Object);
            services.AddSingleton<IConfiguration>(GetConfiguration());
            services.AddSingleton(GetRepositoryWrapperMock().Object);
            services.AddSingleton<TvShowDbContext>(context);
            services.AddHostedService<ScrapeWorker>();

            if (httpClientFactory == null)
            {
                httpClientFactory = GetHttpClientFactory(HttpStatusCode.OK);
            }
            services.AddSingleton(httpClientFactory);

            var serviceProvider = services.BuildServiceProvider();
            return serviceProvider;
        }

        private static IHttpClientFactory GetHttpClientFactory(HttpStatusCode httpStatusCode)
        {
            var mockFactory = new Mock<IHttpClientFactory>();
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            //Setup call to /updates/shows
            mockHttpMessageHandler.Protected()
                .SetupSequence<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.Is<HttpRequestMessage>(rm => rm.RequestUri.AbsoluteUri.EndsWith("/updates/shows")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = httpStatusCode,
                    Content = new StringContent("{\"1\":1631010933,\"2\":1631010934,\"3\":1631010934,\"4\":1631010934}"),
                });

            //Setup call to /updates/shows?since
            mockHttpMessageHandler.Protected()
                .SetupSequence<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.Is<HttpRequestMessage>(rm => rm.RequestUri.AbsoluteUri.Contains("/updates/shows?since=")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = httpStatusCode,
                    Content = new StringContent("{}"),
                });

            //Setup call to /shows/1
            mockHttpMessageHandler.Protected()
                .SetupSequence<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.Is<HttpRequestMessage>(rm => rm.RequestUri.AbsoluteUri.EndsWith("/shows/1")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = httpStatusCode,
                    Content = new StringContent(tvShowResponseForId1)
                });

            //Setup call to /shows/2
            mockHttpMessageHandler.Protected()
                .SetupSequence<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.Is<HttpRequestMessage>(rm => rm.RequestUri.AbsoluteUri.EndsWith("/shows/2")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = httpStatusCode,
                    Content = new StringContent(tvShowResponseForId2)
                });

            //Setup call to /shows/3
            mockHttpMessageHandler.Protected()
                .SetupSequence<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.Is<HttpRequestMessage>(rm => rm.RequestUri.AbsoluteUri.EndsWith("/shows/3")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = new StringContent("")
                });

            //Setup call to /shows/4
            mockHttpMessageHandler.Protected()
                .SetupSequence<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.Is<HttpRequestMessage>(rm => rm.RequestUri.AbsoluteUri.EndsWith("/shows/4")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = httpStatusCode,
                    Content = new StringContent(tvShowResponseForId4)
                });

            //Setup call to /shows/1/cast
            mockHttpMessageHandler.Protected()
                .SetupSequence<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.Is<HttpRequestMessage>(rm => rm.RequestUri.AbsoluteUri.EndsWith("/shows/1/cast")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = httpStatusCode,
                    Content = new StringContent(castMemberResponseForId1)
                });

            //Setup call to /shows/2/cast
            mockHttpMessageHandler.Protected()
                .SetupSequence<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.Is<HttpRequestMessage>(rm => rm.RequestUri.AbsoluteUri.EndsWith("/shows/2/cast")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = httpStatusCode,
                    Content = new StringContent(castMemberResponseForId2)
                });

            //Setup call to /shows/3/cast
            mockHttpMessageHandler.Protected()
                .SetupSequence<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.Is<HttpRequestMessage>(rm => rm.RequestUri.AbsoluteUri.EndsWith("/shows/4/cast")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = new StringContent("")
                });

            var client = new HttpClient(mockHttpMessageHandler.Object);
            client.BaseAddress = new Uri("http://localhost/");
            mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);
            return mockFactory.Object;
        }

        private static Mock<IRepositoryWrapper> GetRepositoryWrapperMock()
        {
            var mock = new Mock<IRepositoryWrapper>();

            var mockScrapes = new Mock<IScrapesRepository>();
            mockScrapes.SetupSequence(x => x.GetScrapes())
                .Returns(_scrapeList)
                .Returns(_scrapeList.Skip(1).AsQueryable())
                .Returns(_scrapeList.Skip(2).AsQueryable())
                .Returns(new List<Scrape>().AsQueryable())
                .Returns(new List<Scrape>()
                {
                    new Scrape
                    {
                        Id = 1,
                        ScrapeDate = DateTime.Now,
                        TvShowId = 1
                    },
                    new Scrape
                    {
                        Id = 2,
                        ScrapeDate = DateTime.Now,
                        TvShowId = 2
                    },
                    new Scrape
                    {
                        Id = 3,
                        ScrapeDate = DateTime.Now,
                        TvShowId = 3
                    },

                }.AsQueryable())
                .Returns(_scrapeList);

            var mockTvShows = new Mock<ITvShowRepository>();
            mockTvShows.SetupSequence(x => x.GetTvShowIds())
                .Returns(new List<int>().AsQueryable())
                .Returns(new List<int> { 1, 2, 3, 4 }.AsQueryable())
                .Returns(new List<int> { 1, 2, 4 }.AsQueryable());



            mock.Setup(m => m.TvShowRepository).Returns(() => mockTvShows.Object);
            mock.Setup(m => m.CastMemberRepository).Returns(() => new Mock<ICastMemberRepository>().Object);
            mock.Setup(m => m.ScrapesRepository).Returns(() => mockScrapes.Object);
            mock.Setup(m => m.SaveAsync()).ReturnsAsync(1);
            return mock;
        }

        private static IConfiguration GetConfiguration()
        {
            return new ConfigurationBuilder()
                .AddInMemoryCollection(_inMemorySettings)
                .Build();
        }

        #region Response Examples

        private const string tvShowResponseForId1 = "{\"id\":1,\"url\":\"https://www.tvmaze.com/shows/1/under-the-dome\",\"name\":\"Under the Dome\",\"type\":\"Scripted\",\"language\":\"English\",\"genres\":[\"Drama\",\"Science-Fiction\",\"Thriller\"],\"status\":\"Ended\",\"runtime\":60,\"averageRuntime\":60,\"premiered\":\"2013-06-24\",\"ended\":\"2015-09-10\",\"officialSite\":\"http://www.cbs.com/shows/under-the-dome/\",\"schedule\":{\"time\":\"22:00\",\"days\":[\"Thursday\"]},\"rating\":{\"average\":6.5},\"weight\":98,\"network\":{\"id\":2,\"name\":\"CBS\",\"country\":{\"name\":\"United States\",\"code\":\"US\",\"timezone\":\"America/New_York\"},\"officialSite\":\"https://www.cbs.com/\"},\"webChannel\":null,\"dvdCountry\":null,\"externals\":{\"tvrage\":25988,\"thetvdb\":264492,\"imdb\":\"tt1553656\"},\"image\":{\"medium\":\"https://static.tvmaze.com/uploads/images/medium_portrait/81/202627.jpg\",\"original\":\"https://static.tvmaze.com/uploads/images/original_untouched/81/202627.jpg\"},\"summary\":\"<p><b>Under the Dome</b> is the story of a small town that is suddenly and inexplicably sealed off from the rest of the world by an enormous transparent dome. The town's inhabitants must deal with surviving the post-apocalyptic conditions while searching for answers about the dome, where it came from and if and when it will go away.</p>\",\"updated\":1631010933,\"_links\":{\"self\":{\"href\":\"https://api.tvmaze.com/shows/1\"},\"previousepisode\":{\"href\":\"https://api.tvmaze.com/episodes/185054\"}}}";
        private const string tvShowResponseForId2 = "{\"id\":2,\"url\":\"https://www.tvmaze.com/shows/2/person-of-interest\",\"name\":\"Person of Interest\",\"type\":\"Scripted\",\"language\":\"English\",\"genres\":[\"Action\",\"Crime\",\"Science-Fiction\"],\"status\":\"Ended\",\"runtime\":60,\"averageRuntime\":60,\"premiered\":\"2011-09-22\",\"ended\":\"2016-06-21\",\"officialSite\":\"http://www.cbs.com/shows/person_of_interest/\",\"schedule\":{\"time\":\"22:00\",\"days\":[\"Tuesday\"]},\"rating\":{\"average\":8.8},\"weight\":97,\"network\":{\"id\":2,\"name\":\"CBS\",\"country\":{\"name\":\"United States\",\"code\":\"US\",\"timezone\":\"America/New_York\"},\"officialSite\":\"https://www.cbs.com/\"},\"webChannel\":null,\"dvdCountry\":null,\"externals\":{\"tvrage\":28376,\"thetvdb\":248742,\"imdb\":\"tt1839578\"},\"image\":{\"medium\":\"https://static.tvmaze.com/uploads/images/medium_portrait/163/407679.jpg\",\"original\":\"https://static.tvmaze.com/uploads/images/original_untouched/163/407679.jpg\"},\"summary\":\"<p>You are being watched. The government has a secret system, a machine that spies on you every hour of every day. I know because I built it. I designed the Machine to detect acts of terror but it sees everything. Violent crimes involving ordinary people. People like you. Crimes the government considered \\\"irrelevant\\\". They wouldn't act so I decided I would. But I needed a partner. Someone with the skills to intervene. Hunted by the authorities, we work in secret. You'll never find us. But victim or perpetrator, if your number is up, we'll find you.</p>\",\"updated\":1631565378,\"_links\":{\"self\":{\"href\":\"https://api.tvmaze.com/shows/2\"},\"previousepisode\":{\"href\":\"https://api.tvmaze.com/episodes/659372\"}}}";
        private const string tvShowResponseForId4 = "{\"id\":4,\"url\":\"https://www.tvmaze.com/shows/4/arrow\",\"name\":\"Arrow\",\"type\":\"Scripted\",\"language\":\"English\",\"genres\":[\"Drama\",\"Action\",\"Science-Fiction\"],\"status\":\"Ended\",\"runtime\":60,\"averageRuntime\":60,\"premiered\":\"2012-10-10\",\"ended\":\"2020-01-28\",\"officialSite\":\"http://www.cwtv.com/shows/arrow\",\"schedule\":{\"time\":\"21:00\",\"days\":[\"Tuesday\"]},\"rating\":{\"average\":7.4},\"weight\":98,\"network\":{\"id\":5,\"name\":\"The CW\",\"country\":{\"name\":\"United States\",\"code\":\"US\",\"timezone\":\"America/New_York\"},\"officialSite\":\"https://www.cwtv.com/\"},\"webChannel\":null,\"dvdCountry\":null,\"externals\":{\"tvrage\":30715,\"thetvdb\":257655,\"imdb\":\"tt2193021\"},\"image\":{\"medium\":\"https://static.tvmaze.com/uploads/images/medium_portrait/213/534017.jpg\",\"original\":\"https://static.tvmaze.com/uploads/images/original_untouched/213/534017.jpg\"},\"summary\":\"<p>After a violent shipwreck, billionaire playboy Oliver Queen was missing and presumed dead for five years before being discovered alive on a remote island in the Pacific. He returned home to Starling City, welcomed by his devoted mother Moira, beloved sister Thea and former flame Laurel Lance. With the aid of his trusted chauffeur/bodyguard John Diggle, the computer-hacking skills of Felicity Smoak and the occasional, reluctant assistance of former police detective, now beat cop, Quentin Lance, Oliver has been waging a one-man war on crime.</p>\",\"updated\":1652385053,\"_links\":{\"self\":{\"href\":\"https://api.tvmaze.com/shows/4\"},\"previousepisode\":{\"href\":\"https://api.tvmaze.com/episodes/1744752\"}}}";

        private const string castMemberResponseForId1 = "[{\"person\":{\"id\":1,\"url\":\"https://www.tvmaze.com/people/1/mike-vogel\",\"name\":\"Mike Vogel\",\"country\":{\"name\":\"United States\",\"code\":\"US\",\"timezone\":\"America/New_York\"},\"birthday\":\"1979-07-17\",\"deathday\":null,\"gender\":\"Male\",\"image\":{\"medium\":\"https://static.tvmaze.com/uploads/images/medium_portrait/0/1815.jpg\",\"original\":\"https://static.tvmaze.com/uploads/images/original_untouched/0/1815.jpg\"},\"updated\":1634211735,\"_links\":{\"self\":{\"href\":\"https://api.tvmaze.com/people/1\"}}},\"character\":{\"id\":1,\"url\":\"https://www.tvmaze.com/characters/1/under-the-dome-dale-barbie-barbara\",\"name\":\"Dale \\\"Barbie\\\" Barbara\",\"image\":{\"medium\":\"https://static.tvmaze.com/uploads/images/medium_portrait/0/3.jpg\",\"original\":\"https://static.tvmaze.com/uploads/images/original_untouched/0/3.jpg\"},\"_links\":{\"self\":{\"href\":\"https://api.tvmaze.com/characters/1\"}}},\"self\":false,\"voice\":false},{\"person\":{\"id\":2,\"url\":\"https://www.tvmaze.com/people/2/rachelle-lefevre\",\"name\":\"Rachelle Lefevre\",\"country\":{\"name\":\"Canada\",\"code\":\"CA\",\"timezone\":\"America/Halifax\"},\"birthday\":\"1979-02-01\",\"deathday\":null,\"gender\":\"Female\",\"image\":{\"medium\":\"https://static.tvmaze.com/uploads/images/medium_portrait/82/207417.jpg\",\"original\":\"https://static.tvmaze.com/uploads/images/original_untouched/82/207417.jpg\"},\"updated\":1656232858,\"_links\":{\"self\":{\"href\":\"https://api.tvmaze.com/people/2\"}}},\"character\":{\"id\":2,\"url\":\"https://www.tvmaze.com/characters/2/under-the-dome-julia-shumway\",\"name\":\"Julia Shumway\",\"image\":{\"medium\":\"https://static.tvmaze.com/uploads/images/medium_portrait/0/6.jpg\",\"original\":\"https://static.tvmaze.com/uploads/images/original_untouched/0/6.jpg\"},\"_links\":{\"self\":{\"href\":\"https://api.tvmaze.com/characters/2\"}}},\"self\":false,\"voice\":false}]";
        private const string castMemberResponseForId2 = "[{\"person\":{\"id\":3,\"url\":\"https://www.tvmaze.com/people/3/alexander-koch\",\"name\":\"Alexander Koch\",\"country\":{\"name\":\"United States\",\"code\":\"US\",\"timezone\":\"America/New_York\"},\"birthday\":\"1988-02-24\",\"deathday\":null,\"gender\":\"Male\",\"image\":{\"medium\":\"https://static.tvmaze.com/uploads/images/medium_portrait/205/513325.jpg\",\"original\":\"https://static.tvmaze.com/uploads/images/original_untouched/205/513325.jpg\"},\"updated\":1651887118,\"_links\":{\"self\":{\"href\":\"https://api.tvmaze.com/people/3\"}}},\"character\":{\"id\":3,\"url\":\"https://www.tvmaze.com/characters/3/under-the-dome-junior-rennie\",\"name\":\"Junior Rennie\",\"image\":{\"medium\":\"https://static.tvmaze.com/uploads/images/medium_portrait/0/10.jpg\",\"original\":\"https://static.tvmaze.com/uploads/images/original_untouched/0/10.jpg\"},\"_links\":{\"self\":{\"href\":\"https://api.tvmaze.com/characters/3\"}}},\"self\":false,\"voice\":false}]";

        #endregion

    }
}