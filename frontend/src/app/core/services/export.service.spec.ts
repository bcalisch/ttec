import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ExportService } from './export.service';

describe('ExportService', () => {
  let service: ExportService;
  let httpMock: HttpTestingController;

  let createObjectURLSpy: jasmine.Spy;
  let revokeObjectURLSpy: jasmine.Spy;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()]
    });
    service = TestBed.inject(ExportService);
    httpMock = TestBed.inject(HttpTestingController);

    createObjectURLSpy = spyOn(window.URL, 'createObjectURL').and.returnValue('blob:mock-url');
    revokeObjectURLSpy = spyOn(window.URL, 'revokeObjectURL');
  });

  afterEach(() => httpMock.verify());

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('exportCsv should GET blob from /api/projects/:id/export/csv', () => {
    const clickSpy = jasmine.createSpy('click');
    spyOn(document, 'createElement').and.returnValue({ click: clickSpy, href: '', download: '' } as any);

    service.exportCsv('p1');

    const req = httpMock.expectOne('/api/projects/p1/export/csv');
    expect(req.request.method).toBe('GET');
    expect(req.request.responseType).toBe('blob');
    req.flush(new Blob(['test,data'], { type: 'text/csv' }));

    expect(createObjectURLSpy).toHaveBeenCalled();
    expect(clickSpy).toHaveBeenCalled();
    expect(revokeObjectURLSpy).toHaveBeenCalled();
  });

  it('exportGeoJson should GET blob from /api/projects/:id/export/geojson', () => {
    const clickSpy = jasmine.createSpy('click');
    spyOn(document, 'createElement').and.returnValue({ click: clickSpy, href: '', download: '' } as any);

    service.exportGeoJson('p1');

    const req = httpMock.expectOne('/api/projects/p1/export/geojson');
    expect(req.request.method).toBe('GET');
    expect(req.request.responseType).toBe('blob');
    req.flush(new Blob(['{"type":"FeatureCollection"}'], { type: 'application/geo+json' }));

    expect(createObjectURLSpy).toHaveBeenCalled();
    expect(clickSpy).toHaveBeenCalled();
    expect(revokeObjectURLSpy).toHaveBeenCalled();
  });

  it('exportCsv should pass filter params', () => {
    const clickSpy = jasmine.createSpy('click');
    spyOn(document, 'createElement').and.returnValue({ click: clickSpy, href: '', download: '' } as any);

    service.exportCsv('p1', { from: '2024-01-01', to: '2024-12-31', testTypeId: 'tt1', status: 'Fail' });

    const req = httpMock.expectOne(r => r.url === '/api/projects/p1/export/csv');
    expect(req.request.params.get('from')).toBe('2024-01-01');
    expect(req.request.params.get('to')).toBe('2024-12-31');
    expect(req.request.params.get('testTypeId')).toBe('tt1');
    expect(req.request.params.get('status')).toBe('Fail');
    req.flush(new Blob([''], { type: 'text/csv' }));
  });

  it('exportGeoJson should pass filter params', () => {
    const clickSpy = jasmine.createSpy('click');
    spyOn(document, 'createElement').and.returnValue({ click: clickSpy, href: '', download: '' } as any);

    service.exportGeoJson('p1', { from: '2024-06-01', status: 'Pass' });

    const req = httpMock.expectOne(r => r.url === '/api/projects/p1/export/geojson');
    expect(req.request.params.get('from')).toBe('2024-06-01');
    expect(req.request.params.get('status')).toBe('Pass');
    req.flush(new Blob(['{}'], { type: 'application/geo+json' }));
  });
});
