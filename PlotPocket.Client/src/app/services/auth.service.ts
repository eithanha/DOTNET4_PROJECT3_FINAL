import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { User } from '../models/user';
import { EmailLoginDetails } from '../models/email-login-details';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private _http = inject(HttpClient);
  private _apiUrl = 'http://localhost:5000';

  private _userKey: string = 'curUser';

  private _userSubject: BehaviorSubject<User | null> =
    new BehaviorSubject<User | null>(null);

  public user$: Observable<User | null> = this._userSubject.asObservable();

  constructor() {
    const userJsonRaw = localStorage.getItem(this._userKey);

    const user: User | null = userJsonRaw ? JSON.parse(userJsonRaw) : null;

    this._userSubject.next(user);
  }

  public get user(): User | null {
    return this._userSubject.value;
  }

  public register(details: EmailLoginDetails): Observable<User> {
    return this._http
      .post<User>(`${this._apiUrl}/api/auth/register`, details)
      .pipe(
        tap((user) => {
          localStorage.setItem(this._userKey, JSON.stringify(user));

          this._userSubject.next(user);
        })
      );
  }

  public login(details: EmailLoginDetails): Observable<User> {
    return this._http
      .post<User>(`${this._apiUrl}/api/auth/login`, details)
      .pipe(
        tap((user) => {
          localStorage.setItem(this._userKey, JSON.stringify(user));

          this._userSubject.next(user);
        })
      );
  }

  public isLoggedIn(): boolean {
    return !!this._userSubject.value;
  }

  public logout(): Observable<any> {
    return this._http.post<any>(`${this._apiUrl}/api/auth/logout`, {}).pipe(
      tap(() => {
        localStorage.removeItem(this._userKey);

        this._userSubject.next(null);
      })
    );
  }

  public clearFrontendCredentials(): void {
    localStorage.removeItem(this._userKey);

    this._userSubject.next(null);
  }
}
