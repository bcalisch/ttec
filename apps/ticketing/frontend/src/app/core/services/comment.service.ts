import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { TicketComment, CreateCommentRequest } from '../models';

@Injectable({ providedIn: 'root' })
export class CommentService {
  private http = inject(HttpClient);

  getAll(ticketId: string): Observable<TicketComment[]> {
    return this.http.get<TicketComment[]>(`/api/tickets/${ticketId}/comments`);
  }

  create(ticketId: string, req: CreateCommentRequest): Observable<TicketComment> {
    return this.http.post<TicketComment>(`/api/tickets/${ticketId}/comments`, req);
  }

  delete(ticketId: string, commentId: string): Observable<void> {
    return this.http.delete<void>(`/api/tickets/${ticketId}/comments/${commentId}`);
  }
}
