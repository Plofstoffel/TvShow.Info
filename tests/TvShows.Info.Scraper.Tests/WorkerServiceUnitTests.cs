using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using TvShows.Info.DAL.Models;
using TvShows.Info.DAL.Repository;

namespace TvShows.Info.ScrapeWorkerService.Tests
{
    [TestClass]
    public class WorkerServiceUnitTests
    {

        [TestMethod]
        public async Task StartScrapingWithNoScrapesWorks()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(GetConfiguration());
            services.AddSingleton(GetRepositoryWrapperMock().Object);
            services.AddSingleton(Mock.Of<ILogger<ScrapeWorker>>());
            services.AddHostedService<ScrapeWorker>();

            var serviceProvider = services.BuildServiceProvider();

            var repoWrapper = Mock.Get(serviceProvider.GetService<IRepositoryWrapper>());
            var backgroundService = serviceProvider.GetService<IHostedService>() as ScrapeWorker;

            await backgroundService.StartAsync(CancellationToken.None);
            await Task.Delay(1000);

            repoWrapper.Verify(c => c.SaveAsync(), Times.AtLeastOnce);
            await backgroundService.StopAsync(CancellationToken.None);
        }

        [TestMethod]
        public async Task StartScrapingCancelationWithTokenWorks()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(GetConfiguration());
            services.AddSingleton(GetRepositoryWrapperMock().Object);
            services.AddSingleton(Mock.Of<ILogger<ScrapeWorker>>());
            services.AddHostedService<ScrapeWorker>();

            var serviceProvider = services.BuildServiceProvider();

            var repoWrapper = Mock.Get(serviceProvider.GetService<IRepositoryWrapper>());
            var backgroundService = serviceProvider.GetService<IHostedService>() as ScrapeWorker;
            var cancelSource = new CancellationTokenSource();

            await backgroundService.StartAsync(cancelSource.Token);
            await Task.Delay(1000);
            cancelSource.Cancel();
            await Task.Delay(1000);
            repoWrapper.Verify(c => c.ScrapesRepository.RemoveScrapes(It.IsAny<List<Scrape>>()), Times.AtLeastOnce);
            await backgroundService.StopAsync(cancelSource.Token);

        }

        [TestMethod]
        public async Task StartScrapingWithSomeScrapesWorks()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(GetConfiguration());
            services.AddSingleton(GetRepositoryWrapperMock(true).Object);
            services.AddSingleton(Mock.Of<ILogger<ScrapeWorker>>());
            services.AddHostedService<ScrapeWorker>();

            var serviceProvider = services.BuildServiceProvider();

            var repoWrapper = Mock.Get(serviceProvider.GetService<IRepositoryWrapper>());
            var backgroundService = serviceProvider.GetService<IHostedService>() as ScrapeWorker;

            await backgroundService.StartAsync(CancellationToken.None);
            await Task.Delay(5000);
            repoWrapper.Verify(c => c.SaveAsync(), Times.AtLeastOnce);
            await backgroundService.StopAsync(CancellationToken.None);

        }

        private static Mock<IRepositoryWrapper> GetRepositoryWrapperMock(bool addScrapes = false)
        {
            var mock = new Mock<IRepositoryWrapper>();

            var mockScrapes = new Mock<IScrapesRepository>();
            if (addScrapes)
            {
                mockScrapes.Setup(x => x.GetScrapes()).Returns(new List<Scrape>()
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
            }

            mock.Setup(m => m.TvShowRepository).Returns(() => new Mock<ITvShowRepository>().Object);
            mock.Setup(m => m.CastMemberRepository).Returns(() => new Mock<ICastMemberRepository>().Object);
            mock.Setup(m => m.ScrapesRepository).Returns(() => mockScrapes.Object);
            mock.Setup(m => m.SaveAsync()).ReturnsAsync(1);
            return mock;
        }

        private static IConfiguration GetConfiguration()
        {
            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile("appsettings.Development.json", true, true)
                .AddJsonFile("appsettings.local.json", true, true)
                .Build();
        }
    }
}