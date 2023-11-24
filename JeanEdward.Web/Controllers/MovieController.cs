using jean_edwards.Interfaces;
using jean_edwards.Model;
using Microsoft.AspNetCore.Mvc;

namespace jean_edwards.Controllers;

[ApiController]
[Route("api/movies")]
public class MovieController : ControllerBase
{
    private readonly ILogger<MovieController> _logger;
    private readonly IMovieServices _movieService;

    public MovieController(ILogger<MovieController> logger, IMovieServices movieService)
    {
        _logger = logger;
        _movieService = movieService;
    }

    [HttpGet("search/{title}")]
    public async Task<MovieModel> SearchMovies(string title, [FromQuery] MovieQuery query)
    {
        return await _movieService.SearchMovies(title, query);
    }


    [HttpGet("results")]
    public async Task<List<SearchResultResponse>> GetMovieSearchResults() => await _movieService.GetMovieSearchResults();
}
