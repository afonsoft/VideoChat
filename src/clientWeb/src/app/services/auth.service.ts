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

  login(email: string, password: string): Observable<LoginResponse> {
    if (environment.enableMock) {
      return this.mockLogin(email, password);
    }

    return this.http.post<LoginResponse>(`${this.apiUrl}/api/auth/login`, { email, password })
      .pipe(
        map(response => {
          localStorage.setItem('token', response.token);
          localStorage.setItem('user', JSON.stringify(response.user));
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
        map(response => {
          // Redirect to Google OAuth
          window.location.href = response['authUrl'];
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
          localStorage.setItem('token', response.token);
          localStorage.setItem('user', JSON.stringify(response.user));
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
          localStorage.setItem('token', response.token);
          localStorage.setItem('user', JSON.stringify(response.user));
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
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    this.currentUserSubject.next(null);
    this.isAuthenticatedSubject.next(false);
    this.router.navigate(['/login']);
  }

  refreshToken(): Observable<LoginResponse> {
    const token = localStorage.getItem('token');
    if (!token) {
      return throwError(() => new Error('No token available'));
    }

    return this.http.post<LoginResponse>(`${this.apiUrl}/api/auth/refresh`, { token })
      .pipe(
        map(response => {
          localStorage.setItem('token', response.token);
          return response;
        }),
        catchError(error => {
          console.error('Token refresh error:', error);
          this.logout();
          return throwError(() => new Error('Session expired. Please login again.'));
        })
      );
  }

  getCurrentUser(): User | null {
    return this.currentUserSubject.value;
    return new Promise((resolve, reject) => {
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
          resolve(mockResponse);
        } else {
          reject(new Error('Credenciais inválidas'));
        }
      }, 1000);
    });
  }
}
