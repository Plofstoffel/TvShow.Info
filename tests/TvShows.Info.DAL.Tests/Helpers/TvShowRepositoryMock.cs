using Moq;
using TvShows.Info.DAL.Context.Models;
using TvShows.Info.DAL.Repository;

namespace TvShows.Info.DAL.Tests.Helpers
{
    public static class TvShowRepositoryMock
    {
        public static Mock<ITvShowRepository> GetMock()
        {
            var mock = new Mock<ITvShowRepository>();
            var show = new TvShow()
            {
                Id = 1,
                Name = "Invader Zim",
                Cast = new List<CastMember>()
                    {
                        new CastMember()
                        {
                            Id = 1,
                            Name = "Zim",
                            Bitrthday = DateTime.Now.AddYears(-25)
                        }
                    }
            };
            var shows = new List<TvShow>()
            {
                show
            };

            mock.Setup(m => m.AddOrUpdate(It.IsAny<TvShow>()))
                .Returns<TvShow>(x => x);

            return mock;
        }
    }

}
