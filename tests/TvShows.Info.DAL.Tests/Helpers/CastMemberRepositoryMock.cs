using Moq;
using TvShows.Info.DAL.Context.Models;
using TvShows.Info.DAL.Repository;

namespace TvShows.Info.DAL.Tests.Helpers
{
    public static class CastMemberRepositoryMock
    {
        public static Mock<ICastMemberRepository> GetMock()
        {
            var mock = new Mock<ICastMemberRepository>();
            var castMember = new CastMember()
            {
                Id = 1,
                Name = "Zim",
                Birthday = DateTime.Now.AddYears(-25)

            };

            mock.Setup(m => m.AddOrUpdate(It.IsAny<CastMember>()))
                    .Returns<CastMember>(x => x);

            return mock;
        }
    }
}
