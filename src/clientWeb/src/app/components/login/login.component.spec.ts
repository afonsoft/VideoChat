import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FormsModule } from '@angular/forms';
import { By } from '@angular/platform-browser';
import { RouterTestingModule } from '@angular/router/testing';
import { of } from 'rxjs';

import { LoginComponent } from './login.component';
import { AuthService } from '../services/auth.service';

describe('LoginComponent', () => {
  let component: LoginComponent;
  let fixture: ComponentFixture<LoginComponent>;
  let authServiceSpy: jasmine.SpyObj<AuthService>;

  beforeEach(async () => {
    const spy = jasmine.createSpyObj('AuthService', [
      'login',
      'loginWithGoogle',
      'isLoggedIn'
    ]);

    await TestBed.configureTestingModule({
      imports: [
        FormsModule,
        RouterTestingModule
      ],
      declarations: [LoginComponent],
      providers: [
        { provide: AuthService, useValue: spy }
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;
    authServiceSpy = TestBed.inject(AuthService) as jasmine.SpyObj<AuthService>;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should have form fields initialized', () => {
    expect(component.email).toBe('');
    expect(component.password).toBe('');
    expect(component.isLoading).toBe(false);
    expect(component.errorMessage).toBe('');
  });

  it('should show loading state during login', () => {
    // Arrange
    authServiceSpy.login.and.returnValue(of({} as any));

    // Act
    component.onLogin();
    fixture.detectChanges();

    // Assert
    expect(component.isLoading).toBe(true);
    const button = fixture.debugElement.query(By.css('button[type="submit"]'));
    expect(button.nativeElement.disabled).toBe(true);
  });

  it('should call authService.login with form data', () => {
    // Arrange
    const mockResponse = {
      token: 'test-token',
      user: {
        id: '1',
        email: 'test@example.com',
        name: 'Test User'
      }
    };

    component.email = 'test@example.com';
    component.password = 'password123';
    authServiceSpy.login.and.returnValue(of(mockResponse));

    // Act
    component.onLogin();

    // Assert
    expect(authServiceSpy.login).toHaveBeenCalledWith('test@example.com', 'password123');
    expect(component.isLoading).toBe(false);
  });

  it('should show error message on login failure', () => {
    // Arrange
    const errorMessage = 'Invalid credentials';
    component.email = 'test@example.com';
    component.password = 'wrongpassword';
    authServiceSpy.login.and.returnValue(
      new Promise((_, reject) => reject(new Error(errorMessage)))
    );

    // Act
    component.onLogin();

    // Assert
    expect(component.errorMessage).toBe(errorMessage);
    expect(component.isLoading).toBe(false);
  });

  it('should call authService.loginWithGoogle on Google login', () => {
    // Arrange
    const mockResponse = {
      token: 'google-token',
      user: {
        id: '2',
        email: 'user@gmail.com',
        name: 'Google User'
      }
    };

    authServiceSpy.loginWithGoogle.and.returnValue(of(mockResponse));

    // Act
    component.onGoogleLogin();

    // Assert
    expect(authServiceSpy.loginWithGoogle).toHaveBeenCalled();
    expect(component.isLoading).toBe(false);
  });

  it('should disable form when loading', () => {
    // Arrange
    component.isLoading = true;
    fixture.detectChanges();

    // Act
    const emailInput = fixture.debugElement.query(By.css('input[type="email"]'));
    const passwordInput = fixture.debugElement.query(By.css('input[type="password"]'));
    const submitButton = fixture.debugElement.query(By.css('button[type="submit"]'));

    // Assert
    expect(emailInput.nativeElement.disabled).toBe(false); // Email should remain enabled
    expect(passwordInput.nativeElement.disabled).toBe(false); // Password should remain enabled
    expect(submitButton.nativeElement.disabled).toBe(true);
  });

  it('should show error message when present', () => {
    // Arrange
    component.errorMessage = 'Test error message';
    fixture.detectChanges();

    // Act
    const errorElement = fixture.debugElement.query(By.css('.error-message'));

    // Assert
    expect(errorElement).toBeTruthy();
    expect(errorElement.nativeElement.textContent).toContain('Test error message');
  });

  it('should not show error message when empty', () => {
    // Arrange
    component.errorMessage = '';
    fixture.detectChanges();

    // Act
    const errorElement = fixture.debugElement.query(By.css('.error-message'));

    // Assert
    expect(errorElement).toBeFalsy();
  });

  it('should have Google login button', () => {
    // Act
    const googleButton = fixture.debugElement.query(By.css('.google-btn'));

    // Assert
    expect(googleButton).toBeTruthy();
    expect(googleButton.nativeElement.textContent).toContain('Continuar com Google');
  });

  it('should have signup link', () => {
    // Act
    const signupLink = fixture.debugElement.query(By.css('.signup-link a'));

    // Assert
    expect(signupLink).toBeTruthy();
    expect(signupLink.nativeElement.textContent).toContain('Registre-se');
  });

  it('should validate form before submission', () => {
    // Arrange
    component.email = '';
    component.password = '';
    fixture.detectChanges();

    // Act
    const form = fixture.debugElement.query(By.css('.login-form'));
    const submitButton = fixture.debugElement.query(By.css('button[type="submit"]'));

    // Assert
    expect(form.nativeElement.checkValidity()).toBe(false);
    expect(submitButton.nativeElement.disabled).toBe(true);
  });

  it('should enable submit button when form is valid', () => {
    // Arrange
    component.email = 'test@example.com';
    component.password = 'password123';
    fixture.detectChanges();

    // Act
    const form = fixture.debugElement.query(By.css('.login-form'));
    const submitButton = fixture.debugElement.query(By.css('button[type="submit"]'));

    // Assert
    expect(form.nativeElement.checkValidity()).toBe(true);
    expect(submitButton.nativeElement.disabled).toBe(false);
  });
});
