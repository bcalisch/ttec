import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { OutOfSpecItem, CoverageCell, TrendPoint } from '../models';

@Injectable({ providedIn: 'root' })
export class AnalyticsService {
  private http = inject(HttpClient);

  getOutOfSpec(projectId: string): Observable<OutOfSpecItem[]> {
    return this.http.get<OutOfSpecItem[]>(`/api/projects/${projectId}/analytics/out-of-spec`);
  }

  getCoverage(projectId: string): Observable<{gridSize: number, cells: CoverageCell[], gaps: number}> {
    return this.http.get<{gridSize: number, cells: CoverageCell[], gaps: number}>(`/api/projects/${projectId}/analytics/coverage`);
  }

  getTrends(projectId: string, interval: 'day' | 'week' = 'day'): Observable<TrendPoint[]> {
    const params = new HttpParams().set('interval', interval);
    return this.http.get<TrendPoint[]>(`/api/projects/${projectId}/analytics/trends`, { params });
  }
}
