import { TestBed, ComponentFixture } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { AnalyticsComponent } from './analytics.component';
import { OutOfSpecItem } from '../../core/models';

describe('AnalyticsComponent', () => {
  let component: AnalyticsComponent;
  let fixture: ComponentFixture<AnalyticsComponent>;
  let httpMock: HttpTestingController;

  const mockOutOfSpec: OutOfSpecItem[] = [
    {
      id: '1', testTypeName: 'pH', value: 9.5, threshold: 8.5,
      severity: 3, longitude: -100.5, latitude: 40.1,
      timestamp: '2024-06-15T10:00:00Z'
    },
    {
      id: '2', testTypeName: 'Turbidity', value: 25.0, threshold: 10.0,
      severity: 2, longitude: -100.6, latitude: 40.2,
      timestamp: '2024-06-16T10:00:00Z'
    }
  ];

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AnalyticsComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()]
    }).compileComponents();

    httpMock = TestBed.inject(HttpTestingController);
    fixture = TestBed.createComponent(AnalyticsComponent);
    component = fixture.componentInstance;
  });

  afterEach(() => httpMock.verify());

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load out-of-spec and trends on init when projectId is set', () => {
    component.projectId = 'p1';
    fixture.detectChanges();

    const oosReq = httpMock.expectOne('/api/projects/p1/analytics/out-of-spec');
    oosReq.flush(mockOutOfSpec);

    const trendsReq = httpMock.expectOne(r => r.url === '/api/projects/p1/analytics/trends');
    trendsReq.flush([]);

    expect(component.outOfSpecItems.length).toBe(2);
    expect(component.loadingOos).toBeFalse();
  });

  it('should render out-of-spec table rows', () => {
    component.projectId = 'p1';
    fixture.detectChanges();

    httpMock.expectOne('/api/projects/p1/analytics/out-of-spec').flush(mockOutOfSpec);
    httpMock.expectOne(r => r.url === '/api/projects/p1/analytics/trends').flush([]);

    fixture.detectChanges();

    const rows = fixture.nativeElement.querySelectorAll('tbody tr');
    expect(rows.length).toBe(2);
    expect(rows[0].textContent).toContain('pH');
    expect(rows[1].textContent).toContain('Turbidity');
  });

  it('should show empty message when no out-of-spec results', () => {
    component.projectId = 'p1';
    fixture.detectChanges();

    httpMock.expectOne('/api/projects/p1/analytics/out-of-spec').flush([]);
    httpMock.expectOne(r => r.url === '/api/projects/p1/analytics/trends').flush([]);

    fixture.detectChanges();

    const emptyRow = fixture.nativeElement.querySelector('tbody td');
    expect(emptyRow.textContent).toContain('No out-of-spec results');
  });

  it('should emit locateOnMap when onLocate is called', () => {
    let emitted: {latitude: number, longitude: number} | null = null;
    component.locateOnMap.subscribe((loc: {latitude: number, longitude: number}) => emitted = loc);

    component.onLocate(mockOutOfSpec[0]);

    expect(emitted).toBeTruthy();
    expect(emitted!.latitude).toBe(40.1);
    expect(emitted!.longitude).toBe(-100.5);
  });

  it('should emit locateOnMap when a table row is clicked', () => {
    component.projectId = 'p1';
    fixture.detectChanges();

    httpMock.expectOne('/api/projects/p1/analytics/out-of-spec').flush(mockOutOfSpec);
    httpMock.expectOne(r => r.url === '/api/projects/p1/analytics/trends').flush([]);
    fixture.detectChanges();

    let emitted: {latitude: number, longitude: number} | null = null;
    component.locateOnMap.subscribe((loc: {latitude: number, longitude: number}) => emitted = loc);

    const row = fixture.nativeElement.querySelector('tbody tr');
    row.click();

    expect(emitted).toBeTruthy();
    expect(emitted!.latitude).toBe(40.1);
    expect(emitted!.longitude).toBe(-100.5);
  });

  it('severityLabel should return correct labels', () => {
    expect(component.severityLabel(3)).toBe('High');
    expect(component.severityLabel(4)).toBe('High');
    expect(component.severityLabel(2)).toBe('Medium');
    expect(component.severityLabel(1)).toBe('Low');
    expect(component.severityLabel(0)).toBe('Low');
  });

  it('severityBadgeClass should return correct classes', () => {
    expect(component.severityBadgeClass(3)).toContain('bg-red-100');
    expect(component.severityBadgeClass(2)).toContain('bg-amber-100');
    expect(component.severityBadgeClass(1)).toContain('bg-yellow-100');
  });

  it('severityRowClass should return bg-red-50 for high severity', () => {
    expect(component.severityRowClass(3, false)).toBe('bg-red-50');
    expect(component.severityRowClass(3, true)).toBe('bg-red-50');
  });

  it('severityRowClass should alternate for low severity', () => {
    expect(component.severityRowClass(1, true)).toBe('bg-gray-50');
    expect(component.severityRowClass(1, false)).toBe('');
  });
});
