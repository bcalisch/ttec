import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { KnowledgeArticle, CreateKnowledgeArticleRequest, UpdateKnowledgeArticleRequest } from '../models';

@Injectable({ providedIn: 'root' })
export class KnowledgeArticleService {
  private http = inject(HttpClient);

  getAll(tag?: string): Observable<KnowledgeArticle[]> {
    let params = new HttpParams();
    if (tag) params = params.set('tag', tag);
    return this.http.get<KnowledgeArticle[]>('/api/knowledge-articles', { params });
  }

  getById(id: string): Observable<KnowledgeArticle> {
    return this.http.get<KnowledgeArticle>(`/api/knowledge-articles/${id}`);
  }

  create(req: CreateKnowledgeArticleRequest): Observable<KnowledgeArticle> {
    return this.http.post<KnowledgeArticle>('/api/knowledge-articles', req);
  }

  update(id: string, req: UpdateKnowledgeArticleRequest): Observable<KnowledgeArticle> {
    return this.http.put<KnowledgeArticle>(`/api/knowledge-articles/${id}`, req);
  }
}
