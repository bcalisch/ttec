import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TestType, CreateTestResultRequest } from '../../core/models';
import { TestTypeService } from '../../core/services/test-type.service';
import { TestResultService } from '../../core/services/test-result.service';

@Component({
  selector: 'app-add-test-form',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="fixed inset-0 z-[2000] flex items-center justify-center bg-black/50" (click)="closed.emit()">
      <div class="bg-white rounded-lg shadow-xl w-full max-w-md" (click)="$event.stopPropagation()">
        <div class="flex items-center justify-between p-4 border-b border-gray-200">
          <h2 class="text-lg font-semibold text-gray-900">Add New Test Result</h2>
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
            <label for="testType" class="block text-sm font-medium text-gray-700 mb-1">Test Type *</label>
            <select
              id="testType"
              [(ngModel)]="selectedTestTypeId"
              name="testType"
              required
              class="w-full px-3 py-2 border border-gray-300 rounded-md text-sm focus:ring-2 focus:ring-blue-500 focus:border-blue-500">
              <option value="">Select a test type...</option>
              @for (tt of testTypes; track tt.id) {
                <option [value]="tt.id">{{ tt.name }} ({{ tt.unit }})</option>
              }
            </select>
            @if (selectedTestType) {
              <p class="text-xs text-gray-500 mt-1">
                Threshold: {{ selectedTestType.minThreshold ?? '—' }} – {{ selectedTestType.maxThreshold ?? '—' }} {{ selectedTestType.unit }}
              </p>
            }
          </div>

          <div>
            <label for="value" class="block text-sm font-medium text-gray-700 mb-1">Value *</label>
            <div class="flex items-center gap-2">
              <input
                id="value"
                type="number"
                [(ngModel)]="value"
                name="value"
                required
                step="any"
                class="flex-1 px-3 py-2 border border-gray-300 rounded-md text-sm focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                placeholder="Enter test value" />
              @if (selectedTestType) {
                <span class="text-sm text-gray-500 shrink-0">{{ selectedTestType.unit }}</span>
              }
            </div>
            @if (selectedTestType && value !== null) {
              <p class="text-xs mt-1" [class]="statusPreviewClass">
                Predicted status: {{ statusPreview }}
              </p>
            }
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

          <div>
            <label for="technician" class="block text-sm font-medium text-gray-700 mb-1">Technician</label>
            <input
              id="technician"
              type="text"
              [(ngModel)]="technician"
              name="technician"
              class="w-full px-3 py-2 border border-gray-300 rounded-md text-sm focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
              placeholder="Name of technician" />
          </div>

          <div>
            <label for="source" class="block text-sm font-medium text-gray-700 mb-1">Source</label>
            <input
              id="source"
              type="text"
              [(ngModel)]="source"
              name="source"
              class="w-full px-3 py-2 border border-gray-300 rounded-md text-sm focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
              placeholder="e.g. Field Test, Lab Test" />
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
                Add Test Result
              }
            </button>
          </div>
        </form>
      </div>
    </div>
  `
})
export class AddTestFormComponent implements OnInit {
  @Input() projectId = '';
  @Input() latitude = 0;
  @Input() longitude = 0;
  @Output() closed = new EventEmitter<void>();
  @Output() saved = new EventEmitter<void>();

  testTypes: TestType[] = [];
  selectedTestTypeId = '';
  value: number | null = null;
  timestamp = '';
  technician = '';
  source = '';
  submitting = false;
  errorMessage = '';

  constructor(
    private testTypeService: TestTypeService,
    private testResultService: TestResultService
  ) {}

  ngOnInit(): void {
    this.testTypeService.getAll().subscribe({
      next: (types) => { this.testTypes = types; }
    });
    // Default timestamp to now in local datetime format
    const now = new Date();
    now.setMinutes(now.getMinutes() - now.getTimezoneOffset());
    this.timestamp = now.toISOString().slice(0, 16);
  }

  get selectedTestType(): TestType | null {
    return this.testTypes.find(t => t.id === this.selectedTestTypeId) ?? null;
  }

  get statusPreview(): string {
    const tt = this.selectedTestType;
    if (!tt || this.value === null) return '';
    if (tt.minThreshold != null && this.value < tt.minThreshold) return 'Fail';
    if (tt.maxThreshold != null && this.value > tt.maxThreshold) return 'Fail';
    return 'Pass';
  }

  get statusPreviewClass(): string {
    switch (this.statusPreview) {
      case 'Fail': return 'text-red-600 font-medium';
      case 'Pass': return 'text-green-600 font-medium';
      default: return 'text-gray-500';
    }
  }

  isValid(): boolean {
    return !!(this.selectedTestTypeId && this.value !== null && this.timestamp);
  }

  onSubmit(): void {
    if (!this.isValid()) return;
    this.submitting = true;
    this.errorMessage = '';

    const req: CreateTestResultRequest = {
      testTypeId: this.selectedTestTypeId,
      value: this.value!,
      timestamp: new Date(this.timestamp).toISOString(),
      longitude: this.longitude,
      latitude: this.latitude,
      source: this.source || undefined,
      technician: this.technician || undefined
    };

    this.testResultService.create(this.projectId, req).subscribe({
      next: () => {
        this.submitting = false;
        this.saved.emit();
      },
      error: (err) => {
        this.submitting = false;
        this.errorMessage = err.error?.message || err.error?.title || 'Failed to save test result. Please try again.';
      }
    });
  }
}
