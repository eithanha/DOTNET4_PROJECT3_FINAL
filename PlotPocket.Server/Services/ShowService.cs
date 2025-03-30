using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PlotPocket.Server.Models;

using PlotPocket.Server.Models.Responses;
using PlotPocket.Server.Data;
using PlotPocket.Server.Models.Dtos;

using System.Net.Http;
using System.Threading.Tasks;

namespace PlotPocket.Server.Services;

public class ShowService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly string _tmdbImageBaseUrl;
    private readonly HttpClient _httpClient;
    private readonly string _tmdbApiKey;
    private readonly string _tmdbBaseUrl = "https://api.themoviedb.org/3";

    public ShowService(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
        _tmdbImageBaseUrl = _configuration["TMDB:Images:SecureBaseUrl"] + _configuration["TMDB:Images:PosterSizes:Medium"];
        _tmdbApiKey = configuration["TMDB:ApiKey"];

        // Configure HttpClient with better DNS resolution and retry policy
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true,
            UseProxy = false,
            AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
        };

        _httpClient = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(30),
            DefaultRequestHeaders =
            {
                Accept = { new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json") }
            }
        };
    }

    /**
     * Below can be used for converting return objects from the Trending endpoints 
     * to ShowDtos. 
     * 
     * TODO: Make sure to fill in the ShowDto properties on the return of this method.
     *          You should **NOT** need to modify anything else.
     * 
     **/
    public ShowDto MediaItemToShowDto(ApiMediaItem mediaItem, string? userId)
    {
        string? dateToParse = mediaItem switch
        {
            Movie movie => movie.ReleaseDate,
            TvShow tvShow => tvShow.FirstAirDate,
            Trending trendingShow => trendingShow.ReleaseDate ?? trendingShow.FirstAirDate,
            _ => null
        };

        DateTime? date = null;
        if (!string.IsNullOrEmpty(dateToParse))
        {
            if (DateTime.TryParse(dateToParse, out DateTime parsedDate))
            {
                date = parsedDate;
            }
        }

        string? title = mediaItem switch
        {
            Trending trendingMedia => trendingMedia.MediaType == "movie" ? trendingMedia.Title : trendingMedia.Name,
            Movie movie => movie.Title,
            TvShow tvShow => tvShow.Name,
            _ => null
        };

        return new ShowDto
        {
            Id = mediaItem.Id,
            Type = mediaItem switch
            {
                Trending trendingItem => trendingItem.MediaType == "movie" ? ShowType.Movie : ShowType.TvShow,
                Movie => ShowType.Movie,
                TvShow => ShowType.TvShow,
                _ => ShowType.Movie
            },
            Title = title ?? string.Empty,
            Overview = mediaItem.Overview,
            ReleaseDate = date,
            PosterPath = !string.IsNullOrEmpty(mediaItem.PosterPath) ? _tmdbImageBaseUrl + mediaItem.PosterPath : null,
            Rating = mediaItem.VoteAverage,
            IsWatchlisted = userId != null && IsShowInUserWatchlist(mediaItem.Id, userId),
            IsWatched = userId != null && IsShowWatchedByUser(mediaItem.Id, userId)
        };
    }

    public ShowDto ShowToShowDto(Show show)
    {
        return new ShowDto
        {
            Id = show.ShowApiId,
            Type = show.Type,
            Title = show.Title,
            Overview = show.Overview,
            ReleaseDate = show.ReleaseDate,
            PosterPath = show.PosterPath,
            IsWatchlisted = true,
            IsWatched = show.Watched
        };
    }

    public ShowDto MovieToShowDto(Movie movie)
    {
        return new ShowDto
        {
            Id = movie.Id,
            Type = ShowType.Movie,
            Title = movie.Title,
            Overview = movie.Overview,
            ReleaseDate = movie.DisplayDate,
            PosterPath = !string.IsNullOrEmpty(movie.PosterPath) ? _tmdbImageBaseUrl + movie.PosterPath : null,
            Rating = movie.VoteAverage
        };
    }

    public ShowDto TvShowToShowDto(TvShow tvShow)
    {
        return new ShowDto
        {
            Id = tvShow.Id,
            Type = ShowType.TvShow,
            Title = tvShow.Name,
            Overview = tvShow.Overview,
            ReleaseDate = tvShow.DisplayDate,
            PosterPath = !string.IsNullOrEmpty(tvShow.PosterPath) ? _tmdbImageBaseUrl + tvShow.PosterPath : null,
            Rating = tvShow.VoteAverage
        };
    }

    private bool IsShowInUserWatchlist(int showId, string userId)
    {
        return _context.Set<Show>()
            .Include(s => s.Users)
            .Any(s => s.ShowApiId == showId && s.Users.Any(u => u.Id == userId));
    }

    private bool IsShowWatchedByUser(int showId, string userId)
    {
        return _context.Set<Show>()
            .Include(s => s.Users)
            .Any(s => s.ShowApiId == showId && s.Users.Any(u => u.Id == userId) && s.Watched);
    }

    public async Task<List<ShowDto>> GetTrendingShows()
    {
        try
        {
            if (string.IsNullOrEmpty(_tmdbApiKey))
            {
                throw new InvalidOperationException("TMDB API key is not configured");
            }

            var url = $"{_tmdbBaseUrl}/trending/all/day?api_key={_tmdbApiKey}";
            Console.WriteLine($"Making request to TMDB: {url}");

            // Add retry logic
            int maxRetries = 3;
            int currentRetry = 0;
            while (currentRetry < maxRetries)
            {
                try
                {
                    var response = await _httpClient.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    var content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"TMDB Response received successfully");

                    var trendingResponse = JsonSerializer.Deserialize<TrendingResponse>(content);
                    if (trendingResponse?.Results == null)
                    {
                        throw new InvalidOperationException("Invalid response from TMDB API");
                    }

                    return trendingResponse.Results.Select(item => MediaItemToShowDto(item, null)).ToList();
                }
                catch (HttpRequestException ex) when (currentRetry < maxRetries - 1)
                {
                    currentRetry++;
                    Console.WriteLine($"Attempt {currentRetry} failed: {ex.Message}");
                    await Task.Delay(1000 * currentRetry); // Exponential backoff
                    continue;
                }
                catch (HttpRequestException ex)
                {
                    throw new HttpRequestException($"Failed to connect to TMDB API after {maxRetries} attempts. Please check your internet connection and DNS settings.", ex);
                }
            }

            throw new InvalidOperationException("Failed to get trending shows after multiple attempts");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetTrendingShows: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    public async Task<List<ShowDto>> GetTrendingMovies()
    {
        var response = await _httpClient.GetAsync($"{_tmdbBaseUrl}/trending/movie/day?api_key={_tmdbApiKey}");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var trendingResponse = JsonSerializer.Deserialize<TrendingResponse>(content);

        return trendingResponse.Results.Select(item => MediaItemToShowDto(item, null)).ToList();
    }

    public async Task<List<ShowDto>> GetTrendingTvShows()
    {
        var response = await _httpClient.GetAsync($"{_tmdbBaseUrl}/trending/tv/day?api_key={_tmdbApiKey}");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var trendingResponse = JsonSerializer.Deserialize<TrendingResponse>(content);

        return trendingResponse.Results.Select(item => MediaItemToShowDto(item, null)).ToList();
    }

    public async Task<ShowDto> GetShowDetails(int id)
    {
        var response = await _httpClient.GetAsync($"{_tmdbBaseUrl}/movie/{id}?api_key={_tmdbApiKey}");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var movie = JsonSerializer.Deserialize<Movie>(content);

        DateTime? releaseDate = null;
        if (!string.IsNullOrEmpty(movie.ReleaseDate))
        {
            if (DateTime.TryParse(movie.ReleaseDate, out DateTime parsedDate))
            {
                releaseDate = parsedDate;
            }
        }

        return new ShowDto
        {
            Id = movie.Id,
            Title = movie.Title,
            Overview = movie.Overview,
            PosterPath = movie.PosterPath,
            ReleaseDate = releaseDate,
            Type = ShowType.Movie
        };
    }

    public async Task<bool> AddToWatchlist(string userId, int showId)
    {
        var user = await _context.Users
            .Include(u => u.Shows)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return false;

        var show = await _context.Shows.FindAsync(showId);
        if (show == null) return false;

        user.Shows.Add(show);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveFromWatchlist(string userId, int showId)
    {
        var user = await _context.Users
            .Include(u => u.Shows)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return false;

        var show = await _context.Shows.FindAsync(showId);
        if (show == null) return false;

        user.Shows.Remove(show);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<ShowDto>> GetUserWatchlist(string userId)
    {
        var user = await _context.Users
            .Include(u => u.Shows)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return new List<ShowDto>();

        return user.Shows.Select(show => new ShowDto
        {
            Id = show.ShowApiId,
            Title = show.Title,
            Overview = show.Overview,
            PosterPath = show.PosterPath,
            ReleaseDate = show.ReleaseDate,
            Type = show.Type,
            IsWatchlisted = true,
            IsWatched = show.Watched
        }).ToList();
    }

    public async Task<List<ShowDto>> GetUserWatchlistAsync(string userId)
    {
        var userShows = await _context.Set<Show>()
            .Include(s => s.Users)
            .Where(s => s.Users.Any(u => u.Id == userId))
            .ToListAsync();

        return userShows.Select(ShowToShowDto).ToList();
    }

    public async Task<ShowDto> AddToWatchlistAsync(string userId, int showId)
    {
        var show = await _context.Set<Show>()
            .Include(s => s.Users)
            .FirstOrDefaultAsync(s => s.ShowApiId == showId);

        if (show == null)
        {
            // If show doesn't exist in our database, create it
            var movieDetails = await GetShowDetails(showId);
            show = new Show
            {
                ShowApiId = showId,
                Title = movieDetails.Title,
                Overview = movieDetails.Overview,
                ReleaseDate = movieDetails.ReleaseDate,
                PosterPath = movieDetails.PosterPath,
                Type = ShowType.Movie
            };
            _context.Set<Show>().Add(show);
        }

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        if (!show.Users.Contains(user))
        {
            show.Users.Add(user);
        }

        await _context.SaveChangesAsync();
        return ShowToShowDto(show);
    }

    public async Task<ShowDto> RemoveFromWatchlistAsync(string userId, int showId)
    {
        var show = await _context.Set<Show>()
            .Include(s => s.Users)
            .FirstOrDefaultAsync(s => s.ShowApiId == showId);

        if (show == null)
        {
            throw new InvalidOperationException("Show not found");
        }

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        show.Users.Remove(user);
        await _context.SaveChangesAsync();
        return ShowToShowDto(show);
    }
}
