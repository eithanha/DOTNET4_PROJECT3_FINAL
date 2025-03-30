import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ShowCardComponent } from '../show-card/show-card.component';
import { ShowDto } from '../../models/show.dto';
import { ShowService } from '../../services/show.service';

@Component({
  selector: 'app-movies',
  standalone: true,
  imports: [CommonModule, ShowCardComponent],
  templateUrl: './movies.component.html',
  styleUrl: './movies.component.css',
})
export class MoviesComponent implements OnInit {
  movies: ShowDto[] = [];
  loading = true;
  error: string | null = null;

  constructor(private showService: ShowService) {}

  ngOnInit(): void {
    this.loadMovies();
  }

  loadMovies(): void {
    this.loading = true;
    this.error = null;
    this.showService.getMovies().subscribe({
      next: (movies: ShowDto[]) => {
        this.movies = movies;
        this.loading = false;
      },
      error: (error: Error) => {
        this.error = 'Failed to load movies. Please try again later.';
        this.loading = false;
        console.error('Error loading movies:', error);
      },
    });
  }

  onWatchlistToggle(show: ShowDto): void {
    this.showService.toggleWatchlist(show).subscribe({
      next: (updatedShow: ShowDto) => {
        const index = this.movies.findIndex((m) => m.id === updatedShow.id);
        if (index !== -1) {
          this.movies[index] = updatedShow;
        }
      },
      error: (error: Error) => {
        console.error('Error toggling watchlist:', error);
      },
    });
  }
}
