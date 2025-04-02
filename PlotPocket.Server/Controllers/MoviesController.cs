using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PlotPocket.Server.Models;
using PlotPocket.Server.Services;
using System.Security.Claims;
using PlotPocket.Server.Models.Dtos;
using PlotPocket.Server.Models.Responses;

namespace PlotPocket.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MoviesController : ControllerBase
{
    private readonly ITmdbService _tmdbService;
    private readonly ShowService _showService;

    public MoviesController(ITmdbService tmdbService, ShowService showService)
    {
        _tmdbService = tmdbService;
        _showService = showService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ShowDto>>> GetMovies()
    {
        try
        {
            var movieResponse = await _tmdbService.GetPopularMoviesAsync();
            var movies = movieResponse.Results.Select(movie => new ShowDto
            {
                Id = movie.Id,
                Title = movie.Title,
                Overview = movie.Overview,
                ReleaseDate = movie.DisplayDate,
                PosterPath = movie.PosterPath,
                Type = ShowType.Movie,
                Rating = movie.VoteAverage
            }).ToList();


            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                var userWatchlist = await _showService.GetUserWatchlistAsync(userId);
                foreach (var movie in movies)
                {
                    movie.IsWatchlisted = userWatchlist.Any(s => s.Id == movie.Id);
                }
            }

            return Ok(movies);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error fetching movies from TMDB");
        }
    }

    [HttpGet("now-playing")]
    public async Task<ActionResult<IEnumerable<ShowDto>>> GetNowPlayingMovies()
    {
        try
        {
            var movieResponse = await _tmdbService.GetNowPlayingMoviesAsync();
            var movies = movieResponse.Results.Select(movie => new ShowDto
            {
                Id = movie.Id,
                Title = movie.Title,
                Overview = movie.Overview,
                ReleaseDate = movie.DisplayDate,
                PosterPath = movie.PosterPath,
                Type = ShowType.Movie,
                Rating = movie.VoteAverage
            }).ToList();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                var userWatchlist = await _showService.GetUserWatchlistAsync(userId);
                foreach (var movie in movies)
                {
                    movie.IsWatchlisted = userWatchlist.Any(s => s.Id == movie.Id);
                }
            }

            return Ok(movies);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error fetching now playing movies from TMDB");
        }
    }

    [HttpGet("top-rated")]
    public async Task<ActionResult<IEnumerable<ShowDto>>> GetTopRatedMovies()
    {
        try
        {
            var movieResponse = await _tmdbService.GetTopRatedMoviesAsync();
            var movies = movieResponse.Results.Select(movie => new ShowDto
            {
                Id = movie.Id,
                Title = movie.Title,
                Overview = movie.Overview,
                ReleaseDate = movie.DisplayDate,
                PosterPath = movie.PosterPath,
                Type = ShowType.Movie,
                Rating = movie.VoteAverage
            }).ToList();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                var userWatchlist = await _showService.GetUserWatchlistAsync(userId);
                foreach (var movie in movies)
                {
                    movie.IsWatchlisted = userWatchlist.Any(s => s.Id == movie.Id);
                }
            }

            return Ok(movies);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error fetching top rated movies from TMDB");
        }
    }

    [HttpGet("popular")]
    public async Task<ActionResult<IEnumerable<ShowDto>>> GetPopularMovies()
    {
        try
        {
            var movieResponse = await _tmdbService.GetPopularMoviesAsync();
            var movies = movieResponse.Results.Select(movie => new ShowDto
            {
                Id = movie.Id,
                Title = movie.Title,
                Overview = movie.Overview,
                ReleaseDate = movie.DisplayDate,
                PosterPath = movie.PosterPath,
                Type = ShowType.Movie,
                Rating = movie.VoteAverage
            }).ToList();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                var userWatchlist = await _showService.GetUserWatchlistAsync(userId);
                foreach (var movie in movies)
                {
                    movie.IsWatchlisted = userWatchlist.Any(s => s.Id == movie.Id);
                }
            }

            return Ok(movies);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error fetching popular movies from TMDB");
        }
    }

    [Authorize]
    [HttpPost("{id}/watchlist")]
    public async Task<ActionResult<ShowDto>> AddToWatchlist(int id)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var show = await _showService.AddToWatchlistAsync(userId, id);
            return Ok(show);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error adding movie to watchlist");
        }
    }

    [Authorize]
    [HttpDelete("{id}/watchlist")]
    public async Task<ActionResult<ShowDto>> RemoveFromWatchlist(int id)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var show = await _showService.RemoveFromWatchlistAsync(userId, id);
            return Ok(show);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error removing movie from watchlist");
        }
    }
}
