import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ShowCardComponent } from '../show-card/show-card.component';
import { ShowService } from '../../services/show.service';
import { ShowDto } from '../../models/show.dto';

@Component({
  selector: 'app-trending',
  standalone: true,
  imports: [CommonModule, ShowCardComponent],
  templateUrl: './trending.component.html',
})
export class TrendingComponent implements OnInit {
  trendingShows: ShowDto[] = [];
  isLoading = true;
  error: string | null = null;

  constructor(private showService: ShowService) {}

  ngOnInit(): void {
    this.loadTrendingShows();
  }

  private loadTrendingShows(): void {
    this.isLoading = true;
    this.error = null;
    this.showService.getTrendingShows().subscribe({
      next: (shows) => {
        this.trendingShows = shows;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error loading trending shows:', err);
        this.error = 'Failed to load trending shows. Please try again later.';
        this.isLoading = false;
      },
    });
  }

  onWatchlistToggle(show: ShowDto): void {
    if (show.isWatchlisted) {
      this.removeFromWatchlist(show);
    } else {
      this.addToWatchlist(show);
    }
  }

  private addToWatchlist(show: ShowDto): void {
    this.showService.addToWatchlist(show).subscribe({
      next: () => {
        show.isWatchlisted = true;
      },
      error: (err) => {
        console.error('Error adding to watchlist:', err);
        this.error = 'Failed to add show to watchlist';
      },
    });
  }

  private removeFromWatchlist(show: ShowDto): void {
    this.showService.removeFromWatchlist(show).subscribe({
      next: () => {
        show.isWatchlisted = false;
      },
      error: (err) => {
        console.error('Error removing from watchlist:', err);
        this.error = 'Failed to remove show from watchlist';
      },
    });
  }
}
