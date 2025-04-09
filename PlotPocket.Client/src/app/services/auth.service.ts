import {
  HttpClient,
  HttpHeaders,
  HttpErrorResponse,
} from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import {
  BehaviorSubject,
  Observable,
  tap,
  catchError,
  throwError,
  map,
} from 'rxjs';
import { User } from '../models/user';
import { EmailLoginDetails } from '../models/email-login-details';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly apiUrl = '/api/auth';
  private userSubject = new BehaviorSubject<User | null>(null);
  private user$ = this.userSubject.asObservable();

  constructor(private http: HttpClient, private router: Router) {
    this.checkAuthStatus();
  }

  private getHeaders(): HttpHeaders {
    return new HttpHeaders()
      .set('Content-Type', 'application/json')
      .set('Accept', 'application/json');
  }

  private handleError(error: HttpErrorResponse): Observable<never> {
    if (error.status === 401 || error.status === 403) {
      this.clearFrontendCredentials();
      this.router.navigate(['/login'], {
        queryParams: { returnUrl: this.router.url },
      });
    }
    return throwError(() => error);
  }

  private get<T>(url: string): Observable<T> {
    return this.http
      .get<T>(url, {
        withCredentials: true,
        headers: this.getHeaders(),
      })
      .pipe(
        map((response) => response as T),
        catchError(this.handleError.bind(this))
      );
  }

  private post<T>(url: string, body: any): Observable<T> {
    return this.http
      .post<T>(url, body, {
        withCredentials: true,
        headers: this.getHeaders(),
      })
      .pipe(
        map((response) => response as T),
        catchError(this.handleError.bind(this))
      );
  }

  getCurrentUser(): Observable<User | null> {
    return this.user$;
  }

  isLoggedIn(): boolean {
    return this.userSubject.value !== null;
  }

  login(details: EmailLoginDetails): Observable<User> {
    return this.post<User>(`${this.apiUrl}/login`, details).pipe(
      tap((user) => {
        this.userSubject.next(user);
      })
    );
  }

  register(details: EmailLoginDetails): Observable<User> {
    return this.post<User>(`${this.apiUrl}/register`, details).pipe(
      tap((user) => {
        this.userSubject.next(user);
      })
    );
  }

  logout(): Observable<void> {
    return this.post<void>(`${this.apiUrl}/logout`, {}).pipe(
      tap(() => {
        this.clearFrontendCredentials();
      })
    );
  }

  clearFrontendCredentials(): void {
    this.userSubject.next(null);
  }

  private checkAuthStatus(): void {
    this.get<User>(`${this.apiUrl}/status`)
      .pipe(
        tap((user) => {
          if (user) {
            this.userSubject.next(user);
          } else {
            this.clearFrontendCredentials();
          }
        }),
        catchError((error) => {
          if (error.status === 401 || error.status === 403) {
            this.clearFrontendCredentials();
            this.router.navigate(['/login'], {
              queryParams: { returnUrl: this.router.url },
            });
          }
          return throwError(() => error);
        })
      )
      .subscribe();
  }
}
