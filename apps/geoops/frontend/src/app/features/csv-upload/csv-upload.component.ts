import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import * as Papa from 'papaparse';
import { TestResultService } from '../../core/services/test-result.service';
import { CreateTestResultRequest, BatchIngestRequest } from '../../core/models';

@Component({
  selector: 'app-csv-upload',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="fixed inset-0 z-[2000] flex items-center justify-center bg-black/50" (click)="closed.emit()">
      <div class="bg-white rounded-lg shadow-xl w-full max-w-3xl max-h-[80vh] overflow-auto" (click)="$event.stopPropagation()">
        <div class="flex items-center justify-between p-4 border-b border-gray-200">
          <h2 class="text-lg font-semibold">Upload CSV Test Results</h2>
          <button (click)="closed.emit()" class="text-gray-400 hover:text-gray-600 text-xl">&times;</button>
        </div>

        <div class="p-4">
          @switch (step) {
            @case (1) {
              <div
                class="border-2 border-dashed border-gray-300 rounded-lg p-12 text-center hover:border-blue-400 transition-colors"
                (dragover)="$event.preventDefault()"
                (drop)="onDrop($event)">
                <p class="text-gray-500 mb-4">Drag and drop a CSV file here, or</p>
                <label class="px-4 py-2 bg-blue-600 text-white rounded-md cursor-pointer hover:bg-blue-700">
                  Browse Files
                  <input type="file" accept=".csv" (change)="onFileSelect($event)" class="hidden" />
                </label>
                @if (fileName) {
                  <p class="mt-4 text-sm text-gray-600">Selected: {{ fileName }}</p>
                }
              </div>
            }
            @case (2) {
              <div>
                <h3 class="font-medium mb-3">Map CSV columns to fields</h3>
                <p class="text-sm text-gray-500 mb-4">CSV columns: {{ csvHeaders.join(', ') }}</p>
                <div class="grid grid-cols-2 gap-3">
                  @for (field of requiredFields; track field.key) {
                    <div>
                      <label class="block text-sm font-medium text-gray-700 mb-1">
                        {{ field.label }}
                        @if (field.required) { <span class="text-red-500">*</span> }
                      </label>
                      <select
                        [(ngModel)]="columnMapping[field.key]"
                        class="w-full px-2 py-1.5 border border-gray-300 rounded text-sm">
                        <option value="">-- Skip --</option>
                        @for (h of csvHeaders; track h) {
                          <option [value]="h">{{ h }}</option>
                        }
                      </select>
                    </div>
                  }
                </div>
                <div class="flex justify-end gap-2 mt-4">
                  <button (click)="step = 1" class="px-3 py-1.5 text-sm border border-gray-300 rounded hover:bg-gray-50">Back</button>
                  <button
                    (click)="previewData()"
                    [disabled]="!isMappingValid()"
                    class="px-3 py-1.5 text-sm bg-blue-600 text-white rounded hover:bg-blue-700 disabled:opacity-50">
                    Preview
                  </button>
                </div>
              </div>
            }
            @case (3) {
              <div>
                <h3 class="font-medium mb-3">Preview (first 10 rows)</h3>
                <div class="overflow-x-auto mb-4">
                  <table class="w-full text-sm text-left">
                    <thead class="bg-gray-100">
                      <tr>
                        <th class="px-3 py-1.5">Test Type ID</th>
                        <th class="px-3 py-1.5">Timestamp</th>
                        <th class="px-3 py-1.5">Value</th>
                        <th class="px-3 py-1.5">Longitude</th>
                        <th class="px-3 py-1.5">Latitude</th>
                        <th class="px-3 py-1.5">Source</th>
                        <th class="px-3 py-1.5">Technician</th>
                      </tr>
                    </thead>
                    <tbody>
                      @for (row of previewRows; track $index; let odd = $odd) {
                        <tr [class]="odd ? 'bg-gray-50' : ''">
                          <td class="px-3 py-1.5">{{ row.testTypeId }}</td>
                          <td class="px-3 py-1.5">{{ row.timestamp }}</td>
                          <td class="px-3 py-1.5">{{ row.value }}</td>
                          <td class="px-3 py-1.5">{{ row.longitude }}</td>
                          <td class="px-3 py-1.5">{{ row.latitude }}</td>
                          <td class="px-3 py-1.5">{{ row.source }}</td>
                          <td class="px-3 py-1.5">{{ row.technician }}</td>
                        </tr>
                      }
                    </tbody>
                  </table>
                </div>
                <p class="text-sm text-gray-500 mb-4">Total rows: {{ mappedRows.length }}</p>
                <div class="flex justify-end gap-2">
                  <button (click)="step = 2" class="px-3 py-1.5 text-sm border border-gray-300 rounded hover:bg-gray-50">Back</button>
                  <button
                    (click)="submit()"
                    class="px-3 py-1.5 text-sm bg-green-600 text-white rounded hover:bg-green-700">
                    Upload {{ mappedRows.length }} rows
                  </button>
                </div>
              </div>
            }
            @case (4) {
              <div class="text-center py-8">
                @if (uploading) {
                  <div class="animate-spin rounded-full h-10 w-10 border-b-2 border-blue-600 mx-auto mb-4"></div>
                  <p class="text-gray-600">Uploading {{ mappedRows.length }} rows...</p>
                } @else if (uploadError) {
                  <div class="text-red-600 mb-4">
                    <p class="font-medium">Upload failed</p>
                    <p class="text-sm">{{ uploadError }}</p>
                  </div>
                  <button (click)="step = 3" class="px-3 py-1.5 text-sm border border-gray-300 rounded hover:bg-gray-50">Back</button>
                } @else {
                  <p class="text-green-600 font-medium text-lg mb-2">Upload successful!</p>
                  <p class="text-gray-500 text-sm mb-4">{{ mappedRows.length }} test results ingested.</p>
                  <button (click)="uploaded.emit()" class="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700">Done</button>
                }
              </div>
            }
          }
        </div>
      </div>
    </div>
  `
})
export class CsvUploadComponent {
  @Input() projectId = '';
  @Output() closed = new EventEmitter<void>();
  @Output() uploaded = new EventEmitter<void>();

  step = 1;
  fileName = '';
  fileContent = '';
  csvHeaders: string[] = [];
  csvData: Record<string, string>[] = [];
  columnMapping: Record<string, string> = {
    testTypeId: '',
    timestamp: '',
    value: '',
    longitude: '',
    latitude: '',
    source: '',
    technician: ''
  };
  mappedRows: CreateTestResultRequest[] = [];
  previewRows: CreateTestResultRequest[] = [];
  uploading = false;
  uploadError = '';

  requiredFields = [
    { key: 'testTypeId', label: 'Test Type ID', required: true },
    { key: 'timestamp', label: 'Timestamp', required: true },
    { key: 'value', label: 'Value', required: true },
    { key: 'longitude', label: 'Longitude', required: true },
    { key: 'latitude', label: 'Latitude', required: true },
    { key: 'source', label: 'Source', required: false },
    { key: 'technician', label: 'Technician', required: false }
  ];

  constructor(private testResultService: TestResultService) {}

  onDrop(event: DragEvent): void {
    event.preventDefault();
    const file = event.dataTransfer?.files[0];
    if (file) this.processFile(file);
  }

  onFileSelect(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (file) this.processFile(file);
  }

  private processFile(file: File): void {
    this.fileName = file.name;
    const reader = new FileReader();
    reader.onload = () => {
      this.fileContent = reader.result as string;
      const result = Papa.parse(this.fileContent, { header: true, skipEmptyLines: true });
      this.csvHeaders = result.meta.fields ?? [];
      this.csvData = result.data as Record<string, string>[];
      this.step = 2;
    };
    reader.readAsText(file);
  }

  isMappingValid(): boolean {
    return !!(
      this.columnMapping['testTypeId'] &&
      this.columnMapping['timestamp'] &&
      this.columnMapping['value'] &&
      this.columnMapping['longitude'] &&
      this.columnMapping['latitude']
    );
  }

  previewData(): void {
    this.mappedRows = this.csvData.map(row => ({
      testTypeId: row[this.columnMapping['testTypeId']] ?? '',
      timestamp: row[this.columnMapping['timestamp']] ?? '',
      value: parseFloat(row[this.columnMapping['value']] ?? '0'),
      longitude: parseFloat(row[this.columnMapping['longitude']] ?? '0'),
      latitude: parseFloat(row[this.columnMapping['latitude']] ?? '0'),
      source: this.columnMapping['source'] ? row[this.columnMapping['source']] : undefined,
      technician: this.columnMapping['technician'] ? row[this.columnMapping['technician']] : undefined
    }));
    this.previewRows = this.mappedRows.slice(0, 10);
    this.step = 3;
  }

  async submit(): Promise<void> {
    this.step = 4;
    this.uploading = true;
    this.uploadError = '';

    const hash = await this.hashContent(this.fileContent);
    const idempotencyKey = `${this.fileName}-${hash}`;

    const req: BatchIngestRequest = {
      idempotencyKey,
      items: this.mappedRows
    };

    this.testResultService.batchIngest(this.projectId, req).subscribe({
      next: () => { this.uploading = false; },
      error: (err) => {
        this.uploading = false;
        this.uploadError = err.error?.message || 'Upload failed';
      }
    });
  }

  private async hashContent(content: string): Promise<string> {
    const encoder = new TextEncoder();
    const data = encoder.encode(content);
    const hashBuffer = await crypto.subtle.digest('SHA-256', data);
    const hashArray = Array.from(new Uint8Array(hashBuffer));
    return hashArray.map(b => b.toString(16).padStart(2, '0')).join('');
  }
}
