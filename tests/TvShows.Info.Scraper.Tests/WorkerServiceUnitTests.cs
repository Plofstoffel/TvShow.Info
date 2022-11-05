using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Newtonsoft.Json.Serialization;
using System.Net;
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

        [DataTestMethod]
        [DataRow("Day")]
        [DataRow("Week")]
        [DataRow("Month")]
        [DataRow("None")]
        public async Task StartScrapingWithSettingstaleTvShowUpdateFrequencyInDaysWorks(string staleTvShowUpdateFrequencyInDays)
        {
            //Arrange
            _inMemorySettings = new Dictionary<string, string> {
                {"StaleTvShowUpdateFrequency", staleTvShowUpdateFrequencyInDays},
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
            repoWrapper.Verify(c => c.SaveAsync(), Times.AtLeastOnce);
            _loggerMock.Verify(
                m => m.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => v.ToString().Contains("Stale shows")),
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
            repoWrapper.Verify(c => c.SaveAsync(), Times.AtLeastOnce);

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
            services.AddSingleton<IConfiguration>(GetConfiguration());
            services.AddSingleton(GetRepositoryWrapperMock().Object);
            services.AddSingleton(_loggerMock.Object);
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
            mockHttpMessageHandler.Protected()
                .SetupSequence<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = httpStatusCode,
                    Content = new StringContent("{\"1\":1631010933,\"2\":1631010933}"),
                })
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = httpStatusCode,
                    Content = new StringContent("{\"1\":1631010933,\"2\":1631010933}"),
                })
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = httpStatusCode,
                    Content = new StringContent("{}"),
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
                .Returns(_scrapeList.Skip(1).AsQueryable())
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

                }.AsQueryable());



            var mockTvShows = new Mock<ITvShowRepository>();
            mockTvShows.SetupSequence(x => x.GetTvShowIds())
                .Returns(new List<int>().AsQueryable())
                .Returns(new List<int> { 1, 2 }.AsQueryable());



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
    }
}