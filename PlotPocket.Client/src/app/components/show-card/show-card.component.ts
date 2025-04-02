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

  private placeholderImage = '/assets/general-img-portrait.png';

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

    return `https://image.tmdb.org/t/p/w500${posterPath}`;
  }
}
