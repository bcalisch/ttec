import { Component, Input, Output, EventEmitter, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CreateObservationRequest } from '../../core/models';
import { ObservationService } from '../../core/services/observation.service';
import { ToastService } from '../../core/services/toast.service';

@Component({
  selector: 'app-add-observation-form',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="fixed inset-0 z-[2000] flex items-center justify-center bg-black/50" (click)="closed.emit()">
      <div class="bg-white rounded-lg shadow-xl w-full max-w-md" (click)="$event.stopPropagation()">
        <div class="flex items-center justify-between p-4 border-b border-gray-200">
          <h2 class="text-lg font-semibold text-gray-900">Add Observation</h2>
          <button (click)="closed.emit()" class="text-gray-400 hover:text-gray-600 text-xl">&times;</button>
        </div>

        <form (ngSubmit)="onSubmit()" class="p-4 space-y-4">
          <div class="grid grid-cols-2 gap-3 p-3 bg-gray-50 rounded-lg">
            <div>
              <label class="block text-xs font-medium text-gray-500 uppercase">Latitude</label>
              <p class="text-sm font-mono font-medium">{{ latitude | number:'1.6-6' }}</p>
            </div>
            <div>
              <label class="block text-xs font-medium text-gray-500 uppercase">Longitude</label>
              <p class="text-sm font-mono font-medium">{{ longitude | number:'1.6-6' }}</p>
            </div>
          </div>

          <div>
            <label for="note" class="block text-sm font-medium text-gray-700 mb-1">Note *</label>
            <textarea
              id="note"
              [(ngModel)]="note"
              name="note"
              required
              rows="3"
              class="w-full px-3 py-2 border border-gray-300 rounded-md text-sm focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
              placeholder="Describe the observation..."></textarea>
          </div>

          <div>
            <label for="tags" class="block text-sm font-medium text-gray-700 mb-1">Tags</label>
            <input
              id="tags"
              type="text"
              [(ngModel)]="tags"
              name="tags"
              class="w-full px-3 py-2 border border-gray-300 rounded-md text-sm focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
              placeholder="e.g. erosion, sediment, wildlife (comma-separated)" />
          </div>

          <div>
            <label for="timestamp" class="block text-sm font-medium text-gray-700 mb-1">Timestamp *</label>
            <input
              id="timestamp"
              type="datetime-local"
              [(ngModel)]="timestamp"
              name="timestamp"
              required
              class="w-full px-3 py-2 border border-gray-300 rounded-md text-sm focus:ring-2 focus:ring-blue-500 focus:border-blue-500" />
          </div>

          @if (errorMessage) {
            <div class="p-3 bg-red-50 border border-red-200 rounded-md text-sm text-red-700">
              {{ errorMessage }}
            </div>
          }

          <div class="flex justify-end gap-2 pt-2">
            <button
              type="button"
              (click)="closed.emit()"
              class="px-4 py-2 text-sm border border-gray-300 rounded-md hover:bg-gray-50">
              Cancel
            </button>
            <button
              type="submit"
              [disabled]="submitting || !isValid()"
              class="px-4 py-2 text-sm bg-blue-600 text-white rounded-md hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed">
              @if (submitting) {
                <span class="inline-flex items-center gap-1">
                  <span class="animate-spin h-3 w-3 border-2 border-white border-t-transparent rounded-full"></span>
                  Saving...
                </span>
              } @else {
                Add Observation
              }
            </button>
          </div>
        </form>
      </div>
    </div>
  `
})
export class AddObservationFormComponent implements OnInit {
  @Input() projectId = '';
  @Input() latitude = 0;
  @Input() longitude = 0;
  @Output() closed = new EventEmitter<void>();
  @Output() saved = new EventEmitter<void>();

  private observationService = inject(ObservationService);
  private toastService = inject(ToastService);

  note = '';
  tags = '';
  timestamp = '';
  submitting = false;
  errorMessage = '';

  ngOnInit(): void {
    const now = new Date();
    now.setMinutes(now.getMinutes() - now.getTimezoneOffset());
    this.timestamp = now.toISOString().slice(0, 16);
  }

  isValid(): boolean {
    return !!(this.note.trim() && this.timestamp);
  }

  onSubmit(): void {
    if (!this.isValid()) return;
    this.submitting = true;
    this.errorMessage = '';

    const req: CreateObservationRequest = {
      latitude: this.latitude,
      longitude: this.longitude,
      note: this.note.trim(),
      tags: this.tags.trim() || undefined,
      timestamp: new Date(this.timestamp).toISOString()
    };

    this.observationService.create(this.projectId, req).subscribe({
      next: () => {
        this.submitting = false;
        this.toastService.success('Observation added successfully');
        this.saved.emit();
      },
      error: (err) => {
        this.submitting = false;
        this.errorMessage = err.error?.message || err.error?.title || 'Failed to save observation.';
        this.toastService.error(this.errorMessage);
      }
    });
  }
}
