import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { TicketBilling, BillingSummary } from '../models';

@Injectable({ providedIn: 'root' })
export class BillingService {
  private http = inject(HttpClient);

  getTicketBilling(ticketId: string): Observable<TicketBilling> {
    return this.http.get<TicketBilling>(`/api/billing/tickets/${ticketId}`);
  }

  getSummary(): Observable<BillingSummary> {
    return this.http.get<BillingSummary>('/api/billing/summary');
  }
}
