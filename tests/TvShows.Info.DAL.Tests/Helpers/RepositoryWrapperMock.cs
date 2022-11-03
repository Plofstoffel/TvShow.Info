using Moq;
using TvShows.Info.DAL.Repository;

namespace TvShows.Info.DAL.Tests.Helpers
{
    public static class RepositoryWrapperMock
    {
        public static Mock<IRepositoryWrapper> GetMock()
        {
            var mock = new Mock<IRepositoryWrapper>();

            mock.Setup(m => m.TvShowRepository).Returns(() => new Mock<ITvShowRepository>().Object);
            mock.Setup(m => m.CastMemberRepository).Returns(() => new Mock<ICastMemberRepository>().Object);
            mock.Setup(m => m.SaveAsync()).ReturnsAsync(1);

            return mock;
        }
    }
}
