import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { BehaviorSubject, Observable, tap, shareReplay } from 'rxjs';
import { ShowDto } from '../models/show.dto';
import { environment } from '../../environments/environment';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root',
})
export class BookmarkService {
  private apiUrl = `${environment.apiUrl}/shows`;
  private bookmarksSubject = new BehaviorSubject<ShowDto[]>([]);
  private bookmarksCache = new Set<number>();
  private isLoading = false;
  private bookmarks$: Observable<ShowDto[]>;

  constructor(private http: HttpClient, private authService: AuthService) {
    this.bookmarks$ = this.bookmarksSubject.asObservable().pipe(shareReplay(1));

    this.loadBookmarks();
  }

  private getAuthHeaders(): HttpHeaders {
    return new HttpHeaders({
      'Content-Type': 'application/json',
    });
  }

  get bookmarksObservable(): Observable<ShowDto[]> {
    return this.bookmarks$;
  }

  get bookmarks(): ShowDto[] {
    return this.bookmarksSubject.value;
  }

  loadBookmarks(): void {
    if (this.isLoading) return;

    this.isLoading = true;
    this.http
      .get<ShowDto[]>(`${this.apiUrl}/bookmarks`, {
        withCredentials: true,
        headers: this.getAuthHeaders(),
      })
      .pipe(
        tap({
          finalize: () => (this.isLoading = false),
        })
      )
      .subscribe({
        next: (bookmarks) => {
          this.bookmarksSubject.next(bookmarks);

          this.bookmarksCache = new Set(bookmarks.map((b) => b.id));
        },
        error: (error) => {
          console.error('Error loading bookmarks:', error);
          this.bookmarksSubject.next([]);
          this.bookmarksCache.clear();
        },
      });
  }

  addBookmark(show: ShowDto): Observable<ShowDto> {
    return this.http
      .post<ShowDto>(
        `${this.apiUrl}/${show.id}/bookmark`,
        {},
        {
          withCredentials: true,
          headers: this.getAuthHeaders(),
        }
      )
      .pipe(
        tap((newBookmark) => {
          const currentBookmarks = this.bookmarksSubject.value;
          this.bookmarksSubject.next([...currentBookmarks, newBookmark]);
          this.bookmarksCache.add(newBookmark.id);
        })
      );
  }

  removeBookmark(showId: number): Observable<void> {
    return this.http
      .delete<void>(`${this.apiUrl}/${showId}/bookmark`, {
        withCredentials: true,
        headers: this.getAuthHeaders(),
      })
      .pipe(
        tap(() => {
          const currentBookmarks = this.bookmarksSubject.value;
          this.bookmarksSubject.next(
            currentBookmarks.filter((bookmark) => bookmark.id !== showId)
          );
          this.bookmarksCache.delete(showId);
        })
      );
  }

  isBookmarked(showId: number): boolean {
    return this.bookmarksCache.has(showId);
  }

  updateShowsBookmarkStatus(shows: ShowDto[]): ShowDto[] {
    return shows.map((show) => ({
      ...show,
      isBookmarked: this.isBookmarked(show.id),
    }));
  }
}
