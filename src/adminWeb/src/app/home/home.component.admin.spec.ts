import { TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { FormsModule } from '@angular/forms';
import { By } from '@angular/platform-browser';
import { HomeComponent } from './home.component';

describe('HomeComponent', () => {
  let component: HomeComponent;
  let fixture: ComponentFixture<HomeComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        RouterTestingModule,
        HttpClientTestingModule,
        FormsModule
      ],
      declarations: [HomeComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(HomeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize with empty statistics', () => {
    expect(component.totalUsers).toBe(0);
    expect(component.activeGroups).toBe(0);
    expect(component.totalMessages).toBe(0);
    expect(component.activeCalls).toBe(0);
  });

  it('should load statistics on initialization', () => {
    // Act
    component.ngOnInit();

    // Assert
    expect(component.isLoading).toBe(true);
  });

  it('should handle loading state correctly', () => {
    // Arrange
    component.isLoading = true;
    fixture.detectChanges();

    // Act
    const loadingElement = fixture.debugElement.query(By.css('.loading-spinner'));

    // Assert
    expect(loadingElement).toBeTruthy();
  });

  it('should display statistics when loaded', () => {
    // Arrange
    component.totalUsers = 100;
    component.activeGroups = 25;
    component.totalMessages = 5000;
    component.activeCalls = 5;
    component.isLoading = false;
    fixture.detectChanges();

    // Act
    const statsElements = fixture.debugElement.queryAll(By.css('.stat-card'));

    // Assert
    expect(statsElements.length).toBe(4);
    expect(statsElements[0].nativeElement.textContent).toContain('100');
    expect(statsElements[1].nativeElement.textContent).toContain('25');
    expect(statsElements[2].nativeElement.textContent).toContain('5000');
    expect(statsElements[3].nativeElement.textContent).toContain('5');
  });

  it('should have refresh functionality', () => {
    // Arrange
    spyOn(component, 'loadStatistics');

    // Act
    component.refreshStats();

    // Assert
    expect(component.loadStatistics).toHaveBeenCalled();
  });

  it('should handle error states gracefully', () => {
    // Arrange
    component.isLoading = false;
    component.hasError = true;
    fixture.detectChanges();

    // Act
    const errorElement = fixture.debugElement.query(By.css('.error-message'));

    // Assert
    expect(errorElement).toBeTruthy();
    expect(errorElement.nativeElement.textContent).toContain('Failed to load statistics');
  });
});
