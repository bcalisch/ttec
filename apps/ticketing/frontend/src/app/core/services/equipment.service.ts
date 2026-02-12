import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Equipment, CreateEquipmentRequest, UpdateEquipmentRequest } from '../models';

@Injectable({ providedIn: 'root' })
export class EquipmentService {
  private http = inject(HttpClient);

  getAll(): Observable<Equipment[]> {
    return this.http.get<Equipment[]>('/api/equipment');
  }

  getById(id: string): Observable<Equipment> {
    return this.http.get<Equipment>(`/api/equipment/${id}`);
  }

  create(req: CreateEquipmentRequest): Observable<Equipment> {
    return this.http.post<Equipment>('/api/equipment', req);
  }

  update(id: string, req: UpdateEquipmentRequest): Observable<Equipment> {
    return this.http.put<Equipment>(`/api/equipment/${id}`, req);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`/api/equipment/${id}`);
  }
}
