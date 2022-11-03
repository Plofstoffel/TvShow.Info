using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using TvShows.Info.DAL.Context.Models;
using TvShows.Info.DAL.Tests.Helpers;

namespace TvShows.Info.DAL.Tests
{
    [TestClass]
    public class RepositoryUnitTest
    {
        [TestMethod]
        public void RepositoryAddCastMember()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<TvShowDbContext>()
            .UseInMemoryDatabase(databaseName: "MovieListDatabase")
            .Options;

            var castMember = new CastMember
            {
                Id = 1,
                Name = "Test Member",
                Bitrthday = DateTime.Now.AddYears(-25)
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
                Assert.AreEqual(castMemberResult?.Bitrthday, castMember.Bitrthday);
            }
        }

        [TestMethod]
        public void RepositoryAddTvShow()
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
                Cast = cast,
                LastUpdates = DateTime.Now
            };

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
                var tvShowResult = context.TvShows.Include(c => c.Cast).FirstOrDefault();
                Assert.AreEqual(tvShowResult?.Id, tcShow.Id);
                Assert.AreEqual(tvShowResult?.Name, tcShow.Name);
                Assert.AreEqual(tvShowResult?.Cast?.Count(), tcShow.Cast.Count());
                Assert.AreEqual(tvShowResult?.LastUpdates, tcShow.LastUpdates);
            }
        }

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
        public async Task RepositoryWrapperTest()
        {
            // Arrange
            var mock = RepositoryWrapperMock.GetMock();

            // Act

            var saveResult = await mock.Object.SaveAsync();

            //Assert
            Assert.IsNotNull(mock.Object.TvShowRepository);
            Assert.IsNotNull(mock.Object.CastMemberRepository);
            Assert.AreEqual(1, saveResult);

        }
    }
}