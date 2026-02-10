import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TestResultFeature } from '../../core/models';

type SortField = 'testTypeName' | 'value' | 'status' | 'timestamp' | 'latitude' | 'longitude';

@Component({
  selector: 'app-data-table',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="overflow-x-auto">
      <table class="w-full text-sm text-left">
        <thead class="text-xs text-gray-700 uppercase bg-gray-100 sticky top-0">
          <tr>
            <th (click)="sort('testTypeName')" class="px-4 py-2 cursor-pointer hover:bg-gray-200">
              Test Type {{ sortIcon('testTypeName') }}
            </th>
            <th (click)="sort('value')" class="px-4 py-2 cursor-pointer hover:bg-gray-200">
              Value {{ sortIcon('value') }}
            </th>
            <th (click)="sort('status')" class="px-4 py-2 cursor-pointer hover:bg-gray-200">
              Status {{ sortIcon('status') }}
            </th>
            <th (click)="sort('timestamp')" class="px-4 py-2 cursor-pointer hover:bg-gray-200">
              Timestamp {{ sortIcon('timestamp') }}
            </th>
            <th (click)="sort('latitude')" class="px-4 py-2 cursor-pointer hover:bg-gray-200">
              Lat {{ sortIcon('latitude') }}
            </th>
            <th (click)="sort('longitude')" class="px-4 py-2 cursor-pointer hover:bg-gray-200">
              Lon {{ sortIcon('longitude') }}
            </th>
          </tr>
        </thead>
        <tbody>
          @for (f of sortedFeatures; track f.id; let odd = $odd) {
            <tr
              (click)="rowSelected.emit(f)"
              [class]="odd ? 'bg-gray-50 hover:bg-blue-50 cursor-pointer' : 'bg-white hover:bg-blue-50 cursor-pointer'">
              <td class="px-4 py-2 font-medium">{{ f.testTypeName }}</td>
              <td class="px-4 py-2">{{ f.value }} {{ f.unit }}</td>
              <td class="px-4 py-2">
                <span [class]="statusBadgeClass(f.status)" class="text-xs px-2 py-0.5 rounded-full font-medium">
                  {{ f.status }}
                </span>
              </td>
              <td class="px-4 py-2">{{ f.timestamp | date:'short' }}</td>
              <td class="px-4 py-2">{{ f.latitude | number:'1.5-5' }}</td>
              <td class="px-4 py-2">{{ f.longitude | number:'1.5-5' }}</td>
            </tr>
          } @empty {
            <tr>
              <td colspan="6" class="px-4 py-8 text-center text-gray-500">No test results to display</td>
            </tr>
          }
        </tbody>
      </table>
    </div>
  `
})
export class DataTableComponent {
  @Input() features: TestResultFeature[] = [];
  @Output() rowSelected = new EventEmitter<TestResultFeature>();

  sortField: SortField = 'timestamp';
  sortAsc = false;

  get sortedFeatures(): TestResultFeature[] {
    return [...this.features].sort((a, b) => {
      const aVal = a[this.sortField];
      const bVal = b[this.sortField];
      let cmp = 0;
      if (typeof aVal === 'number' && typeof bVal === 'number') {
        cmp = aVal - bVal;
      } else {
        cmp = String(aVal).localeCompare(String(bVal));
      }
      return this.sortAsc ? cmp : -cmp;
    });
  }

  sort(field: SortField): void {
    if (this.sortField === field) {
      this.sortAsc = !this.sortAsc;
    } else {
      this.sortField = field;
      this.sortAsc = true;
    }
  }

  sortIcon(field: SortField): string {
    if (this.sortField !== field) return '';
    return this.sortAsc ? '\u25B2' : '\u25BC';
  }

  statusBadgeClass(status: string): string {
    const colors: Record<string, string> = {
      'Pass': 'bg-green-100 text-green-700',
      'Warn': 'bg-amber-100 text-amber-700',
      'Fail': 'bg-red-100 text-red-700'
    };
    return colors[status] ?? 'bg-gray-100 text-gray-700';
  }
}
