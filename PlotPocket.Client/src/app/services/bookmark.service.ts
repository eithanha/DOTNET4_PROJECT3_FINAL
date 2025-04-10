import { Injectable } from '@angular/core';
import {
  HttpClient,
  HttpHeaders,
  HttpErrorResponse,
} from '@angular/common/http';
import {
  BehaviorSubject,
  Observable,
  tap,
  shareReplay,
  throwError,
  catchError,
  map,
} from 'rxjs';
import { ShowDto } from '../models/show.dto';
import { AuthService } from './auth.service';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root',
})
export class BookmarkService {
  private readonly apiUrl = '/api/shows';
  private bookmarksSubject = new BehaviorSubject<ShowDto[]>([]);
  private bookmarksCache = new Set<number>();
  private isLoading = false;
  private bookmarks$: Observable<ShowDto[]>;

  constructor(
    private http: HttpClient,
    private authService: AuthService,
    private router: Router
  ) {
    this.bookmarks$ = this.bookmarksSubject.asObservable().pipe(shareReplay(1));

    this.authService.getCurrentUser().subscribe((user) => {
      if (user) {
        this.loadBookmarks();
      } else {
        this.clearBookmarks();
      }
    });
  }

  private getHeaders(): HttpHeaders {
    return new HttpHeaders()
      .set('Content-Type', 'application/json')
      .set('Accept', 'application/json');
  }

  private handleError(error: HttpErrorResponse): Observable<never> {
    if (error.status === 401 || error.status === 403) {
      this.router.navigate(['/login'], {
        queryParams: { returnUrl: this.router.url },
      });
    }
    return throwError(() => error);
  }

  private get<T>(url: string): Observable<T> {
    return this.http
      .get<T>(url, {
        withCredentials: true,
        headers: this.getHeaders(),
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

  get bookmarksObservable(): Observable<ShowDto[]> {
    return this.bookmarks$;
  }

  get bookmarks(): ShowDto[] {
    return this.bookmarksSubject.value;
  }

  getBookmarks(): Observable<ShowDto[]> {
    return this.bookmarks$;
  }

  private clearBookmarks(): void {
    this.bookmarksSubject.next([]);
    this.bookmarksCache.clear();
  }

  private loadBookmarks(): void {
    if (this.isLoading) return;
    this.isLoading = true;

    this.get<ShowDto[]>(`${this.apiUrl}/bookmarks`)
      .pipe(
        tap((bookmarks) => {
          this.bookmarksSubject.next(bookmarks);
          this.bookmarksCache = new Set(bookmarks.map((b) => b.id));
          this.isLoading = false;
        }),
        catchError((error) => {
          this.isLoading = false;
          this.clearBookmarks();
          return throwError(() => error);
        })
      )
      .subscribe();
  }

  addBookmark(showOrId: ShowDto | number): Observable<ShowDto> {
    const showId = typeof showOrId === 'number' ? showOrId : showOrId.id;
    return this.post<ShowDto>(`${this.apiUrl}/${showId}/bookmark`, {}).pipe(
      tap((show) => {
        const currentBookmarks = this.bookmarksSubject.value;
        this.bookmarksSubject.next([...currentBookmarks, show]);
        this.bookmarksCache.add(show.id);
      })
    );
  }

  removeBookmark(showId: number): Observable<ShowDto> {
    return this.delete<ShowDto>(`${this.apiUrl}/${showId}/bookmark`).pipe(
      tap(() => {
        const currentBookmarks = this.bookmarksSubject.value;
        this.bookmarksSubject.next(
          currentBookmarks.filter((b) => b.id !== showId)
        );
        this.bookmarksCache.delete(showId);
      })
    );
  }

  isBookmarked(showId: number): boolean {
    return this.bookmarksCache.has(showId);
  }

  toggleBookmark(showId: number): Observable<ShowDto> {
    return this.isBookmarked(showId)
      ? this.removeBookmark(showId)
      : this.addBookmark(showId);
  }

  updateShowsBookmarkStatus(shows: ShowDto[]): ShowDto[] {
    return shows.map((show) => ({
      ...show,
      isBookmarked: this.isBookmarked(show.id),
    }));
  }
}
