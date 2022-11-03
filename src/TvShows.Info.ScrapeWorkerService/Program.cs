using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Diagnostics.CodeAnalysis;
using TvShows.Info.DAL;
using TvShows.Info.DAL.Repository;

namespace TvShows.Info.ScrapeWorkerService
{
    [ExcludeFromCodeCoverage(Justification = "Must be covered with integration tests")]
    public static class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateBootstrapLogger();

            Log.Information("Starting up");

            try
            {
                IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.AddDbContext<TvShowDbContext>(
                    options => options.UseSqlServer("name=ConnectionStrings:TvShowDbContext")
                    );
                    services.AddScoped<IRepositoryWrapper, RepositoryWrapper>();
                    services.AddHostedService<Worker>();
                })
                .UseSerilog((ctx, lc) => lc
                .WriteTo.Console()
                .ReadFrom.Configuration(ctx.Configuration))
                .Build();

                host.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Unhandled exception");
            }
            finally
            {
                Log.Information("Shut down complete");
                Log.CloseAndFlush();
            }
        }
    }
}