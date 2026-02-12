import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { TestType } from '../models';

@Injectable({ providedIn: 'root' })
export class TestTypeService {
  private http = inject(HttpClient);

  getAll(): Observable<TestType[]> {
    return this.http.get<TestType[]>('/api/test-types');
  }

  getById(id: string): Observable<TestType> {
    return this.http.get<TestType>(`/api/test-types/${id}`);
  }

  create(req: Partial<TestType>): Observable<TestType> {
    return this.http.post<TestType>('/api/test-types', req);
  }

  update(id: string, req: Partial<TestType>): Observable<TestType> {
    return this.http.put<TestType>(`/api/test-types/${id}`, req);
  }
}
