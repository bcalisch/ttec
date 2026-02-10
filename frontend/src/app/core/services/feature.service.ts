import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { FeaturesResponse } from '../models';

export interface FeatureQueryParams {
  bbox?: string;
  from?: string;
  to?: string;
  types?: string;
  testTypeId?: string;
  status?: string;
  page?: number;
  pageSize?: number;
}

@Injectable({ providedIn: 'root' })
export class FeatureService {
  private http = inject(HttpClient);

  getFeatures(projectId: string, params: FeatureQueryParams = {}): Observable<FeaturesResponse> {
    let httpParams = new HttpParams();
    if (params.bbox) httpParams = httpParams.set('bbox', params.bbox);
    if (params.from) httpParams = httpParams.set('from', params.from);
    if (params.to) httpParams = httpParams.set('to', params.to);
    if (params.types) httpParams = httpParams.set('types', params.types);
    if (params.testTypeId) httpParams = httpParams.set('testTypeId', params.testTypeId);
    if (params.status) httpParams = httpParams.set('status', params.status);
    if (params.page) httpParams = httpParams.set('page', params.page.toString());
    if (params.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());

    return this.http.get<FeaturesResponse>(`/api/projects/${projectId}/features`, { params: httpParams });
  }
}
