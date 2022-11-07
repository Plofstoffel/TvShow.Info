using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Mime;
using TvShows.Info.DAL.Repository;

namespace TvShows.Info.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Bad Request", Type = typeof(string))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal Server Error", Type = typeof(string))]
    public class TvShowController : ControllerBase
    {
        private readonly ILogger<TvShowController> _logger;
        private readonly IRepositoryWrapper _repositoryWrapper;

        public TvShowController(ILogger<TvShowController> logger, IRepositoryWrapper repositoryWrapper)
        {
            _logger = logger;
            _repositoryWrapper = repositoryWrapper;
        }

        [HttpGet]
        [Route("[action]")]
        [SwaggerOperation(
            Summary = "Determine if a reboot is allowed",
            Description = "Returns true if a reboot is allowed, false otherwise",
            OperationId = nameof(GetTvShows))]
        [SwaggerResponse(StatusCodes.Status200OK, "Success", Type = typeof(bool))]
        public IActionResult GetTvShows(int pageSize, int pageNumber)
        {
            _logger.LogInformation($"Retrieving {pageSize} TvShows from page {pageNumber}.");
            return Ok(_repositoryWrapper.TvShowRepository.GetTvShows(pageSize, pageNumber));
        }
    }
}
