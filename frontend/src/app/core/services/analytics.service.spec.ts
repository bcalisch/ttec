import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { AnalyticsService } from './analytics.service';
import { OutOfSpecItem, CoverageCell, TrendPoint } from '../models';

describe('AnalyticsService', () => {
  let service: AnalyticsService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()]
    });
    service = TestBed.inject(AnalyticsService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('getOutOfSpec should GET /api/projects/:id/analytics/out-of-spec', () => {
    const mockItems: OutOfSpecItem[] = [
      {
        id: '1', testTypeName: 'pH', value: 9.5, threshold: 8.5,
        severity: 2, longitude: -100.5, latitude: 40.1,
        timestamp: '2024-06-15T10:00:00Z'
      }
    ];

    service.getOutOfSpec('p1').subscribe(items => {
      expect(items.length).toBe(1);
      expect(items[0].testTypeName).toBe('pH');
      expect(items[0].severity).toBe(2);
    });

    const req = httpMock.expectOne('/api/projects/p1/analytics/out-of-spec');
    expect(req.request.method).toBe('GET');
    req.flush(mockItems);
  });

  it('getCoverage should GET /api/projects/:id/analytics/coverage', () => {
    const mockResponse = {
      gridSize: 100,
      cells: [
        { minLon: -100.5, minLat: 40.0, maxLon: -100.4, maxLat: 40.1, count: 5 },
        { minLon: -100.4, minLat: 40.0, maxLon: -100.3, maxLat: 40.1, count: 0 }
      ] as CoverageCell[],
      gaps: 1
    };

    service.getCoverage('p1').subscribe(response => {
      expect(response.gridSize).toBe(100);
      expect(response.cells.length).toBe(2);
      expect(response.gaps).toBe(1);
      expect(response.cells[0].count).toBe(5);
    });

    const req = httpMock.expectOne('/api/projects/p1/analytics/coverage');
    expect(req.request.method).toBe('GET');
    req.flush(mockResponse);
  });

  it('getTrends should GET /api/projects/:id/analytics/trends with interval param', () => {
    const mockTrends: TrendPoint[] = [
      { period: '2024-06-15', testTypeName: 'pH', avg: 7.2, min: 6.8, max: 7.6 }
    ];

    service.getTrends('p1', 'day').subscribe(points => {
      expect(points.length).toBe(1);
      expect(points[0].testTypeName).toBe('pH');
      expect(points[0].avg).toBe(7.2);
    });

    const req = httpMock.expectOne(r =>
      r.url === '/api/projects/p1/analytics/trends' && r.params.get('interval') === 'day'
    );
    expect(req.request.method).toBe('GET');
    req.flush(mockTrends);
  });

  it('getTrends should default to day interval', () => {
    service.getTrends('p1').subscribe();

    const req = httpMock.expectOne(r =>
      r.url === '/api/projects/p1/analytics/trends' && r.params.get('interval') === 'day'
    );
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  it('getTrends should pass week interval', () => {
    service.getTrends('p1', 'week').subscribe();

    const req = httpMock.expectOne(r =>
      r.url === '/api/projects/p1/analytics/trends' && r.params.get('interval') === 'week'
    );
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });
});
