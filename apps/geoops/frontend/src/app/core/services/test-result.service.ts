import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CreateTestResultRequest, BatchIngestRequest } from '../models';

@Injectable({ providedIn: 'root' })
export class TestResultService {
  private http = inject(HttpClient);

  create(projectId: string, req: CreateTestResultRequest): Observable<unknown> {
    return this.http.post(`/api/projects/${projectId}/test-results`, req);
  }

  batchIngest(projectId: string, req: BatchIngestRequest): Observable<unknown> {
    return this.http.post(`/api/projects/${projectId}/ingest/test-results`, req);
  }
}
