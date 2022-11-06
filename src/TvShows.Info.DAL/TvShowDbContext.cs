using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TvShows.Info.DAL.Context.Models;
using TvShows.Info.DAL.Models;

namespace TvShows.Info.DAL
{
    public class TvShowDbContext : DbContext
    {
        public readonly ILoggerFactory _loggerFactory;

        public TvShowDbContext(DbContextOptions<TvShowDbContext> options, ILoggerFactory loggerFactory)
            : base(options)
        {
            _loggerFactory = loggerFactory;

            this.Database.EnsureCreated();

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Scrape>()
                .HasKey(c => c.Id);
            modelBuilder.Entity<Scrape>()
                .Property(c => c.Id)
                .ValueGeneratedNever();
            modelBuilder.Entity<Scrape>()
                .Property(c => c.ScrapeDate)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<TvShow>()
                .HasKey(c => c.Id);
            modelBuilder.Entity<TvShow>()
                .Property(c => c.Id)
                .ValueGeneratedNever();
            modelBuilder.Entity<TvShow>()
                .Property(c => c.LastUpdated)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<CastMember>()
                .HasKey(c => c.Id);
            modelBuilder.Entity<CastMember>()
                .Property(c => c.Id)
                .ValueGeneratedNever();
        }

        public DbSet<TvShow> TvShows => Set<TvShow>();

        public DbSet<CastMember> CastMembers => Set<CastMember>();

        public DbSet<Scrape> Scrapes => Set<Scrape>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseLoggerFactory(_loggerFactory);
    }
}
