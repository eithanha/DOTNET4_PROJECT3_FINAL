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
import { map, takeUntil } from 'rxjs/operators';

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
      case 'now-playing':
        request$ = this.showService.getNowPlayingMovies();
        break;
      case 'top-rated':
        request$ = this.showService.getTopRatedMovies();
        break;
      default:
        request$ = this.showService.getPopularMovies();
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
          console.error('Error loading movies:', error);
          this.error = 'Failed to load movies. Please try again later.';
          this.loading = false;
        },
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
      .searchShows(this.searchQuery)
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
          console.error('Error searching movies:', error);
          this.error = 'Failed to search movies. Please try again later.';
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
        error: (error) => console.error('Error removing bookmark:', error),
      });
    } else {
      this.bookmarkService.addBookmark(show).subscribe({
        next: (updatedShow) => {
          const index = this.movies.findIndex((s) => s.id === show.id);
          if (index !== -1) {
            this.movies[index] = updatedShow;
          }
        },
        error: (error) => console.error('Error adding bookmark:', error),
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
          console.error('Error removing from watchlist:', error),
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
        error: (error) => console.error('Error adding to watchlist:', error),
      });
    }
  }
}
