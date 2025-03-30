import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ShowDto } from '../../models/show.dto';
import { AuthService } from '../../services/auth.service';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-show-card',
  templateUrl: './show-card.component.html',
  styleUrls: ['./show-card.component.css'],
  standalone: true,
  imports: [CommonModule],
})
export class ShowCardComponent {
  @Input() show!: ShowDto;
  @Output() watchlistToggle = new EventEmitter<ShowDto>();

  constructor(public authService: AuthService) {}

  toggleWatchlist(): void {
    this.watchlistToggle.emit(this.show);
  }

  isLoggedIn(): boolean {
    return this.authService.isLoggedIn();
  }

  getImageUrl(path: string | null): string {
    if (!path) return 'assets/placeholder-poster.jpg';
    return `${environment.tmdbImageBaseUrl}${path}`;
  }
}
