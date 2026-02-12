import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';

export interface Toast {
  id: number;
  message: string;
  type: 'success' | 'error' | 'info' | 'warning';
}

@Injectable({ providedIn: 'root' })
export class ToastService {
  private counter = 0;
  private toastSubject = new Subject<Toast>();

  toast$ = this.toastSubject.asObservable();

  success(message: string): void {
    this.emit({ message, type: 'success' });
  }

  error(message: string): void {
    this.emit({ message, type: 'error' });
  }

  info(message: string): void {
    this.emit({ message, type: 'info' });
  }

  warning(message: string): void {
    this.emit({ message, type: 'warning' });
  }

  private emit(partial: Omit<Toast, 'id'>): void {
    this.toastSubject.next({ ...partial, id: ++this.counter });
  }
}
