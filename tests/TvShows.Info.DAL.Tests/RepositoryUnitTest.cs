using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using TvShows.Info.DAL.Context.Models;

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
            }
        }
    }
}