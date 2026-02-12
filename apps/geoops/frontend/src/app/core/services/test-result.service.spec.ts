import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestResultService } from './test-result.service';

describe('TestResultService', () => {
  let service: TestResultService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()]
    });
    service = TestBed.inject(TestResultService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('create should POST to /api/projects/:id/test-results', () => {
    const req = {
      testTypeId: 'tt1',
      timestamp: '2024-01-01T00:00:00Z',
      value: 7.5,
      longitude: -100.1,
      latitude: 40.2
    };
    service.create('p1', req).subscribe();
    const httpReq = httpMock.expectOne('/api/projects/p1/test-results');
    expect(httpReq.request.method).toBe('POST');
    expect(httpReq.request.body.testTypeId).toBe('tt1');
    expect(httpReq.request.body.value).toBe(7.5);
    httpReq.flush({});
  });

  it('batchIngest should POST to /api/projects/:id/ingest/test-results', () => {
    const req = {
      idempotencyKey: 'key1',
      items: [{
        testTypeId: 'tt1',
        timestamp: '2024-01-01T00:00:00Z',
        value: 7.5,
        longitude: -100.1,
        latitude: 40.2
      }]
    };
    service.batchIngest('p1', req).subscribe();
    const httpReq = httpMock.expectOne('/api/projects/p1/ingest/test-results');
    expect(httpReq.request.method).toBe('POST');
    expect(httpReq.request.body.idempotencyKey).toBe('key1');
    expect(httpReq.request.body.items.length).toBe(1);
    httpReq.flush({});
  });
});
