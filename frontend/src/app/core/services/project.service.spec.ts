import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ProjectService } from './project.service';
import { ProjectStatus } from '../models';

describe('ProjectService', () => {
  let service: ProjectService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()]
    });
    service = TestBed.inject(ProjectService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('getAll should GET /api/projects', () => {
    service.getAll().subscribe(projects => {
      expect(projects.length).toBe(1);
      expect(projects[0].name).toBe('Test');
    });
    const req = httpMock.expectOne('/api/projects');
    expect(req.request.method).toBe('GET');
    req.flush([{ id: '1', name: 'Test', client: 'C', status: 0 }]);
  });

  it('getById should GET /api/projects/:id', () => {
    service.getById('abc').subscribe(p => {
      expect(p.id).toBe('abc');
    });
    const req = httpMock.expectOne('/api/projects/abc');
    expect(req.request.method).toBe('GET');
    req.flush({ id: 'abc', name: 'P', client: 'C', status: 0 });
  });

  it('create should POST /api/projects', () => {
    const body = { name: 'New', client: 'Client', status: ProjectStatus.Active };
    service.create(body).subscribe(p => {
      expect(p.name).toBe('New');
    });
    const req = httpMock.expectOne('/api/projects');
    expect(req.request.method).toBe('POST');
    expect(req.request.body.name).toBe('New');
    req.flush({ id: '2', name: 'New', client: 'Client', status: 1 });
  });

  it('update should PUT /api/projects/:id', () => {
    service.update('x', { name: 'Updated' }).subscribe(p => {
      expect(p.name).toBe('Updated');
    });
    const req = httpMock.expectOne('/api/projects/x');
    expect(req.request.method).toBe('PUT');
    req.flush({ id: 'x', name: 'Updated', client: 'C', status: 0 });
  });

  it('getBoundaries should GET /api/projects/:id/boundaries', () => {
    service.getBoundaries('p1').subscribe(b => {
      expect(b.length).toBe(1);
    });
    const req = httpMock.expectOne('/api/projects/p1/boundaries');
    expect(req.request.method).toBe('GET');
    req.flush([{ id: 'b1', projectId: 'p1', geoJson: '{}' }]);
  });

  it('createBoundary should POST /api/projects/:id/boundaries', () => {
    service.createBoundary('p1', '{"type":"Polygon"}').subscribe(b => {
      expect(b.geoJson).toBe('{"type":"Polygon"}');
    });
    const req = httpMock.expectOne('/api/projects/p1/boundaries');
    expect(req.request.method).toBe('POST');
    expect(req.request.body.geoJson).toBe('{"type":"Polygon"}');
    req.flush({ id: 'b2', projectId: 'p1', geoJson: '{"type":"Polygon"}' });
  });

  it('deleteBoundary should DELETE /api/projects/:id/boundaries/:bid', () => {
    service.deleteBoundary('p1', 'b1').subscribe();
    const req = httpMock.expectOne('/api/projects/p1/boundaries/b1');
    expect(req.request.method).toBe('DELETE');
    req.flush(null);
  });

  it('delete should DELETE /api/projects/:id', () => {
    service.delete('p1').subscribe();
    const req = httpMock.expectOne('/api/projects/p1');
    expect(req.request.method).toBe('DELETE');
    req.flush(null);
  });
});
