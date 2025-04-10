import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ShowCardComponent } from '../show-card/show-card.component';
import { ShowDto } from '../../models/show.dto';
import { ShowService } from '../../services/show.service';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth.service';
import { Router } from '@angular/router';
import { BookmarkService } from '../../services/bookmark.service';
import { Subject, Observable } from 'rxjs';
import { map, takeUntil } from 'rxjs/operators';

@Component({
  selector: 'app-tv-shows',
  standalone: true,
  imports: [CommonModule, ShowCardComponent, FormsModule],
  templateUrl: './tv-shows.component.html',
  styleUrl: './tv-shows.component.css',
})
export class TvShowsComponent implements OnInit, OnDestroy {
  tvShows: ShowDto[] = [];
  loading = false;
  error: string | null = null;
  searchQuery = '';
  selectedFilter: 'popular' | 'top-rated' | 'on-air' = 'popular';
  private destroy$ = new Subject<void>();

  constructor(
    private showService: ShowService,
    private bookmarkService: BookmarkService,
    private router: Router,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.loadTvShows();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private loadTvShows(): void {
    this.loading = true;
    this.error = null;

    let request$: Observable<ShowDto[]>;
    switch (this.selectedFilter) {
      case 'popular':
        request$ = this.showService.getPopularTvShows();
        break;
      case 'top-rated':
        request$ = this.showService.getTopRatedTvShows();
        break;
      default:
        request$ = this.showService.getOnAirTvShows();
    }

    request$
      .pipe(
        map((shows) => this.bookmarkService.updateShowsBookmarkStatus(shows)),
        takeUntil(this.destroy$)
      )
      .subscribe({
        next: (shows) => {
          this.tvShows = shows;
          this.loading = false;
        },
        error: (error) => {
          console.error('Error Loading TV Shows:', error);
          this.error = 'Failed To Load TV Shows. Please Try Again Later.';
          this.loading = false;
        },
      });
  }

  onSearch(): void {
    if (!this.searchQuery.trim()) {
      this.loadTvShows();
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
          this.tvShows = shows;
          this.loading = false;
        },
        error: (error) => {
          console.error('Error searching TV shows:', error);
          this.error = 'Failed to search TV shows. Please try again later.';
          this.loading = false;
        },
      });
  }

  onFilterChange(filter: 'on-air' | 'top-rated' | 'popular'): void {
    this.selectedFilter = filter;
    this.loadTvShows();
  }

  onBookmarkToggle(show: ShowDto): void {
    if (!this.authService.isLoggedIn()) {
      this.router.navigate(['/login']);
      return;
    }

    if (this.bookmarkService.isBookmarked(show.id)) {
      this.bookmarkService.removeBookmark(show.id).subscribe({
        next: () => {
          const index = this.tvShows.findIndex((s) => s.id === show.id);
          if (index !== -1) {
            this.tvShows[index] = {
              ...this.tvShows[index],
              isBookmarked: false,
            };
          }
        },
        error: (error) => console.error('Error Removing Bookmark:', error),
      });
    } else {
      this.bookmarkService.addBookmark(show).subscribe({
        next: (updatedShow) => {
          const index = this.tvShows.findIndex((s) => s.id === show.id);
          if (index !== -1) {
            this.tvShows[index] = updatedShow;
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
          const index = this.tvShows.findIndex((s) => s.id === show.id);
          if (index !== -1) {
            this.tvShows[index] = {
              ...this.tvShows[index],
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
          const index = this.tvShows.findIndex((s) => s.id === show.id);
          if (index !== -1) {
            this.tvShows[index] = {
              ...this.tvShows[index],
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
      case 'on-air':
        return 'TV Shows Airing Today';
      case 'top-rated':
        return 'Top Rated TV Shows';
      case 'popular':
      default:
        return 'Popular TV Shows';
    }
  }
}
