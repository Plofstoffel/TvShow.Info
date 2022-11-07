using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using TvShows.Info.DAL.Context.Models;
using TvShows.Info.DAL.Models;

namespace TvShows.Info.DAL.Tests
{
    [TestClass]
    public class DbContextUnitTests
    {
        [TestMethod]
        public void DbContextAddCastMember()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<TvShowDbContext>()
            .UseInMemoryDatabase(databaseName: "MovieListDatabase")
            .Options;

            var castMember = new CastMember
            {
                Id = 1,
                Name = "Test Member",
                Birthday = DateTime.Now.AddYears(-25)
            };

            // Act
            using (var context = new TvShowDbContext(options, new NullLoggerFactory()))
            {
                context.Database.EnsureCreated();
                context.CastMembers.Add(castMember);
                context.SaveChanges();
            }

            //Assert
            using (var context = new TvShowDbContext(options, new NullLoggerFactory()))
            {
                var castMemberResult = context.CastMembers.FirstOrDefault();
                Assert.AreEqual(castMemberResult?.Id, castMember.Id);
                Assert.AreEqual(castMemberResult?.Name, castMember.Name);
                Assert.AreEqual(castMemberResult?.Birthday, castMember.Birthday);
            }
        }

        [TestMethod]
        public void DbContextAddTvShow()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<TvShowDbContext>()
            .UseInMemoryDatabase(databaseName: "MovieListDatabase")
            .Options;

            var cast = new List<CastMember>();

            var tom = new CastMember
            {
                Id = 12,
                Name = "Tom Member",
                Birthday = DateTime.Now.AddYears(-25)
            };

            var dick = new CastMember
            {
                Id = 13,
                Name = "Dick Member",
                Birthday = DateTime.Now.AddYears(-25)
            };

            var harry = new CastMember
            {
                Id = 14,
                Name = "Harry Member",
                Birthday = DateTime.Now.AddYears(-25)
            };

            cast.Add(tom);
            cast.Add(dick);
            cast.Add(harry);

            var tcShow = new TvShow
            {
                Id = 1432,
                Name = "3 Old Men"
            };

            tcShow.Cast.Add(tom);
            tcShow.Cast.Add(dick);
            tcShow.Cast.Add(harry);

            // Act
            using (var context = new TvShowDbContext(options, new NullLoggerFactory()))
            {
                context.Database.EnsureCreated();
                context.TvShows.Add(tcShow);
                context.SaveChanges();
            }

            //Assert
            using (var context = new TvShowDbContext(options, new NullLoggerFactory()))
            {
                var tvShowResult = context.TvShows.Include(x => x.Cast).FirstOrDefault();
                Assert.AreEqual(tvShowResult?.Id, tcShow.Id);
                Assert.AreEqual(tvShowResult?.Name, tcShow.Name);
                Assert.AreEqual(tvShowResult?.Cast?.Count(), tcShow.Cast.Count());
            }
        }

        [TestMethod]
        public void DbContextAddScrape()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<TvShowDbContext>()
            .UseInMemoryDatabase(databaseName: "MovieListDatabase")
            .Options;

            var scrape = new Scrape
            {
                ScrapeDate = DateTime.Now,
                TvShowId = 1
            };

            // Act
            using (var context = new TvShowDbContext(options, new NullLoggerFactory()))
            {
                context.Database.EnsureCreated();
                context.Scrapes.Add(scrape);
                context.SaveChanges();
            }

            //Assert
            using (var context = new TvShowDbContext(options, new NullLoggerFactory()))
            {
                var scrapeResult = context.Scrapes.FirstOrDefault();
                Assert.AreEqual(scrapeResult?.TvShowId, scrape.TvShowId);
                Assert.AreEqual(scrapeResult?.ScrapeDate, scrape.ScrapeDate);
            }
        }
    }
}
