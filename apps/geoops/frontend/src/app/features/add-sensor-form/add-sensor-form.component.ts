import { Component, Input, Output, EventEmitter, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CreateSensorRequest } from '../../core/models';
import { SensorService } from '../../core/services/sensor.service';
import { ToastService } from '../../core/services/toast.service';

@Component({
  selector: 'app-add-sensor-form',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="fixed inset-0 z-[2000] flex items-center justify-center bg-black/50" (click)="closed.emit()">
      <div class="bg-white rounded-lg shadow-xl w-full max-w-md" (click)="$event.stopPropagation()">
        <div class="flex items-center justify-between p-4 border-b border-gray-200">
          <h2 class="text-lg font-semibold text-gray-900">Add Sensor</h2>
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
            <label for="sensorType" class="block text-sm font-medium text-gray-700 mb-1">Sensor Type *</label>
            <select
              id="sensorType"
              [(ngModel)]="sensorType"
              name="sensorType"
              required
              class="w-full px-3 py-2 border border-gray-300 rounded-md text-sm focus:ring-2 focus:ring-blue-500 focus:border-blue-500">
              <option value="">Select a sensor type...</option>
              @for (t of commonTypes; track t) {
                <option [value]="t">{{ t }}</option>
              }
              <option value="__custom">Other (custom)...</option>
            </select>
          </div>

          @if (sensorType === '__custom') {
            <div>
              <label for="customType" class="block text-sm font-medium text-gray-700 mb-1">Custom Type *</label>
              <input
                id="customType"
                type="text"
                [(ngModel)]="customType"
                name="customType"
                required
                class="w-full px-3 py-2 border border-gray-300 rounded-md text-sm focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                placeholder="Enter sensor type" />
            </div>
          }

          <div>
            <label for="metadataJson" class="block text-sm font-medium text-gray-700 mb-1">Metadata (JSON)</label>
            <textarea
              id="metadataJson"
              [(ngModel)]="metadataJson"
              name="metadataJson"
              rows="3"
              class="w-full px-3 py-2 border border-gray-300 rounded-md text-sm font-mono focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
              placeholder='e.g. {"manufacturer": "Acme", "model": "X100"}'></textarea>
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
                Add Sensor
              }
            </button>
          </div>
        </form>
      </div>
    </div>
  `
})
export class AddSensorFormComponent {
  @Input() projectId = '';
  @Input() latitude = 0;
  @Input() longitude = 0;
  @Output() closed = new EventEmitter<void>();
  @Output() saved = new EventEmitter<void>();

  private sensorService = inject(SensorService);
  private toastService = inject(ToastService);

  sensorType = '';
  customType = '';
  metadataJson = '';
  submitting = false;
  errorMessage = '';

  commonTypes = [
    'Temperature',
    'Humidity',
    'Pressure',
    'pH',
    'Turbidity',
    'Dissolved Oxygen',
    'Flow Rate',
    'Water Level',
    'Soil Moisture',
    'Vibration'
  ];

  get resolvedType(): string {
    return this.sensorType === '__custom' ? this.customType.trim() : this.sensorType;
  }

  isValid(): boolean {
    return !!this.resolvedType;
  }

  onSubmit(): void {
    if (!this.isValid()) return;
    this.submitting = true;
    this.errorMessage = '';

    // Validate metadata JSON if provided
    if (this.metadataJson.trim()) {
      try {
        JSON.parse(this.metadataJson);
      } catch {
        this.submitting = false;
        this.errorMessage = 'Invalid JSON in metadata field.';
        return;
      }
    }

    const req: CreateSensorRequest = {
      latitude: this.latitude,
      longitude: this.longitude,
      type: this.resolvedType,
      metadataJson: this.metadataJson.trim() || undefined
    };

    this.sensorService.create(this.projectId, req).subscribe({
      next: () => {
        this.submitting = false;
        this.toastService.success('Sensor added successfully');
        this.saved.emit();
      },
      error: (err) => {
        this.submitting = false;
        this.errorMessage = err.error?.message || err.error?.title || 'Failed to save sensor.';
        this.toastService.error(this.errorMessage);
      }
    });
  }
}
