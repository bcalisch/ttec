import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';

export interface ExportParams {
  from?: string;
  to?: string;
  testTypeId?: string;
  status?: string;
}

@Injectable({ providedIn: 'root' })
export class ExportService {
  private http = inject(HttpClient);

  exportCsv(projectId: string, params: ExportParams = {}): void {
    const httpParams = this.buildParams(params);
    this.http.get(`/api/projects/${projectId}/export/csv`, {
      params: httpParams,
      responseType: 'blob'
    }).subscribe(blob => {
      this.downloadFile(blob, `project-${projectId}-export.csv`, 'text/csv');
    });
  }

  exportGeoJson(projectId: string, params: ExportParams = {}): void {
    const httpParams = this.buildParams(params);
    this.http.get(`/api/projects/${projectId}/export/geojson`, {
      params: httpParams,
      responseType: 'blob'
    }).subscribe(blob => {
      this.downloadFile(blob, `project-${projectId}-export.geojson`, 'application/geo+json');
    });
  }

  private buildParams(params: ExportParams): HttpParams {
    let httpParams = new HttpParams();
    if (params.from) httpParams = httpParams.set('from', params.from);
    if (params.to) httpParams = httpParams.set('to', params.to);
    if (params.testTypeId) httpParams = httpParams.set('testTypeId', params.testTypeId);
    if (params.status) httpParams = httpParams.set('status', params.status);
    return httpParams;
  }

  private downloadFile(blob: Blob, filename: string, mimeType: string): void {
    const url = window.URL.createObjectURL(new Blob([blob], { type: mimeType }));
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    a.click();
    window.URL.revokeObjectURL(url);
  }
}
