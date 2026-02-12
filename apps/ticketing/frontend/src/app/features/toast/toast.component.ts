import { Component, OnInit, OnDestroy, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs';
import { ToastService, Toast } from '../../core/services/toast.service';

@Component({
  selector: 'app-toast',
  standalone: true,
  imports: [CommonModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (toast) {
      <div class="fixed bottom-4 right-4 z-50 animate-slide-in">
        <div [class]="containerClass" class="px-4 py-3 rounded-lg shadow-lg flex items-center gap-2 text-sm">
          <span>{{ toast.message }}</span>
          <button (click)="dismiss()" class="ml-2 opacity-70 hover:opacity-100">&times;</button>
        </div>
      </div>
    }
  `,
  styles: [`
    @keyframes slideIn {
      from { transform: translateX(100%); opacity: 0; }
      to { transform: translateX(0); opacity: 1; }
    }
    .animate-slide-in { animation: slideIn 0.3s ease-out; }
  `]
})
export class ToastComponent implements OnInit, OnDestroy {
  toast: Toast | null = null;
  private sub?: Subscription;
  private timer?: ReturnType<typeof setTimeout>;

  constructor(private toastService: ToastService, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.sub = this.toastService.toast$.subscribe(t => {
      this.toast = t;
      this.cdr.markForCheck();
      if (this.timer) clearTimeout(this.timer);
      this.timer = setTimeout(() => this.dismiss(), 5000);
    });
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
    if (this.timer) clearTimeout(this.timer);
  }

  dismiss(): void {
    this.toast = null;
    this.cdr.markForCheck();
  }

  get containerClass(): string {
    switch (this.toast?.type) {
      case 'success': return 'bg-green-600 text-white';
      case 'error': return 'bg-red-600 text-white';
      case 'warning': return 'bg-yellow-500 text-white';
      default: return 'bg-blue-600 text-white';
    }
  }
}
