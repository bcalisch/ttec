import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface AttachmentResponse {
  id: string;
  entityType: string;
  entityId: string;
  contentType: string;
  uploadedBy: string;
  uploadedAt: string;
  url: string;
}

@Injectable({ providedIn: 'root' })
export class AttachmentService {
  private http = inject(HttpClient);

  upload(entityType: string, entityId: string, file: File): Observable<AttachmentResponse> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('entityType', entityType);
    formData.append('entityId', entityId);
    return this.http.post<AttachmentResponse>('/api/attachments', formData);
  }

  list(entityType: string, entityId: string): Observable<AttachmentResponse[]> {
    const params = new HttpParams()
      .set('entityType', entityType)
      .set('entityId', entityId);
    return this.http.get<AttachmentResponse[]>('/api/attachments', { params });
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`/api/attachments/${id}`);
  }
}
