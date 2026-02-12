import { Component, Output, EventEmitter, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TestTypeService } from '../../core/services/test-type.service';
import { TestType } from '../../core/models';

@Component({
  selector: 'app-filter-panel',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="flex flex-wrap items-center gap-3 px-4 py-2 bg-white border-b border-gray-200 text-sm">
      <div class="flex items-center gap-1">
        <label class="text-gray-600 font-medium">From</label>
        <input
          type="date"
          [(ngModel)]="from"
          (ngModelChange)="emitFilters()"
          class="px-2 py-1 border border-gray-300 rounded text-sm" />
      </div>
      <div class="flex items-center gap-1">
        <label class="text-gray-600 font-medium">To</label>
        <input
          type="date"
          [(ngModel)]="to"
          (ngModelChange)="emitFilters()"
          class="px-2 py-1 border border-gray-300 rounded text-sm" />
      </div>
      <div class="flex items-center gap-1">
        <label class="text-gray-600 font-medium">Test Type</label>
        <select
          [(ngModel)]="testTypeId"
          (ngModelChange)="emitFilters()"
          class="px-2 py-1 border border-gray-300 rounded text-sm">
          <option value="">All</option>
          @for (tt of testTypes; track tt.id) {
            <option [value]="tt.id">{{ tt.name }}</option>
          }
        </select>
      </div>
      <div class="flex items-center gap-2">
        <label class="text-gray-600 font-medium">Status</label>
        <label class="flex items-center gap-1 cursor-pointer">
          <input type="checkbox" [(ngModel)]="showPass" (ngModelChange)="emitFilters()" class="rounded" />
          <span class="inline-block w-2.5 h-2.5 rounded-full bg-green-500"></span>
          Pass
        </label>
        <label class="flex items-center gap-1 cursor-pointer">
          <input type="checkbox" [(ngModel)]="showWarn" (ngModelChange)="emitFilters()" class="rounded" />
          <span class="inline-block w-2.5 h-2.5 rounded-full bg-amber-500"></span>
          Warn
        </label>
        <label class="flex items-center gap-1 cursor-pointer">
          <input type="checkbox" [(ngModel)]="showFail" (ngModelChange)="emitFilters()" class="rounded" />
          <span class="inline-block w-2.5 h-2.5 rounded-full bg-red-500"></span>
          Fail
        </label>
      </div>
      <div class="flex items-center gap-2 border-l border-gray-300 pl-3">
        <label class="text-gray-600 font-medium">Layers</label>
        <label class="flex items-center gap-1 cursor-pointer">
          <input type="checkbox" [(ngModel)]="showTests" (ngModelChange)="emitFilters()" class="rounded" />
          Tests
        </label>
        <label class="flex items-center gap-1 cursor-pointer">
          <input type="checkbox" [(ngModel)]="showObservations" (ngModelChange)="emitFilters()" class="rounded" />
          Observations
        </label>
        <label class="flex items-center gap-1 cursor-pointer">
          <input type="checkbox" [(ngModel)]="showSensors" (ngModelChange)="emitFilters()" class="rounded" />
          Sensors
        </label>
      </div>
    </div>
  `
})
export class FilterPanelComponent implements OnInit {
  @Output() filtersChanged = new EventEmitter<Record<string, string>>();

  testTypes: TestType[] = [];
  from = '';
  to = '';
  testTypeId = '';
  showPass = true;
  showWarn = true;
  showFail = true;
  showTests = true;
  showObservations = true;
  showSensors = true;

  constructor(private testTypeService: TestTypeService) {}

  ngOnInit(): void {
    this.testTypeService.getAll().subscribe({
      next: (types) => { this.testTypes = types; }
    });
  }

  emitFilters(): void {
    const statuses: string[] = [];
    if (this.showPass) statuses.push('Pass');
    if (this.showWarn) statuses.push('Warn');
    if (this.showFail) statuses.push('Fail');

    const types: string[] = [];
    if (this.showTests) types.push('tests');
    if (this.showObservations) types.push('observations');
    if (this.showSensors) types.push('sensors');

    const filters: Record<string, string> = {};
    if (this.from) filters['from'] = this.from;
    if (this.to) filters['to'] = this.to;
    if (this.testTypeId) filters['testTypeId'] = this.testTypeId;
    if (statuses.length < 3) filters['status'] = statuses.join(',');
    if (types.length < 3) filters['types'] = types.join(',');

    this.filtersChanged.emit(filters);
  }
}
