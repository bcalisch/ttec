import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { FeatureService } from './feature.service';

describe('FeatureService', () => {
  let service: FeatureService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()]
    });
    service = TestBed.inject(FeatureService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('getFeatures should GET with no params', () => {
    service.getFeatures('p1').subscribe(r => {
      expect(r.tests.length).toBe(0);
    });
    const req = httpMock.expectOne('/api/projects/p1/features');
    expect(req.request.method).toBe('GET');
    req.flush({ tests: [], observations: [], sensors: [], totalTests: 0, page: 1, pageSize: 50 });
  });

  it('getFeatures should pass query params', () => {
    service.getFeatures('p1', {
      bbox: '1,2,3,4',
      from: '2024-01-01',
      to: '2024-12-31',
      types: 'tests',
      testTypeId: 'tt1',
      status: 'Pass',
      page: 2,
      pageSize: 25
    }).subscribe();

    const req = httpMock.expectOne(r =>
      r.url === '/api/projects/p1/features' &&
      r.params.get('bbox') === '1,2,3,4' &&
      r.params.get('from') === '2024-01-01' &&
      r.params.get('to') === '2024-12-31' &&
      r.params.get('types') === 'tests' &&
      r.params.get('testTypeId') === 'tt1' &&
      r.params.get('status') === 'Pass' &&
      r.params.get('page') === '2' &&
      r.params.get('pageSize') === '25'
    );
    expect(req.request.method).toBe('GET');
    req.flush({ tests: [], observations: [], sensors: [], totalTests: 0, page: 2, pageSize: 25 });
  });

  it('getFeatures should omit undefined params', () => {
    service.getFeatures('p1', { bbox: '1,2,3,4' }).subscribe();
    const req = httpMock.expectOne(r => r.url === '/api/projects/p1/features');
    expect(req.request.params.has('from')).toBeFalse();
    expect(req.request.params.has('to')).toBeFalse();
    expect(req.request.params.get('bbox')).toBe('1,2,3,4');
    req.flush({ tests: [], observations: [], sensors: [], totalTests: 0, page: 1, pageSize: 50 });
  });
});
