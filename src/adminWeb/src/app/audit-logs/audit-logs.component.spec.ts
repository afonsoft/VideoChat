import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AuditLogsComponent } from './audit-logs.component';

describe('AuditLogsComponent', () => {
  let component: AuditLogsComponent;
  let fixture: ComponentFixture<AuditLogsComponent>;
  let routerMock: jasmine.SpyObj<Router>;

  beforeEach(async () => {
    const routerSpy = jasmine.createSpyObj('Router', ['navigate']);

    await TestBed.configureTestingModule({
      imports: [
        AuditLogsComponent,
        CommonModule,
        FormsModule,
        ReactiveFormsModule
      ],
      providers: [
        FormBuilder,
        { provide: Router, useValue: routerSpy }
      ]
    })
    .compileComponents();

    routerMock = TestBed.inject(Router) as jasmine.SpyObj<Router>;
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(AuditLogsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize search form with empty values', () => {
    expect(component.searchForm.value).toEqual({
      startDate: '',
      endDate: '',
      userName: '',
      action: '',
      logLevel: ''
    });
  });

  it('should validate form before search', () => {
    component.searchForm.patchValue({ userName: 'test' });
    expect(component.searchForm.valid).toBeTrue();
  });

  it('should reset form and clear results on reset', () => {
    component.auditLogs = [{ id: '1', executionTime: new Date(), userName: 'test', action: 'test', message: 'test', logLevel: 'Information', clientIpAddress: '127.0.0.1' }];
    component.searchForm.patchValue({ userName: 'test' });
    component.currentPage = 1;
    component.totalPages = 5;

    component.onReset();

    expect(component.searchForm.value).toEqual({
      startDate: '',
      endDate: '',
      userName: '',
      action: '',
      logLevel: ''
    });
    expect(component.auditLogs).toEqual([]);
    expect(component.currentPage).toBe(0);
    expect(component.totalPages).toBe(0);
  });

  it('should handle pagination correctly', () => {
    component.totalPages = 3;
    component.currentPage = 1;

    component.previousPage();
    expect(component.currentPage).toBe(0);

    component.nextPage();
    expect(component.currentPage).toBe(1);
  });

  it('should not navigate beyond page boundaries', () => {
    component.totalPages = 2;
    component.currentPage = 0;

    component.previousPage();
    expect(component.currentPage).toBe(0);

    component.currentPage = 1;
    component.nextPage();
    expect(component.currentPage).toBe(1);
  });

  it('should handle search with valid form', () => {
    component.searchForm.patchValue({ userName: 'test' });

    component.onSearch();

    expect(component.loading).toBeFalse();
  });

  it('should not search with invalid form', () => {
    component.searchForm.patchValue({ userName: 'test' });
    component.searchForm.setErrors({ invalid: true });

    component.onSearch();

    expect(component.loading).toBeFalse();
  });
});
