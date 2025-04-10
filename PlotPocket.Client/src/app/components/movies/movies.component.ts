import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ShowCardComponent } from '../show-card/show-card.component';
import { ShowDto } from '../../models/show.dto';
import { ShowService } from '../../services/show.service';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { BookmarkService } from '../../services/bookmark.service';
import { Subject, Observable } from 'rxjs';
import {
  debounceTime,
  distinctUntilChanged,
  map,
  takeUntil,
} from 'rxjs/operators';

@Component({
  selector: 'app-movies',
  standalone: true,
  imports: [CommonModule, ShowCardComponent, FormsModule],
  templateUrl: './movies.component.html',
  styleUrl: './movies.component.css',
})
export class MoviesComponent implements OnInit, OnDestroy {
  movies: ShowDto[] = [];
  loading = false;
  error: string | null = null;
  searchQuery = '';
  private searchSubject$ = new Subject<string>();
  selectedFilter: 'popular' | 'now-playing' | 'top-rated' = 'popular';
  private destroy$ = new Subject<void>();

  constructor(
    private showService: ShowService,
    private bookmarkService: BookmarkService,
    private router: Router,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.loadMovies();
    this.setupSearch();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private loadMovies(): void {
    this.loading = true;
    this.error = null;

    let request$: Observable<ShowDto[]>;
    switch (this.selectedFilter) {
      case 'popular':
        request$ = this.showService.getPopularMovies();
        break;
      case 'top-rated':
        request$ = this.showService.getTopRatedMovies();
        break;
      default:
        request$ = this.showService.getNowPlayingMovies();
    }

    request$
      .pipe(
        map((shows) => this.bookmarkService.updateShowsBookmarkStatus(shows)),
        takeUntil(this.destroy$)
      )
      .subscribe({
        next: (shows) => {
          this.movies = shows;
          this.loading = false;
        },
        error: (error) => {
          console.error('Error Loading Movies:', error);
          this.error = 'Failed To Load Movies. Please Try Again Later.';
          this.loading = false;
        },
      });
  }

  private setupSearch(): void {
    this.searchSubject$
      .pipe(takeUntil(this.destroy$), debounceTime(300), distinctUntilChanged())
      .subscribe(() => {
        this.onSearch();
      });
  }

  onSearch(): void {
    if (!this.searchQuery.trim()) {
      this.loadMovies();
      return;
    }

    this.loading = true;
    this.error = null;

    this.showService
      .searchMovies(this.searchQuery)
      .pipe(
        map((shows) => this.bookmarkService.updateShowsBookmarkStatus(shows)),
        takeUntil(this.destroy$)
      )
      .subscribe({
        next: (shows) => {
          this.movies = shows;
          this.loading = false;
        },
        error: (error) => {
          console.error('Error Searching Movies:', error);
          this.error = 'Failed To Search Movies. Please Try Again Later.';
          this.loading = false;
        },
      });
  }

  onFilterChange(filter: 'popular' | 'now-playing' | 'top-rated'): void {
    this.selectedFilter = filter;
    this.loadMovies();
  }

  onBookmarkToggle(show: ShowDto): void {
    if (!this.authService.isLoggedIn()) {
      this.router.navigate(['/login']);
      return;
    }

    if (this.bookmarkService.isBookmarked(show.id)) {
      this.bookmarkService.removeBookmark(show.id).subscribe({
        next: () => {
          const index = this.movies.findIndex((s) => s.id === show.id);
          if (index !== -1) {
            this.movies[index] = {
              ...this.movies[index],
              isBookmarked: false,
            };
          }
        },
        error: (error) => console.error('Error Removing Bookmark:', error),
      });
    } else {
      this.bookmarkService.addBookmark(show).subscribe({
        next: (updatedShow) => {
          const index = this.movies.findIndex((s) => s.id === show.id);
          if (index !== -1) {
            this.movies[index] = updatedShow;
          }
        },
        error: (error) => console.error('Error Adding Bookmark:', error),
      });
    }
  }

  onWatchlistToggle(show: ShowDto): void {
    if (!this.authService.isLoggedIn()) {
      this.router.navigate(['/login']);
      return;
    }

    if (show.isWatchlisted) {
      this.showService.removeFromWatchlist(show).subscribe({
        next: () => {
          const index = this.movies.findIndex((s) => s.id === show.id);
          if (index !== -1) {
            this.movies[index] = {
              ...this.movies[index],
              isWatchlisted: false,
            };
          }
        },
        error: (error) =>
          console.error('Error Removing From Watchlist:', error),
      });
    } else {
      this.showService.addToWatchlist(show).subscribe({
        next: () => {
          const index = this.movies.findIndex((s) => s.id === show.id);
          if (index !== -1) {
            this.movies[index] = {
              ...this.movies[index],
              isWatchlisted: true,
            };
          }
        },
        error: (error) => console.error('Error Adding To Watchlist:', error),
      });
    }
  }

  getFilterDisplayName(): string {
    switch (this.selectedFilter) {
      case 'now-playing':
        return 'Now Playing Movies';
      case 'top-rated':
        return 'Top Rated Movies';
      case 'popular':
        return 'Popular Movies';
      default:
        return 'Movies';
    }
  }
}
