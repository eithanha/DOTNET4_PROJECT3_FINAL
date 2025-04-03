import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ShowDto } from '../../models/show.dto';
import { ShowService } from '../../services/show.service';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-show-card',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './show-card.component.html',
  styleUrl: './show-card.component.css',
})
export class ShowCardComponent {
  @Input() show!: ShowDto;
  @Output() watchlistToggle = new EventEmitter<ShowDto>();
  @Output() bookmarkToggle = new EventEmitter<ShowDto>();

  private readonly placeholderImage =
    'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iNTAwIiBoZWlnaHQ9IjUwMCIgdmlld0JveD0iMCAwIDUwMCA1MDAiIGZpbGw9Im5vbmUiIHhtbG5zPSJodHRwOi8vd3d3LnczLm9yZy8yMDAwL3N2ZyI+PHJlY3Qgd2lkdGg9IjUwMCIgaGVpZ2h0PSI1MDAiIGZpbGw9IiNFNUU1RTUiLz48dGV4dCB4PSI1MCUiIHk9IjUwJSIgZG9taW5hbnQtYmFzZWxpbmU9Im1pZGRsZSIgdGV4dC1hbmNob3I9Im1pZGRsZSIgZmlsbD0iI0FBQUFBQSIgZm9udC1zaXplPSI0MCIgZm9udC1mYW1pbHk9IkFyaWFsIj5ObyBJbWFnZTwvdGV4dD48L3N2Zz4=';
  private readonly tmdbImageBaseUrl = 'https://image.tmdb.org/t/p/w500';

  constructor(
    private showService: ShowService,
    private router: Router,
    public authService: AuthService
  ) {}

  onWatchlistToggle(): void {
    if (!this.authService.isLoggedIn()) {
      this.router.navigate(['/login']);
      return;
    }
    this.watchlistToggle.emit(this.show);
  }

  onBookmarkToggle(): void {
    if (!this.authService.isLoggedIn()) {
      this.router.navigate(['/login']);
      return;
    }
    this.bookmarkToggle.emit(this.show);
  }

  getImageUrl(): string {
    const posterPath = this.show.PosterPath || this.show.posterPath;
    if (!posterPath) return this.placeholderImage;

    if (posterPath.startsWith('http')) {
      return posterPath;
    }

    return `${this.tmdbImageBaseUrl}${posterPath}`;
  }
}
