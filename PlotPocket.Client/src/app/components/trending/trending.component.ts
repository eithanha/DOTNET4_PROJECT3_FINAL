import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ShowCardComponent } from '../show-card/show-card.component';
import { ShowService } from '../../services/show.service';
import { ShowDto } from '../../models/show.dto';
import {
  Subject,
  takeUntil,
  debounceTime,
  distinctUntilChanged,
  switchMap,
  map,
  Observable,
} from 'rxjs';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { BookmarkService } from '../../services/bookmark.service';

@Component({
  selector: 'app-trending',
  standalone: true,
  imports: [CommonModule, ShowCardComponent, FormsModule],
  templateUrl: './trending.component.html',
})
export class TrendingComponent implements OnInit, OnDestroy {
  trendingShows: ShowDto[] = [];
  loading = false;
  error: string | null = null;
  selectedFilter: 'all' | 'movies' | 'tv-shows' = 'all';
  searchQuery: string = '';
  private destroy$ = new Subject<void>();
  private searchSubject$ = new Subject<string>();
  //shows!: ShowDto[];

  constructor(
    private showService: ShowService,
    private bookmarkService: BookmarkService,
    private router: Router,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.setupSearch();
    this.loadTrendingShows();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private setupSearch(): void {
    this.searchSubject$
      .pipe(
        takeUntil(this.destroy$),
        debounceTime(300),
        distinctUntilChanged(),
        switchMap((query) => {
          if (!query.trim()) {
            return this.showService.getTrendingShows();
          }
          this.loading = true;
          return this.showService.searchShows(query);
        })
      )
      .subscribe({
        next: (shows) => {
          this.trendingShows = shows;
          this.loading = false;
          this.error = null;
        },
        error: (err) => {
          console.error('Error Searching Shows:', err);
          this.error = 'Failed To Search Shows. Please Try Again Later.';
          this.loading = false;
        },
      });
  }

  onSearch(): void {
    this.searchSubject$.next(this.searchQuery);
  }

  private loadTrendingShows(): void {
    this.loading = true;
    this.error = null;

    let request$: Observable<ShowDto[]>;
    switch (this.selectedFilter) {
      case 'movies':
        request$ = this.showService.getTrendingMovies();
        break;
      case 'tv-shows':
        request$ = this.showService.getTrendingTvShows();
        break;
      default:
        request$ = this.showService.getTrendingShows();
    }

    request$
      .pipe(
        map((shows) => this.bookmarkService.updateShowsBookmarkStatus(shows)),
        takeUntil(this.destroy$)
      )
      .subscribe({
        next: (shows) => {
          this.trendingShows = shows;
          this.loading = false;
        },
        error: (error) => {
          console.error('Error Loading Trending Shows:', error);
          this.error = 'Failed To Load Trending Shows. Please Try Again Later.';
          this.loading = false;
        },
      });
  }

  onFilterChange(filter: 'all' | 'movies' | 'tv-shows'): void {
    if (this.selectedFilter !== filter) {
      this.selectedFilter = filter;
      this.loadTrendingShows();
    }
  }

  onWatchlistToggle(show: ShowDto): void {
    if (show.isWatchlisted) {
      this.removeFromWatchlist(show);
    } else {
      this.addToWatchlist(show);
    }
  }

  onBookmarkToggle(show: ShowDto): void {
    if (!this.authService.isLoggedIn()) {
      this.router.navigate(['/login']);
      return;
    }

    if (this.bookmarkService.isBookmarked(show.id)) {
      this.bookmarkService.removeBookmark(show.id).subscribe({
        next: () => {
          const index = this.trendingShows.findIndex((s) => s.id === show.id);
          if (index !== -1) {
            this.trendingShows[index] = {
              ...this.trendingShows[index],
              isBookmarked: false,
            };
          }
        },
        error: (error) => console.error('Error Removing Bookmark:', error),
      });
    } else {
      this.bookmarkService.addBookmark(show).subscribe({
        next: (updatedShow) => {
          const index = this.trendingShows.findIndex((s) => s.id === show.id);
          if (index !== -1) {
            this.trendingShows[index] = updatedShow;
          }
        },
        error: (error) => console.error('Error Adding Bookmark:', error),
      });
    }
  }

  private addToWatchlist(show: ShowDto): void {
    this.showService.addToWatchlist(show).subscribe({
      next: () => {
        show.isWatchlisted = true;
      },
      error: (err) => {
        console.error('Error Adding To Watchlist:', err);
        this.error = 'Failed To Add Show To Watchlist';
      },
    });
  }

  private removeFromWatchlist(show: ShowDto): void {
    this.showService.removeFromWatchlist(show).subscribe({
      next: () => {
        show.isWatchlisted = false;
      },
      error: (err) => {
        console.error('Error Removing From Watchlist:', err);
        this.error = 'Failed To Remove Show From Watchlist';
      },
    });
  }

  getFilterDisplayName(): string {
    switch (this.selectedFilter) {
      case 'all':
        return 'Trending Shows';
      case 'movies':
        return 'Trending Movies';
      case 'tv-shows':
        return 'Trending TV Shows';
      default:
        return 'Trending';
    }
  }
}
