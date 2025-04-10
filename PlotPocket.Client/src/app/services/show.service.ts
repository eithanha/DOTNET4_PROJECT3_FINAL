import { Injectable } from '@angular/core';
import {
  HttpClient,
  HttpHeaders,
  HttpErrorResponse,
} from '@angular/common/http';
import {
  Observable,
  tap,
  shareReplay,
  timeout,
  catchError,
  map,
  throwError,
} from 'rxjs';
import { ShowDto } from '../models/show.dto';
import { AuthService } from './auth.service';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root',
})
export class ShowService {
  private readonly apiUrl = '/api';
  private readonly moviesUrl: string;
  private readonly tvShowsUrl: string;
  private cache = new Map<string, Observable<ShowDto[]>>();
  private readonly CACHE_SIZE = 1;
  private readonly TIMEOUT = 10000;

  constructor(
    private http: HttpClient,
    private authService: AuthService,
    private router: Router
  ) {
    this.moviesUrl = `${this.apiUrl}/movies`;
    this.tvShowsUrl = `${this.apiUrl}/TvShows`;
  }

  private getHeaders(): HttpHeaders {
    return new HttpHeaders()
      .set('Content-Type', 'application/json')
      .set('Accept', 'application/json');
  }

  private handleError(error: HttpErrorResponse): Observable<never> {
    if (error.status === 401 || error.status === 403) {
      //this.authService.clearFrontendCredentials();
      this.router.navigate(['/login'], {
        queryParams: { returnUrl: this.router.url },
      });
    }
    return throwError(() => error);
  }

  private get<T>(url: string, options: any = {}): Observable<T> {
    return this.http
      .get<T>(url, {
        withCredentials: true,
        headers: this.getHeaders(),
        ...options,
      })
      .pipe(
        map((response) => response as T),
        catchError(this.handleError.bind(this))
      );
  }

  private post<T>(url: string, body: any): Observable<T> {
    return this.http
      .post<T>(url, body, {
        withCredentials: true,
        headers: this.getHeaders(),
      })
      .pipe(
        map((response) => response as T),
        catchError(this.handleError.bind(this))
      );
  }

  private delete<T>(url: string): Observable<T> {
    return this.http
      .delete<T>(url, {
        withCredentials: true,
        headers: this.getHeaders(),
      })
      .pipe(
        map((response) => response as T),
        catchError(this.handleError.bind(this))
      );
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
    return this.get<ShowDto[]>(`${this.apiUrl}/shows/trending/all`).pipe(
      timeout(this.TIMEOUT),
      tap((shows) => this.updateCache('trending', shows))
    );
  }

  getTrendingMovies(): Observable<ShowDto[]> {
    return this.get<ShowDto[]>(`${this.apiUrl}/shows/trending/movies`).pipe(
      timeout(this.TIMEOUT),
      tap((shows) => this.updateCache('movies', shows))
    );
  }

  getTrendingTvShows(): Observable<ShowDto[]> {
    return this.get<ShowDto[]>(`${this.apiUrl}/shows/trending/tv-shows`).pipe(
      timeout(this.TIMEOUT),
      tap((shows) => this.updateCache('tv-shows', shows))
    );
  }

  searchShows(query: string): Observable<ShowDto[]> {
    return this.get<ShowDto[]>(`${this.apiUrl}/shows/search`, {
      params: { query },
    }).pipe(timeout(this.TIMEOUT));
  }

  getMovies(): Observable<ShowDto[]> {
    return this.get<ShowDto[]>(`${this.moviesUrl}`).pipe(
      map((shows) => this.transformShows(shows))
    );
  }

  getNowPlayingMovies(): Observable<ShowDto[]> {
    return this.get<ShowDto[]>(`${this.moviesUrl}/now-playing`).pipe(
      map((shows) => this.transformShows(shows))
    );
  }

  getTopRatedMovies(): Observable<ShowDto[]> {
    return this.get<ShowDto[]>(`${this.moviesUrl}/top-rated`).pipe(
      map((shows) => this.transformShows(shows))
    );
  }

  getPopularMovies(): Observable<ShowDto[]> {
    return this.get<ShowDto[]>(`${this.moviesUrl}/popular`).pipe(
      map((shows) => this.transformShows(shows))
    );
  }

  getPopularTvShows(): Observable<ShowDto[]> {
    return this.get<ShowDto[]>(`${this.tvShowsUrl}/popular`).pipe(
      map((shows) => this.transformShows(shows))
    );
  }

  getTopRatedTvShows(): Observable<ShowDto[]> {
    return this.get<ShowDto[]>(`${this.tvShowsUrl}/top-rated`).pipe(
      map((shows) => this.transformShows(shows))
    );
  }

  getOnAirTvShows(): Observable<ShowDto[]> {
    return this.get<ShowDto[]>(`${this.tvShowsUrl}/on-air`).pipe(
      map((shows) => this.transformShows(shows))
    );
  }

  getBookmarks(): Observable<ShowDto[]> {
    return this.get<ShowDto[]>(`${this.apiUrl}/shows/bookmarks`).pipe(
      timeout(this.TIMEOUT)
    );
  }

  addToBookmarks(showId: number): Observable<ShowDto> {
    return this.post<ShowDto>(`${this.apiUrl}/shows/${showId}/bookmark`, {});
  }

  removeFromBookmarks(showId: number): Observable<ShowDto> {
    return this.delete<ShowDto>(`${this.apiUrl}/shows/${showId}/bookmark`);
  }

  addToWatchlist(show: ShowDto): Observable<ShowDto> {
    return this.post<ShowDto>(`${this.apiUrl}/shows/${show.id}/watchlist`, {});
  }

  removeFromWatchlist(show: ShowDto): Observable<ShowDto> {
    return this.delete<ShowDto>(`${this.apiUrl}/shows/${show.id}/watchlist`);
  }

  toggleWatchlist(show: ShowDto): Observable<ShowDto> {
    if (!this.authService.isLoggedIn()) {
      throw new Error('User is not logged in');
    }
    return show.isWatchlisted
      ? this.removeFromWatchlist(show)
      : this.addToWatchlist(show);
  }

  private updateCache(key: string, shows: ShowDto[]): void {
    if (this.cache.size >= this.CACHE_SIZE) {
      const firstKey = this.cache.keys().next().value;
      if (firstKey) {
        this.cache.delete(firstKey);
      }
    }
    this.cache.set(
      key,
      new Observable<ShowDto[]>((subscriber) => {
        subscriber.next(shows);
        subscriber.complete();
      }).pipe(shareReplay(1))
    );
  }
}
