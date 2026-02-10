import { Component, OnInit, OnDestroy, ChangeDetectionStrategy, ChangeDetectorRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs';
import { Toast, ToastService } from '../../core/services/toast.service';

@Component({
  selector: 'app-toast',
  standalone: true,
  imports: [CommonModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="fixed bottom-4 right-4 z-[9999] flex flex-col gap-2 max-w-sm">
      @for (toast of toasts; track toast.id) {
        <div
          [class]="containerClass(toast.type)"
          class="flex items-start gap-2 px-4 py-3 rounded-lg shadow-lg text-sm animate-slide-in">
          <span class="shrink-0 mt-0.5">{{ icon(toast.type) }}</span>
          <span class="flex-1">{{ toast.message }}</span>
          <button (click)="dismiss(toast.id)" class="shrink-0 opacity-60 hover:opacity-100 text-lg leading-none">&times;</button>
        </div>
      }
    </div>
  `,
  styles: [`
    @keyframes slideIn {
      from { transform: translateX(100%); opacity: 0; }
      to   { transform: translateX(0);    opacity: 1; }
    }
    .animate-slide-in {
      animation: slideIn 0.25s ease-out;
    }
  `]
})
export class ToastComponent implements OnInit, OnDestroy {
  private toastService = inject(ToastService);
  private cdr = inject(ChangeDetectorRef);
  private sub: Subscription | null = null;
  private timers = new Map<number, ReturnType<typeof setTimeout>>();

  toasts: Toast[] = [];

  ngOnInit(): void {
    this.sub = this.toastService.toast$.subscribe(toast => {
      this.toasts.push(toast);
      this.cdr.markForCheck();
      const timer = setTimeout(() => this.dismiss(toast.id), 5000);
      this.timers.set(toast.id, timer);
    });
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
    this.timers.forEach(timer => clearTimeout(timer));
  }

  dismiss(id: number): void {
    this.toasts = this.toasts.filter(t => t.id !== id);
    const timer = this.timers.get(id);
    if (timer) {
      clearTimeout(timer);
      this.timers.delete(id);
    }
    this.cdr.markForCheck();
  }

  containerClass(type: Toast['type']): string {
    switch (type) {
      case 'success': return 'bg-green-600 text-white';
      case 'error':   return 'bg-red-600 text-white';
      case 'warning': return 'bg-amber-500 text-white';
      case 'info':    return 'bg-blue-600 text-white';
    }
  }

  icon(type: Toast['type']): string {
    switch (type) {
      case 'success': return '\u2713';
      case 'error':   return '\u2717';
      case 'warning': return '\u26A0';
      case 'info':    return '\u2139';
    }
  }
}
