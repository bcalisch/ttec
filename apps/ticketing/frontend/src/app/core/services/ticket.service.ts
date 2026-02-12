import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Ticket, CreateTicketRequest, UpdateTicketRequest, SlaSummary } from '../models';

@Injectable({ providedIn: 'root' })
export class TicketService {
  private http = inject(HttpClient);

  getAll(filters?: { sourceApp?: string; sourceEntityId?: string; status?: string; priority?: string }): Observable<Ticket[]> {
    let params = new HttpParams();
    if (filters?.sourceApp) params = params.set('sourceApp', filters.sourceApp);
    if (filters?.sourceEntityId) params = params.set('sourceEntityId', filters.sourceEntityId);
    if (filters?.status) params = params.set('status', filters.status);
    if (filters?.priority) params = params.set('priority', filters.priority);
    return this.http.get<Ticket[]>('/api/tickets', { params });
  }

  getById(id: string): Observable<Ticket> {
    return this.http.get<Ticket>(`/api/tickets/${id}`);
  }

  create(req: CreateTicketRequest): Observable<Ticket> {
    return this.http.post<Ticket>('/api/tickets', req);
  }

  update(id: string, req: UpdateTicketRequest): Observable<Ticket> {
    return this.http.put<Ticket>(`/api/tickets/${id}`, req);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`/api/tickets/${id}`);
  }

  getSlaSummary(): Observable<SlaSummary> {
    return this.http.get<SlaSummary>('/api/tickets/sla-summary');
  }
}
