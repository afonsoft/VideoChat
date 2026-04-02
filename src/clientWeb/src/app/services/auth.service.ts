import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, of, throwError } from 'rxjs';
import { delay, map, catchError } from 'rxjs/operators';
import { environment } from '../../environments/environment';

export interface User {
  id: string;
  email: string;
  name: string;
  avatar?: string;
  provider?: string;
}

export interface LoginResponse {
  token: string;
  user: User;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = environment.apiUrl;
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  private isAuthenticatedSubject = new BehaviorSubject<boolean>(false);

  currentUser$ = this.currentUserSubject.asObservable();
  isAuthenticated$ = this.isAuthenticatedSubject.asObservable();

  constructor(
    private router: Router,
    private http: HttpClient
  ) {
    this.checkAuthStatus();
  }

  private checkAuthStatus(): void {
    if (typeof window !== 'undefined' && typeof localStorage !== 'undefined') {
      const token = localStorage.getItem('token');
      const user = localStorage.getItem('user');

      if (token && user) {
        try {
          const parsedUser = JSON.parse(user);
          this.currentUserSubject.next(parsedUser);
          this.isAuthenticatedSubject.next(true);
        } catch (e) {
          this.logout();
        }
      }
    }
  }

  login(email: string, password: string): Observable<LoginResponse> {
    if (environment.enableMock) {
      return this.mockLogin(email, password);
    }

    return this.http.post<LoginResponse>(`${this.apiUrl}/api/auth/login`, { email, password })
      .pipe(
        map(response => {
          if (typeof window !== 'undefined' && typeof localStorage !== 'undefined') {
            localStorage.setItem('token', response.token);
            localStorage.setItem('user', JSON.stringify(response.user));
          }
          this.currentUserSubject.next(response.user);
          this.isAuthenticatedSubject.next(true);
          return response;
        }),
        catchError(error => {
          console.error('Login error:', error);
          return throwError(() => new Error('Login failed. Please check your credentials.'));
        })
      );
  }

  loginWithGoogle(): Observable<any> {
    if (environment.enableMock) {
      return this.mockGoogleLogin();
    }

    return this.http.get(`${this.apiUrl}/api/auth/google/url`)
      .pipe(
        map((response: any) => {
          // Redirect to Google OAuth
          window.location.href = response.authUrl;
          return response;
        }),
        catchError(error => {
          console.error('Google login error:', error);
          return throwError(() => new Error('Google login failed.'));
        })
      );
  }

  handleGoogleCallback(code: string, state: string): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/api/auth/google/callback`, { code, state })
      .pipe(
        map(response => {
          if (typeof window !== 'undefined' && typeof localStorage !== 'undefined') {
            localStorage.setItem('token', response.token);
            localStorage.setItem('user', JSON.stringify(response.user));
          }
          this.currentUserSubject.next(response.user);
          this.isAuthenticatedSubject.next(true);
          return response;
        }),
        catchError(error => {
          console.error('Google callback error:', error);
          return throwError(() => new Error('Google authentication failed.'));
        })
      );
  }

  register(userData: {
    email: string;
    password: string;
    firstName: string;
    lastName: string;
  }): Observable<LoginResponse> {
    if (environment.enableMock) {
      return this.mockRegister(userData);
    }

    return this.http.post<LoginResponse>(`${this.apiUrl}/api/auth/register`, userData)
      .pipe(
        map(response => {
          if (typeof window !== 'undefined' && typeof localStorage !== 'undefined') {
            localStorage.setItem('token', response.token);
            localStorage.setItem('user', JSON.stringify(response.user));
          }
          this.currentUserSubject.next(response.user);
          this.isAuthenticatedSubject.next(true);
          return response;
        }),
        catchError(error => {
          console.error('Registration error:', error);
          return throwError(() => new Error('Registration failed. Please try again.'));
        })
      );
  }

  logout(): void {
    if (typeof window !== 'undefined' && typeof localStorage !== 'undefined') {
      localStorage.removeItem('token');
      localStorage.removeItem('user');
    }
    this.currentUserSubject.next(null);
    this.isAuthenticatedSubject.next(false);
    this.router.navigate(['/login']);
  }

  get isLoggedIn(): boolean {
    return this.isAuthenticatedSubject.value;
  }

  get currentUser(): User | null {
    return this.currentUserSubject.value;
  }

  private setUserData(response: LoginResponse): void {
    if (typeof window !== 'undefined' && typeof localStorage !== 'undefined') {
      localStorage.setItem('token', response.token);
      localStorage.setItem('user', JSON.stringify(response.user));
    }
    this.currentUserSubject.next(response.user);
    this.isAuthenticatedSubject.next(true);
  }

  private mockLogin(email: string, password: string): Observable<LoginResponse> {
    return new Observable(observer => {
      setTimeout(() => {
        if (email === 'user@familymeet.com' && password === 'password') {
          const mockUser: User = {
            id: '1',
            email: 'user@familymeet.com',
            name: 'Usuário Teste',
            avatar: 'https://picsum.photos/seed/user1/200/200.jpg',
            provider: 'email'
          };

          const mockResponse: LoginResponse = {
            token: 'mock_jwt_token_' + Date.now(),
            user: mockUser
          };

          this.setUserData(mockResponse);
          observer.next(mockResponse);
          observer.complete();
        } else {
          observer.error(new Error('Credenciais inválidas'));
        }
      }, 1000);
    });
  }

  private mockGoogleLogin(): Observable<LoginResponse> {
    return new Observable(observer => {
      setTimeout(() => {
        const mockUser: User = {
          id: '2',
          email: 'user@gmail.com',
          name: 'Google User',
          avatar: 'https://picsum.photos/seed/googleuser/200/200.jpg',
          provider: 'google'
        };

        const mockResponse: LoginResponse = {
          token: 'mock_google_token_' + Date.now(),
          user: mockUser
        };

        this.setUserData(mockResponse);
        observer.next(mockResponse);
        observer.complete();
      }, 1000);
    });
  }

  private mockRegister(userData: {
    email: string;
    password: string;
    firstName: string;
    lastName: string;
  }): Observable<LoginResponse> {
    return new Observable(observer => {
      setTimeout(() => {
        const mockUser: User = {
          id: '3',
          email: userData.email,
          name: `${userData.firstName} ${userData.lastName}`,
          avatar: 'https://picsum.photos/seed/newuser/200/200.jpg',
          provider: 'email'
        };

        const mockResponse: LoginResponse = {
          token: 'mock_register_token_' + Date.now(),
          user: mockUser
        };

        this.setUserData(mockResponse);
        observer.next(mockResponse);
        observer.complete();
      }, 1000);
    });
  }

  refreshToken(): Observable<LoginResponse> {
    const token = typeof window !== 'undefined' && typeof localStorage !== 'undefined'
      ? localStorage.getItem('token')
      : null;
    if (!token) {
      return throwError(() => new Error('No token available'));
    }

    return this.http.post<LoginResponse>(`${this.apiUrl}/api/auth/refresh`, { token })
      .pipe(
        map(response => {
          if (typeof window !== 'undefined' && typeof localStorage !== 'undefined') {
            localStorage.setItem('token', response.token);
          }
          return response;
        }),
        catchError(error => {
          console.error('Token refresh error:', error);
          this.logout();
          return throwError(() => new Error('Session expired. Please login again.'));
        })
      );
  }

}
