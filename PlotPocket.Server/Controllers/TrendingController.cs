using Microsoft.AspNetCore.Mvc;
using PlotPocket.Server.Models.Dtos;
using PlotPocket.Server.Services;


namespace PlotPocket.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TrendingController : ControllerBase
    {
        private readonly ShowService _showService;

        public TrendingController(ShowService showService)
        {
            _showService = showService;
        }

        [HttpGet("test")]
        public async Task<ActionResult> TestConnection()
        {
            try
            {
                var client = new HttpClient();
                var response = await client.GetAsync("https://api.themoviedb.org/3/configuration?api_key=85063218333c20a38e3fc6f6c4daf5f9");
                response.EnsureSuccessStatusCode();
                return Ok("TMDB API is accessible");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Connection test failed", message = ex.Message });
            }
        }

        [HttpGet("all")]
        public async Task<ActionResult<List<ShowDto>>> GetTrendingAll()
        {
            try
            {
                var shows = await _showService.GetTrendingShows();
                return Ok(shows);
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"InvalidOperationException in GetTrendingAll: {ex.Message}");
                return StatusCode(500, new { error = "Configuration error", message = ex.Message });
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HttpRequestException in GetTrendingAll: {ex.Message}");
                return StatusCode(500, new { error = "API request failed", message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error in GetTrendingAll: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        [HttpGet("movies")]
        public async Task<ActionResult<List<ShowDto>>> GetTrendingMovies()
        {
            try
            {
                var movies = await _showService.GetTrendingMovies();
                return Ok(movies);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error fetching trending movies: {ex.Message}");
            }
        }

        [HttpGet("tv-shows")]
        public async Task<ActionResult<List<ShowDto>>> GetTrendingTvShows()
        {
            try
            {
                var tvShows = await _showService.GetTrendingTvShows();
                return Ok(tvShows);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error fetching trending TV shows: {ex.Message}");
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<List<ShowDto>>> SearchShows([FromQuery] string query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return BadRequest("Search query cannot be empty");
                }

                var results = await _showService.SearchShows(query);
                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error searching shows: {ex.Message}");
            }
        }
    }
}
