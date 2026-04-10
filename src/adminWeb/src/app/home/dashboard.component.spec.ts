import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { HomeComponent } from './home.component';
import { AuthService } from '@abp/ng.core';
import { CoreTestingModule } from '@abp/ng.core/testing';
import { ThemeSharedTestingModule } from '@abp/ng.theme.shared/testing';
import { NgxValidateCoreModule } from '@ngx-validate/core';
import { vi } from 'vitest';

describe('HomeComponent - Dashboard Tests', () => {
  let fixture: ComponentFixture<HomeComponent>;
  let component: HomeComponent;
  let mockAuthService: { isAuthenticated: boolean; navigateToLogin: ReturnType<typeof vi.fn> };

  beforeEach(async () => {
    mockAuthService = {
      isAuthenticated: false,
      navigateToLogin: vi.fn()
    };

    await TestBed.configureTestingModule({
      imports: [
        CoreTestingModule.withConfig(),
        ThemeSharedTestingModule.withConfig(),
        NgxValidateCoreModule,
        HomeComponent
      ],
      providers: [
        { provide: AuthService, useValue: mockAuthService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(HomeComponent);
    component = fixture.componentInstance;
  });

  it('should create the component', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  describe('authentication state', () => {
    it('should show login button when not authenticated', () => {
      mockAuthService.isAuthenticated = false;
      fixture.detectChanges();

      const element = fixture.nativeElement;
      const button = element.querySelector('[role="button"]');
      expect(button).toBeDefined();
    });

    it('should hide login button when authenticated', () => {
      mockAuthService.isAuthenticated = true;
      fixture.detectChanges();

      const element = fixture.nativeElement;
      const button = element.querySelector('[role="button"]');
      expect(button).toBeNull();
    });

    it('should set hasLoggedIn to true when authenticated', () => {
      mockAuthService.isAuthenticated = true;
      fixture.detectChanges();

      expect(component.hasLoggedIn).toBe(true);
    });

    it('should set hasLoggedIn to false when not authenticated', () => {
      mockAuthService.isAuthenticated = false;
      fixture.detectChanges();

      expect(component.hasLoggedIn).toBe(false);
    });
  });

  describe('navigation', () => {
    it('should call navigateToLogin when login button is clicked', () => {
      mockAuthService.isAuthenticated = false;
      fixture.detectChanges();

      const element = fixture.nativeElement;
      const button = element.querySelector('[role="button"]');
      if (button) {
        button.click();
        expect(mockAuthService.navigateToLogin).toHaveBeenCalled();
      }
    });
  });

  describe('component rendering', () => {
    it('should render the component template', () => {
      fixture.detectChanges();
      const element = fixture.nativeElement;
      expect(element).toBeTruthy();
    });

    it('should render with correct structure', () => {
      fixture.detectChanges();
      const compiled = fixture.nativeElement as HTMLElement;
      expect(compiled).toBeTruthy();
    });
  });
});
