using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PlotPocket.Server.Models;
using PlotPocket.Server.Models.Responses;
using PlotPocket.Server.Data;
using PlotPocket.Server.Models.Dtos;



namespace PlotPocket.Server.Services;

public class ShowService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly string _tmdbImageBaseUrl;
    private readonly HttpClient _httpClient;
    private readonly string _tmdbApiKey;
    private readonly string _tmdbBaseUrl = "https://api.themoviedb.org/3";
    private readonly TMDBService _tmdbService;

    public ShowService(ApplicationDbContext context, IConfiguration configuration, TMDBService tmdbService)
    {
        _context = context;
        _configuration = configuration;
        _tmdbImageBaseUrl = _configuration["TMDB:Images:SecureBaseUrl"] + _configuration["TMDB:Images:PosterSizes:Medium"];
        _tmdbApiKey = configuration["TMDB:ApiKey"];
        _tmdbService = tmdbService;


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


    private ShowDto MediaItemToShowDto(ApiMediaItem mediaItem, string userId = null)
    {
        string title = null;
        DateTime? date = null;
        string posterPath = mediaItem.PosterPath;

        if (mediaItem is Movie movie)
        {
            title = movie.Title;
            if (DateTime.TryParse(movie.ReleaseDate, out DateTime movieDate))
            {
                date = movieDate;
            }
        }
        else if (mediaItem is TvShow tvShow)
        {
            title = tvShow.Name;
            if (DateTime.TryParse(tvShow.FirstAirDate, out DateTime tvDate))
            {
                date = tvDate;
            }
        }
        else if (mediaItem is Trending trending)
        {
            title = trending.Title ?? trending.Name;
            string dateStr = trending.ReleaseDate ?? trending.FirstAirDate;
            if (!string.IsNullOrEmpty(dateStr) && DateTime.TryParse(dateStr, out DateTime trendingDate))
            {
                date = trendingDate;
            }
        }

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
            PosterPath = posterPath,
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
                throw new InvalidOperationException("TMDB API Key Is Not Configured");
            }

            var url = $"{_tmdbBaseUrl}/trending/all/api_key={_tmdbApiKey}";
            Console.WriteLine($"Making Request To TMDB: {url}");


            int maxRetries = 3;
            int currentRetry = 0;
            while (currentRetry < maxRetries)
            {
                try
                {
                    var response = await _httpClient.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    var content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"TMDB Response Received Successfully");

                    var trendingResponse = JsonSerializer.Deserialize<TrendingResponse>(content);
                    if (trendingResponse?.Results == null)
                    {
                        throw new InvalidOperationException("Invalid Response From TMDB API");
                    }

                    return trendingResponse.Results.Select(item => MediaItemToShowDto(item, null)).ToList();
                }
                catch (HttpRequestException ex) when (currentRetry < maxRetries - 1)
                {
                    currentRetry++;
                    Console.WriteLine($"Attempt {currentRetry} Failed: {ex.Message}");
                    await Task.Delay(1000 * currentRetry);
                    continue;
                }
                catch (HttpRequestException ex)
                {
                    throw new HttpRequestException($"Failed To Connect To TMDB API After {maxRetries} Attempts. Please Check Your Internet Connection And DNS Settings.", ex);
                }
            }

            throw new InvalidOperationException("Failed To Get Trending Shows After Multiple Attempts");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetTrendingShows: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            throw;
        }
    }

    public async Task<List<ShowDto>> GetTrendingMovies()
    {
        var response = await _httpClient.GetAsync($"{_tmdbBaseUrl}/trending/movie/api_key={_tmdbApiKey}");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var trendingResponse = JsonSerializer.Deserialize<TrendingResponse>(content);

        return trendingResponse.Results.Select(item => MediaItemToShowDto(item, null)).ToList();
    }

    public async Task<List<ShowDto>> GetTrendingTvShows()
    {
        var response = await _httpClient.GetAsync($"{_tmdbBaseUrl}/trending/tv/api_key={_tmdbApiKey}");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var trendingResponse = JsonSerializer.Deserialize<TrendingResponse>(content);

        return trendingResponse.Results.Select(item => MediaItemToShowDto(item, null)).ToList();
    }

    public async Task<ShowDto> GetShowDetails(int id)
    {
        try
        {
            var mediaItem = await _tmdbService.GetShowDetailsAsync(id);
            return MediaItemToShowDto(mediaItem, null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error Getting Show Details For ShowId {id}: {ex.Message}");
            throw;
        }
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
            throw new InvalidOperationException("User Not Found");
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
            throw new InvalidOperationException("Show Not Found");
        }

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException("User Not Found");
        }

        show.Users.Remove(user);
        await _context.SaveChangesAsync();
        return ShowToShowDto(show);
    }

    public async Task<List<ShowDto>> SearchShows(string query)
    {
        try
        {

            var movieResponse = await _httpClient.GetAsync($"{_tmdbBaseUrl}/search/movie?api_key={_tmdbApiKey}&query={Uri.EscapeDataString(query)}");
            movieResponse.EnsureSuccessStatusCode();
            var movieContent = await movieResponse.Content.ReadAsStringAsync();
            var movieSearchResponse = JsonSerializer.Deserialize<TrendingResponse>(movieContent);


            var tvResponse = await _httpClient.GetAsync($"{_tmdbBaseUrl}/search/tv?api_key={_tmdbApiKey}&query={Uri.EscapeDataString(query)}");
            tvResponse.EnsureSuccessStatusCode();
            var tvContent = await tvResponse.Content.ReadAsStringAsync();
            var tvSearchResponse = JsonSerializer.Deserialize<TrendingResponse>(tvContent);


            var movieResults = movieSearchResponse?.Results.Select(item => MediaItemToShowDto(item, null)) ?? new List<ShowDto>();
            var tvResults = tvSearchResponse?.Results.Select(item => MediaItemToShowDto(item, null)) ?? new List<ShowDto>();


            return movieResults.Concat(tvResults)
                .OrderByDescending(show => show.Rating)
                .ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error In SearchShows: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            throw;
        }
    }

    public async Task<List<ShowDto>> GetPopularTvShows()
    {
        try
        {
            if (string.IsNullOrEmpty(_tmdbApiKey))
            {
                throw new InvalidOperationException("TMDB API Key Is Not Configured");
            }

            var url = $"{_tmdbBaseUrl}/tv/popular?api_key={_tmdbApiKey}";
            Console.WriteLine($"Making Request To TMDB: {url}");


            int maxRetries = 3;
            int currentRetry = 0;
            while (currentRetry < maxRetries)
            {
                try
                {
                    var response = await _httpClient.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    var content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"TMDB Response Received Successfully");

                    var tvShowResponse = JsonSerializer.Deserialize<TvShowResponse>(content);
                    if (tvShowResponse?.Results == null)
                    {
                        throw new InvalidOperationException("Invalid Response From TMDB API");
                    }

                    return tvShowResponse.Results.Select(item => TvShowToShowDto(item)).ToList();
                }
                catch (HttpRequestException ex) when (currentRetry < maxRetries - 1)
                {
                    currentRetry++;
                    Console.WriteLine($"Attempt {currentRetry} Failed: {ex.Message}");
                    await Task.Delay(1000 * currentRetry); 
                    continue;
                }
                catch (HttpRequestException ex)
                {
                    throw new HttpRequestException($"Failed To Connect To TMDB API After {maxRetries} Attempts. Please Check Your Internet Connection And DNS Settings.", ex);
                }
            }

            throw new InvalidOperationException("Failed To Get Popular TV Shows After Multiple Attempts");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error In GetPopularTvShows: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            throw;
        }
    }

    public async Task<List<ShowDto>> GetTopRatedTvShows()
    {
        try
        {
            if (string.IsNullOrEmpty(_tmdbApiKey))
            {
                throw new InvalidOperationException("TMDB API Key Is Not Configured");
            }

            var url = $"{_tmdbBaseUrl}/tv/top_rated?api_key={_tmdbApiKey}";
            Console.WriteLine($"Making Request To TMDB: {url}");

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var tvShowResponse = JsonSerializer.Deserialize<TvShowResponse>(content);

            return tvShowResponse?.Results.Select(item => TvShowToShowDto(item)).ToList() ?? new List<ShowDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error In GetTopRatedTvShows: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            throw;
        }
    }

    public async Task<List<ShowDto>> GetOnAirTvShows()
    {
        try
        {
            if (string.IsNullOrEmpty(_tmdbApiKey))
            {
                throw new InvalidOperationException("TMDB API Key Is Not Configured");
            }

            var url = $"{_tmdbBaseUrl}/tv/on_the_air?api_key={_tmdbApiKey}";
            Console.WriteLine($"Making Request To TMDB: {url}");

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var tvShowResponse = JsonSerializer.Deserialize<TvShowResponse>(content);

            return tvShowResponse?.Results.Select(item => TvShowToShowDto(item)).ToList() ?? new List<ShowDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error In GetOnAirTvShows: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            throw;
        }
    }

    public async Task<IEnumerable<ShowDto>> GetTrendingShowsAsync()
    {
        var response = await _tmdbService.GetTrendingShowsAsync();
        return response.Results.Select(MapToShowDto);
    }

    public async Task<IEnumerable<ShowDto>> GetTrendingMoviesAsync()
    {
        var response = await _tmdbService.GetTrendingMoviesAsync();
        return response.Results.Select(MapToShowDto);
    }

    public async Task<IEnumerable<ShowDto>> GetTrendingTvShowsAsync()
    {
        var response = await _tmdbService.GetTrendingTvShowsAsync();
        return response.Results.Select(MapToShowDto);
    }

    public async Task<IEnumerable<ShowDto>> SearchShowsAsync(string query)
    {
        var response = await _tmdbService.SearchShowsAsync(query);
        return response.Results.Select(MapToShowDto);
    }

    public async Task<ShowDto> AddBookmarkAsync(int showId, string userId)
    {
        try
        {
            Console.WriteLine($"Adding Bookmark For Show {showId} And User {userId}");


            var existingBookmark = await _context.Bookmarks
                .FirstOrDefaultAsync(b => b.ShowId == showId && b.UserId == userId);

            if (existingBookmark != null)
            {
                Console.WriteLine($"Bookmark Already Exists For Show {showId}");

                try
                {
                    var existingShow = await _tmdbService.GetShowDetailsAsync(showId);
                    if (existingShow != null)
                    {
                        var existingDto = MediaItemToShowDto(existingShow, userId);
                        existingDto.IsBookmarked = true;
                        return existingDto;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error Getting Show Details For Existing Bookmark: {ex.Message}");

                    return new ShowDto
                    {
                        Id = showId,
                        IsBookmarked = true
                    };
                }
            }


            var bookmark = new Bookmark
            {
                ShowId = showId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Bookmarks.Add(bookmark);
            await _context.SaveChangesAsync();
            Console.WriteLine($"Created New Bookmark For Show {showId}");


            try
            {
                var newShow = await _tmdbService.GetShowDetailsAsync(showId);
                if (newShow != null)
                {
                    var newDto = MediaItemToShowDto(newShow, userId);
                    newDto.IsBookmarked = true;
                    return newDto;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error Getting Show Details For New Bookmark: {ex.Message}");

                return new ShowDto
                {
                    Id = showId,
                    IsBookmarked = true
                };
            }


            throw new Exception($"Failed To Get Show Details For Show {showId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error In AddBookmarkAsync: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            throw;
        }
    }

    public async Task<ShowDto> RemoveBookmarkAsync(int showId, string userId)
    {
        var bookmark = await _context.Bookmarks
            .FirstOrDefaultAsync(b => b.ShowId == showId && b.UserId == userId);

        if (bookmark != null)
        {
            _context.Bookmarks.Remove(bookmark);
            await _context.SaveChangesAsync();
        }

        var show = await _tmdbService.GetShowDetailsAsync(showId);
        var showDto = MediaItemToShowDto(show, userId);
        showDto.IsBookmarked = false;
        return showDto;
    }

    public async Task<IEnumerable<ShowDto>> GetBookmarksAsync(string userId)
    {
        try
        {

            var bookmarkedShowIds = await _context.Bookmarks
                .Where(b => b.UserId == userId)
                .Select(b => b.ShowId)
                .ToListAsync();

            Console.WriteLine($"Found {bookmarkedShowIds.Count} Bookmarks For User {userId}");

            var shows = new List<ShowDto>();
            foreach (var showId in bookmarkedShowIds)
            {
                try
                {
                    Console.WriteLine($"Getting Details For Show {showId}");
                    var show = await _tmdbService.GetShowDetailsAsync(showId);
                    if (show != null)
                    {
                        var showDto = MediaItemToShowDto(show, userId);
                        showDto.IsBookmarked = true;
                        shows.Add(showDto);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error Getting Show Details For ShowId {showId}: {ex.Message}");

                    continue;
                }
            }

            return shows;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error In GetBookmarksAsync: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            throw;
        }
    }

    private ShowDto MapToShowDto(dynamic show)
    {

        string? posterPath = null;
        if (!string.IsNullOrEmpty(show.PosterPath))
        {

            if (show.PosterPath.ToString().StartsWith("http"))
            {
                posterPath = show.PosterPath.ToString();
            }

            else
            {
                posterPath = _tmdbImageBaseUrl + show.PosterPath.ToString();
            }
        }

        string? title = show.Title ?? show.Name;
        DateTime? releaseDate = null;

        if (show.ReleaseDate != null)
        {
            if (DateTime.TryParse(show.ReleaseDate.ToString(), out DateTime parsedDate))
            {
                releaseDate = parsedDate;
            }
        }
        else if (show.FirstAirDate != null)
        {
            if (DateTime.TryParse(show.FirstAirDate.ToString(), out DateTime parsedDate))
            {
                releaseDate = parsedDate;
            }
        }

        return new ShowDto
        {
            Id = show.Id,
            Title = title ?? string.Empty,
            Overview = show.Overview,
            PosterPath = posterPath,
            Rating = show.VoteAverage,
            ReleaseDate = releaseDate,
            Type = show.Title != null ? ShowType.Movie : ShowType.TvShow
        };
    }
}
