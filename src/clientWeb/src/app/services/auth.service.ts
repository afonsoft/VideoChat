import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable, of, throwError } from 'rxjs';
import { delay, map, catchError } from 'rxjs/operators';

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
  private readonly API_URL = 'http://localhost:5000/api';
  private readonly TOKEN_KEY = 'familymeet_token';
  private readonly USER_KEY = 'familymeet_user';

  private currentUserSubject = new BehaviorSubject<User | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(private router: Router) {
    this.loadStoredUser();
  }

  get currentUser(): User | null {
    return this.currentUserSubject.value;
  }

  get token(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  get isLoggedIn(): boolean {
    return !!this.token;
  }

  async login(email: string, password: string): Promise<LoginResponse> {
    try {
      const response = await fetch(`${this.API_URL}/auth/login`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ email, password })
      });

      if (!response.ok) {
        throw new Error('Login failed');
      }

      const data: LoginResponse = await response.json();
      this.setUserData(data);
      return data;
    } catch (error) {
      throw error;
    }
  }

  async loginWithGoogle(): Promise<LoginResponse> {
    try {
      // In a real implementation, this would use Google OAuth flow
      // For now, simulate Google login
      const response = await fetch(`${this.API_URL}/auth/google`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ 
          idToken: 'mock_google_token' 
        })
      });

      if (!response.ok) {
        throw new Error('Google login failed');
      }

      const data: LoginResponse = await response.json();
      this.setUserData(data);
      return data;
    } catch (error) {
      throw error;
    }
  }

  async refreshToken(): Promise<LoginResponse> {
    try {
      const response = await fetch(`${this.API_URL}/auth/refresh`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${this.token}`
        },
        body: JSON.stringify({ token: this.token })
      });

      if (!response.ok) {
        throw new Error('Token refresh failed');
      }

      const data: LoginResponse = await response.json();
      this.setUserData(data);
      return data;
    } catch (error) {
      this.logout();
      throw error;
    }
  }

  logout(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.USER_KEY);
    this.currentUserSubject.next(null);
    this.router.navigate(['/login']);
  }

  private setUserData(data: LoginResponse): void {
    localStorage.setItem(this.TOKEN_KEY, data.token);
    localStorage.setItem(this.USER_KEY, JSON.stringify(data.user));
    this.currentUserSubject.next(data.user);
  }

  private loadStoredUser(): void {
    const token = localStorage.getItem(this.TOKEN_KEY);
    const userStr = localStorage.getItem(this.USER_KEY);

    if (token && userStr) {
      try {
        const user = JSON.parse(userStr);
        this.currentUserSubject.next(user);
      } catch (error) {
        this.clearStorage();
      }
    }
  }

  private clearStorage(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.USER_KEY);
    this.currentUserSubject.next(null);
  }

  // Mock data for development
  mockLogin(email: string, password: string): Promise<LoginResponse> {
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
