import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, tap, shareReplay, timeout, catchError, map } from 'rxjs';
import { ShowDto } from '../models/show.dto';
import { environment } from '../../environments/environment';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root',
})
export class ShowService {
  private apiUrl = `${environment.apiUrl}/shows`;
  private moviesUrl = `${environment.apiUrl}/movies`;
  private tvShowsUrl = `${environment.apiUrl}/TvShows`;
  private cache = new Map<string, Observable<ShowDto[]>>();
  private readonly CACHE_SIZE = 1;
  private readonly TIMEOUT = 10000;

  constructor(private http: HttpClient, private authService: AuthService) {}

  private getAuthHeaders(): HttpHeaders {
    return new HttpHeaders({
      'Content-Type': 'application/json',
      Accept: 'application/json',
    });
  }

  private transformShow(show: any): ShowDto {
    return {
      ...show,
      PosterPath: show.posterPath || show.PosterPath,
      ReleaseDate: show.releaseDate || show.ReleaseDate,
    };
  }

  private transformShows(shows: any[]): ShowDto[] {
    return shows.map((show) => this.transformShow(show));
  }

  getTrendingShows(): Observable<ShowDto[]> {
    return this.http
      .get<ShowDto[]>(`${this.apiUrl}/trending/all`, {
        withCredentials: true,
        headers: this.getAuthHeaders(),
      })
      .pipe(map((shows) => this.transformShows(shows)));
  }

  getTrendingMovies(): Observable<ShowDto[]> {
    return this.http
      .get<ShowDto[]>(`${this.apiUrl}/trending/movies`, {
        withCredentials: true,
        headers: this.getAuthHeaders(),
      })
      .pipe(map((shows) => this.transformShows(shows)));
  }

  getTrendingTvShows(): Observable<ShowDto[]> {
    return this.http
      .get<ShowDto[]>(`${this.apiUrl}/trending/tv-shows`, {
        withCredentials: true,
        headers: this.getAuthHeaders(),
      })
      .pipe(map((shows) => this.transformShows(shows)));
  }

  searchShows(query: string): Observable<ShowDto[]> {
    return this.http
      .get<ShowDto[]>(`${this.apiUrl}/search`, {
        params: { query },
        withCredentials: true,
        headers: this.getAuthHeaders(),
      })
      .pipe(map((shows) => this.transformShows(shows)));
  }

  private getCachedRequest(url: string): Observable<ShowDto[]> {
    if (!this.cache.has(url)) {
      const request = this.http
        .get<ShowDto[]>(url, {
          withCredentials: true,
          headers: this.getAuthHeaders(),
        })
        .pipe(
          timeout(this.TIMEOUT),
          map((shows) => this.transformShows(shows)),
          shareReplay({ bufferSize: this.CACHE_SIZE, refCount: true }),
          catchError((error) => {
            console.error(`Error fetching from ${url}:`, error);
            throw error;
          })
        );
      this.cache.set(url, request);
    }
    return this.cache.get(url)!;
  }

  getMovies(): Observable<ShowDto[]> {
    return this.http
      .get<ShowDto[]>(`${this.moviesUrl}`, {
        withCredentials: true,
        headers: this.getAuthHeaders(),
      })
      .pipe(map((shows) => this.transformShows(shows)));
  }

  getNowPlayingMovies(): Observable<ShowDto[]> {
    return this.http
      .get<ShowDto[]>(`${this.moviesUrl}/now-playing`, {
        withCredentials: true,
        headers: this.getAuthHeaders(),
      })
      .pipe(map((shows) => this.transformShows(shows)));
  }

  getTopRatedMovies(): Observable<ShowDto[]> {
    return this.http
      .get<ShowDto[]>(`${this.moviesUrl}/top-rated`, {
        withCredentials: true,
        headers: this.getAuthHeaders(),
      })
      .pipe(map((shows) => this.transformShows(shows)));
  }

  getPopularMovies(): Observable<ShowDto[]> {
    return this.http
      .get<ShowDto[]>(`${this.moviesUrl}/popular`, {
        withCredentials: true,
        headers: this.getAuthHeaders(),
      })
      .pipe(map((shows) => this.transformShows(shows)));
  }

  getPopularTvShows(): Observable<ShowDto[]> {
    return this.http
      .get<ShowDto[]>(`${this.tvShowsUrl}/popular`, {
        withCredentials: true,
        headers: this.getAuthHeaders(),
      })
      .pipe(map((shows) => this.transformShows(shows)));
  }

  getTopRatedTvShows(): Observable<ShowDto[]> {
    return this.http
      .get<ShowDto[]>(`${this.tvShowsUrl}/top-rated`, {
        withCredentials: true,
        headers: this.getAuthHeaders(),
      })
      .pipe(map((shows) => this.transformShows(shows)));
  }

  getOnAirTvShows(): Observable<ShowDto[]> {
    return this.http
      .get<ShowDto[]>(`${this.tvShowsUrl}/on-air`, {
        withCredentials: true,
        headers: this.getAuthHeaders(),
      })
      .pipe(map((shows) => this.transformShows(shows)));
  }

  getBookmarks(): Observable<ShowDto[]> {
    console.log('ShowService - Getting bookmarks');
    return this.http
      .get<ShowDto[]>(`${this.apiUrl}/bookmarks`, {
        withCredentials: true,
        headers: this.getAuthHeaders(),
      })
      .pipe(
        map((shows) => this.transformShows(shows)),
        catchError((error) => {
          console.error('Error getting bookmarks:', error);
          throw error;
        })
      );
  }

  addBookmark(showId: number): Observable<ShowDto> {
    if (!this.authService.isLoggedIn()) {
      throw new Error('User is not logged in');
    }
    return this.http
      .post<ShowDto>(
        `${this.apiUrl}/${showId}/bookmark`,
        {},
        {
          withCredentials: true,
          headers: this.getAuthHeaders(),
        }
      )
      .pipe(map((show) => this.transformShow(show)));
  }

  removeBookmark(showId: number): Observable<ShowDto> {
    if (!this.authService.isLoggedIn()) {
      throw new Error('User is not logged in');
    }
    return this.http
      .delete<ShowDto>(`${this.apiUrl}/${showId}/bookmark`, {
        withCredentials: true,
        headers: this.getAuthHeaders(),
      })
      .pipe(map((show) => this.transformShow(show)));
  }

  addToWatchlist(show: ShowDto): Observable<ShowDto> {
    if (!this.authService.isLoggedIn()) {
      throw new Error('User is not logged in');
    }
    return this.http
      .post<ShowDto>(
        `${this.apiUrl}/${show.id}/watchlist`,
        {},
        {
          withCredentials: true,
          headers: this.getAuthHeaders(),
        }
      )
      .pipe(map((show) => this.transformShow(show)));
  }

  removeFromWatchlist(show: ShowDto): Observable<ShowDto> {
    if (!this.authService.isLoggedIn()) {
      throw new Error('User is not logged in');
    }
    return this.http
      .delete<ShowDto>(`${this.apiUrl}/${show.id}/watchlist`, {
        withCredentials: true,
        headers: this.getAuthHeaders(),
      })
      .pipe(map((show) => this.transformShow(show)));
  }

  toggleWatchlist(show: ShowDto): Observable<ShowDto> {
    if (!this.authService.isLoggedIn()) {
      throw new Error('User is not logged in');
    }
    return show.isWatchlisted
      ? this.removeFromWatchlist(show)
      : this.addToWatchlist(show);
  }
}
