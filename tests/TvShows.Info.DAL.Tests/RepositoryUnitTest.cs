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
        public void RepositoryAddOrUpdateCastMember()
        {
            // Arrange
            var mock = CastMemberRepositoryMock.GetMock();

            var castMember = new CastMember
            {
                Id = 12,
                Name = "Test Member",
                Bitrthday = DateTime.Now.AddYears(-25)
            };

            // Act
            var castMemberResult = mock.Object.AddOrUpdate(castMember);

            //Assert
            Assert.AreEqual(castMemberResult?.Id, castMember.Id);
            
        }

        [TestMethod]
        public void RepositoryAddTvShow()
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
    }
}