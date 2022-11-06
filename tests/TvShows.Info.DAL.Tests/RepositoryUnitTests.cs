using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using TvShows.Info.DAL.Context.Models;
using TvShows.Info.DAL.Models;
using TvShows.Info.DAL.Repository;
using TvShows.Info.DAL.Tests.Helpers;

namespace TvShows.Info.DAL.Tests
{
    [TestClass]
    public class RepositoryUnitTests
    {
        [TestMethod]
        public void CastMemberRepositoryAddOrUpdateCastMember()
        {
            // Arrange
            var mock = CastMemberRepositoryMock.GetMock();

            var castMember = new CastMember
            {
                Id = 1,
                Name = "Test Member",
                Bitrthday = DateTime.Now.AddYears(-25)
            };

            // Act
            var castMemberResult = mock.Object.AddOrUpdate(castMember);

            //Assert
            Assert.AreEqual(castMemberResult?.Id, castMember.Id);

        }

        [TestMethod]
        public void TvShowRepositoryAddOrUpdateTvShow()
        {
            // Arrange
            var mock = TvShowRepositoryMock.GetMock();

            var cast = new List<CastMember>();

            var tom = new CastMember
            {
                Id = 12,
                Name = "Tom Member",
                Bitrthday = DateTime.Now.AddYears(-25)
            };

            var dick = new CastMember
            {
                Id = 13,
                Name = "Dick Member",
                Bitrthday = DateTime.Now.AddYears(-25)
            };

            var harry = new CastMember
            {
                Id = 14,
                Name = "Harry Member",
                Bitrthday = DateTime.Now.AddYears(-25)
            };

            cast.Add(tom);
            cast.Add(dick);
            cast.Add(harry);

            var tcShow = new TvShow
            {
                Id = 1432,
                Name = "3 Old Men",
                Cast = cast
            };

            // Act
            var tvShowResult = mock.Object.AddOrUpdate(tcShow);

            //Assert
            Assert.AreEqual(tvShowResult?.Id, tcShow.Id);

        }

        [TestMethod]
        public void TvShowRepositoryGetOutDatedTvShows()
        {
            // Arrange
            var mock = TvShowRepositoryMock.GetMock();

            // Act
            var tvShows = mock.Object.GetOutDatedTvShows(DateTime.Now);

            //Assert
            Assert.AreEqual("Invader Zim 0", tvShows.FirstOrDefault()?.Name);

        }

        [TestMethod]
        public void TvShowRepositoryGetTvShows()
        {
            // Arrange
            var mock = TvShowRepositoryMock.GetMock();

            // Act
            var tvShows = mock.Object.GetTvShows(3, 1);

            //Assert
            Assert.AreEqual(3, tvShows.Count());
            Assert.AreEqual("Invader Zim 0", tvShows.FirstOrDefault()?.Name);

        }

        [TestMethod]
        public async Task RepositoryWrapperTest()
        {
            // Arrange
            var mock = RepositoryWrapperMock.GetMock();

            // Act

            var saveResult = await mock.Object.SaveAsync();

            //Assert
            Assert.IsNotNull(mock.Object.TvShowRepository);
            Assert.IsNotNull(mock.Object.CastMemberRepository);
            Assert.IsNotNull(mock.Object.ScrapesRepository);
            Assert.AreEqual(1, saveResult);

        }

        [TestMethod]
        public async Task RepositoryWrapperAddOrUpdateTvShow()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<TvShowDbContext>()
            .UseInMemoryDatabase(databaseName: "MovieListDatabase")
            .EnableSensitiveDataLogging()
            .Options;

            var context = new TvShowDbContext(options, new NullLoggerFactory());

            var sut = new RepositoryWrapper(context);

            var castMember = new CastMember
            {
                Id = 1,
                Name = "Test Member",
                Bitrthday = DateTime.Now.AddYears(-25)
            };

            var tvShow = new TvShow
            {
                Id = 1432,
                Name = "3 Old Men",
                Cast = new List<CastMember>
                {
                    castMember
                }
            };

            // Act
            sut.CastMemberRepository.AddOrUpdate(castMember);
            var saveResult1 = await sut.SaveAsync();

            sut.TvShowRepository.AddOrUpdate(tvShow);
            var saveResult2 = await sut.SaveAsync();

            sut.TvShowRepository.AddOrUpdate(tvShow);
            var saveResult3 = await sut.SaveAsync();

            // Assert
            Assert.AreEqual(1, saveResult1);
            Assert.AreEqual(2, saveResult2);
            Assert.AreEqual(1, saveResult3);

        }

        [TestMethod]
        public async Task RepositoryWrapperGetOutDatedTvShows()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<TvShowDbContext>()
            .UseInMemoryDatabase(databaseName: "MovieListDatabase")
            .Options;

            var context = new TvShowDbContext(options, new NullLoggerFactory());

            var sut = new RepositoryWrapper(context);

            var castMember = new CastMember
            {
                Id = 1,
                Name = "Test Member",
                Bitrthday = DateTime.Now.AddYears(-25)
            };

            var tvShow = new TvShow
            {
                Id = 1437,
                Name = "3 Old Men",
                Cast = new List<CastMember>
                {
                    castMember
                },
                LastUpdated = DateTime.Now.AddDays(-3)
            };

            // Act
            sut.CastMemberRepository.AddOrUpdate(castMember);
            _ = await sut.SaveAsync();

            sut.TvShowRepository.AddOrUpdate(tvShow);
            _ = await sut.SaveAsync();

            sut.TvShowRepository.AddOrUpdate(tvShow);
            _ = await sut.SaveAsync();

            var result = sut.TvShowRepository.GetOutDatedTvShows(DateTime.Now.AddDays(-2));

            // Assert
            Assert.AreEqual(1, result.Count());

        }

        [TestMethod]
        public async Task RepositoryWrapperGetTvShows()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<TvShowDbContext>()
            .UseInMemoryDatabase(databaseName: "MovieListDatabase")
            .EnableSensitiveDataLogging()
            .Options;

            var context = new TvShowDbContext(options, new NullLoggerFactory());

            context.Database.EnsureDeleted();

            var sut = new RepositoryWrapper(context);

            var castMember = new CastMember
            {
                Id = 1,
                Name = "Test Member",
                Bitrthday = DateTime.Now.AddYears(-25)
            };

            var tvShow1 = new TvShow
            {
                Id = 1422,
                Name = "3 Old Men",
                Cast = new List<CastMember>
                {
                    castMember
                },
                LastUpdated = DateTime.Now.AddDays(-3)
            };

            var tvShow2 = new TvShow
            {
                Id = 1423,
                Name = "3 Older Men",
                Cast = new List<CastMember>
                {
                    castMember
                },
                LastUpdated = DateTime.Now.AddDays(-3)
            };

            // Act
            sut.CastMemberRepository.AddOrUpdate(castMember);
            _ = await sut.SaveAsync();

            sut.TvShowRepository.AddOrUpdate(tvShow1);
            _ = await sut.SaveAsync();

            sut.TvShowRepository.AddOrUpdate(tvShow2);
            _ = await sut.SaveAsync();

            var result = sut.TvShowRepository.GetTvShows(10,1);

            // Assert
            Assert.AreEqual(2, result.Count());

        }

        [TestMethod]
        public async Task RepositoryWrapperGetAllTvShowIds()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<TvShowDbContext>()
            .UseInMemoryDatabase(databaseName: "MovieListDatabase")
            .EnableSensitiveDataLogging()
            .Options;

            var context = new TvShowDbContext(options, new NullLoggerFactory());

            context.Database.EnsureDeleted();

            var sut = new RepositoryWrapper(context);

            var castMember = new CastMember
            {
                Id = 1,
                Name = "Test Member",
                Bitrthday = DateTime.Now.AddYears(-25)
            };

            var tvShow1 = new TvShow
            {
                Id = 1422,
                Name = "3 Old Men",
                Cast = new List<CastMember>
                {
                    castMember
                },
                LastUpdated = DateTime.Now.AddDays(-3)
            };

            var tvShow2 = new TvShow
            {
                Id = 9999,
                Name = "99 Older Men",
                Cast = new List<CastMember>
                {
                    castMember
                },
                LastUpdated = DateTime.Now.AddDays(-3)
            };

            // Act
            sut.CastMemberRepository.AddOrUpdate(castMember);
            _ = await sut.SaveAsync();

            sut.TvShowRepository.AddOrUpdate(tvShow1);
            _ = await sut.SaveAsync();

            sut.TvShowRepository.AddOrUpdate(tvShow2);
            _ = await sut.SaveAsync();

            var result = sut.TvShowRepository.GetTvShowIds();

            // Assert
            Assert.AreEqual(2, result.Count());
            Assert.IsTrue(result.Contains(tvShow1.Id));
            Assert.IsTrue(result.Contains(tvShow2.Id));

        }

        [TestMethod]
        public async Task RepositoryWrapperAddScrapes()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<TvShowDbContext>()
            .UseInMemoryDatabase(databaseName: "MovieListDatabase")
            .EnableSensitiveDataLogging()
            .Options;

            var context = new TvShowDbContext(options, new NullLoggerFactory());

            var sut = new RepositoryWrapper(context);

            var scrape = new Scrape
            {
                Id = 12,
                ScrapeDate = DateTime.Now,
                TvShowId = 1432
            };

            // Act
            sut.ScrapesRepository.AddOrUpdate(scrape);
            var saveResult1 = await sut.SaveAsync();

            // Assert
            Assert.AreEqual(1, saveResult1);

        }

        [TestMethod]
        public async Task RepositoryWrapperRemoveScrape()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<TvShowDbContext>()
            .UseInMemoryDatabase(databaseName: "MovieListDatabase")
            .EnableSensitiveDataLogging()
            .Options;

            var context = new TvShowDbContext(options, new NullLoggerFactory());

            context.Database.EnsureDeleted();

            var sut = new RepositoryWrapper(context);

            var scrape1 = new Scrape
            {
                Id = 12,
                ScrapeDate = DateTime.Now,
                TvShowId = 1432
            };
            var scrape2 = new Scrape
            {
                Id = 13,
                ScrapeDate = DateTime.Now,
                TvShowId = 1432
            };
            var scrapes = new List<Scrape> { scrape1, scrape2 };

            // Act
            sut.ScrapesRepository.AddOrUpdate(scrape1);
            sut.ScrapesRepository.AddOrUpdate(scrape2);
            var saveResult1 = await sut.SaveAsync();
            sut.ScrapesRepository.RemoveScrape(scrape1);
            var saveResult2 = await sut.SaveAsync();

            var scrapeResult = sut.ScrapesRepository.GetScrapes();

            // Assert
            Assert.AreEqual(2, saveResult1);
            Assert.AreEqual(1, saveResult2);
            Assert.AreEqual(1, scrapeResult.Count());
        }

        [TestMethod]
        public async Task RepositoryWrapperRemoveScrapes()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<TvShowDbContext>()
            .UseInMemoryDatabase(databaseName: "MovieListDatabase")
            .EnableSensitiveDataLogging()
            .Options;

            var context = new TvShowDbContext(options, new NullLoggerFactory());

            context.Database.EnsureDeleted();

            var sut = new RepositoryWrapper(context);

            var scrape1 = new Scrape
            {
                Id = 12,
                ScrapeDate = DateTime.Now,
                TvShowId = 1432
            };
            var scrape2 = new Scrape
            {
                Id = 13,
                ScrapeDate = DateTime.Now,
                TvShowId = 1432
            };
            var scrapes = new List<Scrape> { scrape1, scrape2 };

            // Act
            sut.ScrapesRepository.AddOrUpdate(scrape1);
            sut.ScrapesRepository.AddOrUpdate(scrape2);
            var saveResult1 = await sut.SaveAsync();
            sut.ScrapesRepository.RemoveScrapes(scrapes);
            var saveResult2 = await sut.SaveAsync();

            var scrapeResult = sut.ScrapesRepository.GetScrapes();

            // Assert
            Assert.AreEqual(2, saveResult1);
            Assert.AreEqual(2, saveResult2);
            Assert.AreEqual(0, scrapeResult.Count());
        }

        [TestMethod]
        public async Task RepositoryWrapperGetScrapesInProgressTvShow()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<TvShowDbContext>()
            .UseInMemoryDatabase(databaseName: "MovieListDatabase")
            .EnableSensitiveDataLogging()
            .Options;

            var context = new TvShowDbContext(options, new NullLoggerFactory());

            var sut = new RepositoryWrapper(context);

            var scrape1 = new Scrape
            {
                Id = 1,
                ScrapeDate = DateTime.Now,
                TvShowId = 1432
            };

            var scrape2 = new Scrape
            {
                Id = 2,
                ScrapeDate = DateTime.Now,
                TvShowId = 1433
            };

            // Act
            sut.ScrapesRepository.AddOrUpdate(scrape1);
            sut.ScrapesRepository.AddOrUpdate(scrape2);
            var saveResult1 = await sut.SaveAsync();

            var scrapes = sut.ScrapesRepository.GetScrapes();

            // Assert
            Assert.AreEqual(2, saveResult1);
            Assert.IsTrue(scrapes.Any(x => x.Id == scrape1.Id));
            Assert.IsTrue(scrapes.Any(x => x.Id == scrape2.Id));

        }

    }
}