import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CreateSensorRequest, SensorFeature } from '../models';

@Injectable({ providedIn: 'root' })
export class SensorService {
  private http = inject(HttpClient);

  create(projectId: string, req: CreateSensorRequest): Observable<SensorFeature> {
    return this.http.post<SensorFeature>(`/api/projects/${projectId}/sensors`, req);
  }

  delete(projectId: string, id: string): Observable<void> {
    return this.http.delete<void>(`/api/projects/${projectId}/sensors/${id}`);
  }
}
