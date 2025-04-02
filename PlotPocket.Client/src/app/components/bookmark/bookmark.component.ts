import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ShowCardComponent } from '../show-card/show-card.component';
import { ShowDto } from '../../models/show.dto';
import { AuthService } from '../../services/auth.service';
import { BookmarkService } from '../../services/bookmark.service';
import { Subject, debounceTime, distinctUntilChanged, takeUntil } from 'rxjs';

@Component({
  selector: 'app-bookmark',
  standalone: true,
  imports: [CommonModule, FormsModule, ShowCardComponent],
  templateUrl: './bookmark.component.html',
  styleUrls: ['./bookmark.component.css'],
})
export class BookmarkComponent implements OnInit, OnDestroy {
  bookmarks: ShowDto[] = [];
  private allBookmarks: ShowDto[] = [];
  loading = false;
  error: string | null = null;
  searchQuery = '';
  private searchSubject$ = new Subject<string>();
  private destroy$ = new Subject<void>();

  constructor(
    private bookmarkService: BookmarkService,
    private router: Router,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    console.log('BookmarkComponent initialized');
    if (!this.authService.isLoggedIn()) {
      console.log('User not logged in, redirecting to login page');
      this.router.navigate(['/login'], {
        queryParams: { returnUrl: '/bookmark' },
      });
      return;
    }
    console.log('User is logged in, loading bookmarks');
    this.loadBookmarks();
    this.setupSearch();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private setupSearch(): void {
    this.searchSubject$
      .pipe(takeUntil(this.destroy$), debounceTime(300), distinctUntilChanged())
      .subscribe(() => {
        this.filterBookmarks();
      });
  }

  private filterBookmarks(): void {
    if (this.searchQuery.trim()) {
      const searchTerm = this.searchQuery.toLowerCase().trim();
      this.bookmarks = this.allBookmarks.filter(
        (show) =>
          show.title.toLowerCase().includes(searchTerm) ||
          show.overview.toLowerCase().includes(searchTerm)
      );
    } else {
      this.bookmarks = [...this.allBookmarks];
    }
  }

  loadBookmarks(): void {
    this.loading = true;
    this.error = null;

    this.bookmarkService.bookmarksObservable.subscribe({
      next: (bookmarks) => {
        this.allBookmarks = bookmarks;
        this.bookmarks = [...bookmarks];
        this.loading = false;
      },
      error: (error) => {
        if (error.status === 401) {
          this.router.navigate(['/login'], {
            queryParams: { returnUrl: '/bookmark' },
          });
        } else {
          this.error = 'Failed to load bookmarks. Please try again later.';
          this.loading = false;
          console.error('Error loading bookmarks:', error);
        }
      },
    });
  }

  onBookmarkToggle(show: ShowDto): void {
    if (!this.authService.isLoggedIn()) {
      this.router.navigate(['/login'], {
        queryParams: { returnUrl: '/bookmark' },
      });
      return;
    }

    if (show.isBookmarked) {
      this.bookmarkService.removeBookmark(show.id).subscribe({
        next: () => {
          this.allBookmarks = this.allBookmarks.filter((b) => b.id !== show.id);
          this.bookmarks = this.bookmarks.filter((b) => b.id !== show.id);
        },
        error: (error) => {
          if (error.status === 401) {
            this.router.navigate(['/login'], {
              queryParams: { returnUrl: '/bookmark' },
            });
          } else {
            console.error('Error removing bookmark:', error);
          }
        },
      });
    }
  }

  onSearch(): void {
    this.searchSubject$.next(this.searchQuery);
  }
}
