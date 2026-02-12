import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { TimeEntry, CreateTimeEntryRequest } from '../models';

@Injectable({ providedIn: 'root' })
export class TimeEntryService {
  private http = inject(HttpClient);

  getAll(ticketId: string): Observable<TimeEntry[]> {
    return this.http.get<TimeEntry[]>(`/api/tickets/${ticketId}/time-entries`);
  }

  create(ticketId: string, req: CreateTimeEntryRequest): Observable<TimeEntry> {
    return this.http.post<TimeEntry>(`/api/tickets/${ticketId}/time-entries`, req);
  }
}
