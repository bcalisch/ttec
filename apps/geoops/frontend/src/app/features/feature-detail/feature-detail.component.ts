import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TestResultFeature } from '../../core/models';
import { AttachmentPanelComponent } from '../../shared/attachment-panel/attachment-panel.component';

@Component({
  selector: 'app-feature-detail',
  standalone: true,
  imports: [CommonModule, AttachmentPanelComponent],
  template: `
    <div
      class="fixed top-0 right-0 h-full w-80 bg-white shadow-xl z-[1500] transform transition-transform duration-300"
      [class.translate-x-0]="feature"
      [class.translate-x-full]="!feature">
      @if (feature) {
        <div class="flex items-center justify-between p-4 border-b border-gray-200">
          <h3 class="font-semibold text-gray-900">Test Result Detail</h3>
          <button (click)="closed.emit()" class="text-gray-400 hover:text-gray-600 text-xl">&times;</button>
        </div>
        <div class="p-4 space-y-4">
          <div>
            <label class="text-xs font-medium text-gray-500 uppercase tracking-wide">Test Type</label>
            <p class="text-gray-900 font-medium">{{ feature.testTypeName }}</p>
          </div>
          <div>
            <label class="text-xs font-medium text-gray-500 uppercase tracking-wide">Value</label>
            <p class="text-gray-900 text-lg font-bold">{{ feature.value }} <span class="text-sm font-normal text-gray-500">{{ feature.unit }}</span></p>
          </div>
          <div>
            <label class="text-xs font-medium text-gray-500 uppercase tracking-wide">Status</label>
            <p>
              <span [class]="statusBadgeClass(feature.status)" class="text-sm px-2.5 py-1 rounded-full font-medium">
                {{ feature.status }}
              </span>
            </p>
          </div>
          <div>
            <label class="text-xs font-medium text-gray-500 uppercase tracking-wide">Timestamp</label>
            <p class="text-gray-900">{{ feature.timestamp | date:'medium' }}</p>
          </div>
          <div>
            <label class="text-xs font-medium text-gray-500 uppercase tracking-wide">Coordinates</label>
            <p class="text-gray-900 text-sm font-mono">
              {{ feature.latitude | number:'1.6-6' }}, {{ feature.longitude | number:'1.6-6' }}
            </p>
          </div>
          <div class="border-t border-gray-200 pt-4">
            <app-attachment-panel entityType="TestResult" [entityId]="feature.id" />
          </div>
        </div>
      }
    </div>
  `,
  styles: [`
    :host {
      display: block;
    }
  `]
})
export class FeatureDetailComponent {
  @Input() feature: TestResultFeature | null = null;
  @Output() closed = new EventEmitter<void>();

  statusBadgeClass(status: string): string {
    const colors: Record<string, string> = {
      'Pass': 'bg-green-100 text-green-700',
      'Warn': 'bg-amber-100 text-amber-700',
      'Fail': 'bg-red-100 text-red-700'
    };
    return colors[status] ?? 'bg-gray-100 text-gray-700';
  }
}
