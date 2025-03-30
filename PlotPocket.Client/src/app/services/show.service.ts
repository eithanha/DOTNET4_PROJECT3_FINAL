import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { ShowDto } from '../models/show.dto';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class ShowService {
  private apiUrl = `${environment.apiUrl}/trending`;

  constructor(private http: HttpClient) {
    console.log('ShowService constructed with API URL:', this.apiUrl);
  }

  getTrendingShows(): Observable<ShowDto[]> {
    console.log('Fetching trending shows from:', `${this.apiUrl}/all`);
    return this.http.get<ShowDto[]>(`${this.apiUrl}/all`).pipe(
      tap({
        next: (shows) => console.log('API Response:', shows),
        error: (error) => console.error('API Error:', error),
      })
    );
  }

  getTrendingMovies(): Observable<ShowDto[]> {
    return this.http.get<ShowDto[]>(`${this.apiUrl}/movies`);
  }

  getTrendingTvShows(): Observable<ShowDto[]> {
    return this.http.get<ShowDto[]>(`${this.apiUrl}/tv-shows`);
  }

  getMovies(): Observable<ShowDto[]> {
    return this.http.get<ShowDto[]>(`${environment.apiUrl}/movies`);
  }

  toggleWatchlist(show: ShowDto): Observable<ShowDto> {
    if (show.isWatchlisted) {
      return this.removeFromWatchlist(show).pipe(
        tap(() => {
          show.isWatchlisted = false;
        })
      );
    } else {
      return this.addToWatchlist(show).pipe(
        tap(() => {
          show.isWatchlisted = true;
        })
      );
    }
  }

  addToWatchlist(show: ShowDto): Observable<ShowDto> {
    return this.http.post<ShowDto>(`${environment.apiUrl}/shows/watchlist`, {
      showId: show.id,
    });
  }

  removeFromWatchlist(show: ShowDto): Observable<ShowDto> {
    return this.http.delete<ShowDto>(
      `${environment.apiUrl}/shows/watchlist/${show.id}`
    );
  }
}
