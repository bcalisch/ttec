import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CreateObservationRequest, ObservationFeature } from '../models';

@Injectable({ providedIn: 'root' })
export class ObservationService {
  private http = inject(HttpClient);

  create(projectId: string, req: CreateObservationRequest): Observable<ObservationFeature> {
    return this.http.post<ObservationFeature>(`/api/projects/${projectId}/observations`, req);
  }

  delete(projectId: string, id: string): Observable<void> {
    return this.http.delete<void>(`/api/projects/${projectId}/observations/${id}`);
  }
}
