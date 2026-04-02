import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { AuthService, User, LoginResponse } from './auth.service';
import { environment } from '../../environments/environment';

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;
  let routerSpy: jasmine.SpyObj<Router>;

  beforeEach(() => {
    const spy = jasmine.createSpyObj('Router', ['navigate']);

    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        AuthService,
        { provide: Router, useValue: spy }
      ]
    });

    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
    routerSpy = TestBed.inject(Router) as jasmine.SpyObj<Router>;
    
    // Clear localStorage before each test
    if (typeof window !== 'undefined' && typeof localStorage !== 'undefined') {
      localStorage.clear();
    }
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('login', () => {
    it('should login successfully with mock credentials', (done) => {
      // Arrange
      const mockResponse: LoginResponse = {
        token: 'mock-token',
        user: {
          id: '1',
          email: 'user@familymeet.com',
          name: 'Usuário Teste',
          avatar: 'https://picsum.photos/seed/user1/200/200.jpg',
          provider: 'email'
        }
      };

      // Act
      service.login('user@familymeet.com', 'password').subscribe(response => {
        // Assert
        expect(response.token).toBe(mockResponse.token);
        expect(response.user.email).toBe(mockResponse.user.email);
        expect(service.isLoggedIn).toBe(true);
        expect(service.currentUser).toEqual(mockResponse.user);
        done();
      });
    });

    it('should fail login with invalid credentials', (done) => {
      // Act & Assert
      service.login('invalid@email.com', 'wrongpassword').subscribe({
        next: () => fail('Should have failed'),
        error: (error) => {
          expect(error.message).toBe('Credenciais inválidas');
          done();
        }
      });
    });
  });

  describe('loginWithGoogle', () => {
    it('should handle Google login with mock', (done) => {
      // Act
      service.loginWithGoogle().subscribe(response => {
        // Assert
        expect(response.user.email).toBe('user@gmail.com');
        expect(response.user.provider).toBe('google');
        expect(service.isLoggedIn).toBe(true);
        done();
      });
    });
  });

  describe('register', () => {
    it('should register new user successfully', (done) => {
      // Arrange
      const userData = {
        email: 'newuser@test.com',
        password: 'password123',
        firstName: 'New',
        lastName: 'User'
      };

      // Act
      service.register(userData).subscribe(response => {
        // Assert
        expect(response.user.email).toBe(userData.email);
        expect(response.user.name).toBe('New User');
        expect(service.isLoggedIn).toBe(true);
        done();
      });
    });
  });

  describe('logout', () => {
    it('should logout and clear user data', () => {
      // Arrange
      // Set some user data first
      service.login('user@familymeet.com', 'password');

      // Act
      service.logout();

      // Assert
      expect(service.isLoggedIn).toBe(false);
      expect(service.currentUser).toBeNull();
      expect(routerSpy.navigate).toHaveBeenCalledWith(['/login']);
    });
  });

  describe('getCurrentUser', () => {
    it('should return current user', () => {
      // Arrange
      const mockUser: User = {
        id: '1',
        email: 'user@test.com',
        name: 'Test User',
        avatar: 'test.jpg',
        provider: 'email'
      };

      // Act
      service.login('user@familymeet.com', 'password');

      // Assert
      expect(service.currentUser).toEqual(mockUser);
    });

    it('should return null when no user is logged in', () => {
      // Act
      const currentUser = service.currentUser;

      // Assert
      expect(currentUser).toBeNull();
    });
  });

  describe('isLoggedIn', () => {
    it('should return true when user is logged in', () => {
      // Act
      service.login('user@familymeet.com', 'password');

      // Assert
      expect(service.isLoggedIn).toBe(true);
    });

    it('should return false when user is not logged in', () => {
      // Act
      const isLoggedIn = service.isLoggedIn;

      // Assert
      expect(isLoggedIn).toBe(false);
    });
  });

  describe('refreshToken', () => {
    it('should refresh token successfully', (done) => {
      // Arrange
      const mockResponse: LoginResponse = {
        token: 'new-refreshed-token',
        user: {
          id: '1',
          email: 'user@familymeet.com',
          name: 'Usuário Teste',
          avatar: 'https://picsum.photos/seed/user1/200/200.jpg',
          provider: 'email'
        }
      };

      // Set initial token
      service.login('user@familymeet.com', 'password');

      // Act
      service.refreshToken().subscribe(response => {
        // Assert
        expect(response.token).toBe(mockResponse.token);
        done();
      });
    });

    it('should fail refresh when no token is available', (done) => {
      // Act & Assert
      service.refreshToken().subscribe({
        next: () => fail('Should have failed'),
        error: (error) => {
          expect(error.message).toBe('No token available');
          done();
        }
      });
    });
  });
});
