import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Project, CreateProjectRequest, ProjectBoundary } from '../models';

@Injectable({ providedIn: 'root' })
export class ProjectService {
  private http = inject(HttpClient);

  getAll(): Observable<Project[]> {
    return this.http.get<Project[]>('/api/projects');
  }

  getById(id: string): Observable<Project> {
    return this.http.get<Project>(`/api/projects/${id}`);
  }

  create(req: CreateProjectRequest): Observable<Project> {
    return this.http.post<Project>('/api/projects', req);
  }

  update(id: string, req: Partial<CreateProjectRequest>): Observable<Project> {
    return this.http.put<Project>(`/api/projects/${id}`, req);
  }

  getBoundaries(id: string): Observable<ProjectBoundary[]> {
    return this.http.get<ProjectBoundary[]>(`/api/projects/${id}/boundaries`);
  }

  createBoundary(id: string, geoJson: string): Observable<ProjectBoundary> {
    return this.http.post<ProjectBoundary>(`/api/projects/${id}/boundaries`, { geoJson });
  }

  deleteBoundary(projectId: string, boundaryId: string): Observable<void> {
    return this.http.delete<void>(`/api/projects/${projectId}/boundaries/${boundaryId}`);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`/api/projects/${id}`);
  }
}
