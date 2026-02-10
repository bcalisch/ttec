import {
  Component, Input, Output, EventEmitter, OnInit, OnChanges, SimpleChanges,
  ViewChild, ElementRef, AfterViewInit
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Chart, registerables } from 'chart.js';
import { AnalyticsService } from '../../core/services/analytics.service';
import { ExportService } from '../../core/services/export.service';
import { OutOfSpecItem, CoverageCell, TrendPoint } from '../../core/models';

Chart.register(...registerables);

@Component({
  selector: 'app-analytics',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="p-4 space-y-6">
      <div class="flex justify-end gap-2 mb-2">
        <button
          (click)="exportCsv()"
          class="px-3 py-1.5 text-sm border border-gray-300 rounded hover:bg-gray-50">
          Export CSV
        </button>
        <button
          (click)="exportGeoJson()"
          class="px-3 py-1.5 text-sm border border-gray-300 rounded hover:bg-gray-50">
          Export GeoJSON
        </button>
      </div>

      <!-- Out-of-spec table -->
      <div>
        <h3 class="text-sm font-semibold text-gray-700 mb-2 uppercase tracking-wide">Out-of-Spec Results</h3>
        @if (loadingOos) {
          <div class="flex justify-center py-4">
            <div class="animate-spin rounded-full h-5 w-5 border-b-2 border-blue-600"></div>
          </div>
        } @else {
          <div class="overflow-x-auto">
            <table class="w-full text-sm text-left">
              <thead class="bg-gray-100 text-xs uppercase">
                <tr>
                  <th class="px-3 py-1.5">Test Type</th>
                  <th class="px-3 py-1.5">Value</th>
                  <th class="px-3 py-1.5">Threshold</th>
                  <th class="px-3 py-1.5">Severity</th>
                  <th class="px-3 py-1.5">Timestamp</th>
                  <th class="px-3 py-1.5">Location</th>
                </tr>
              </thead>
              <tbody>
                @for (item of outOfSpecItems; track item.id; let odd = $odd) {
                  <tr
                    (click)="onLocate(item)"
                    [class]="severityRowClass(item.severity, odd)"
                    class="cursor-pointer hover:bg-blue-50">
                    <td class="px-3 py-1.5 font-medium">{{ item.testTypeName }}</td>
                    <td class="px-3 py-1.5">{{ item.value }}</td>
                    <td class="px-3 py-1.5">{{ item.threshold }}</td>
                    <td class="px-3 py-1.5">
                      <span [class]="severityBadgeClass(item.severity)" class="text-xs px-2 py-0.5 rounded-full">
                        {{ severityLabel(item.severity) }}
                      </span>
                    </td>
                    <td class="px-3 py-1.5">{{ item.timestamp | date:'short' }}</td>
                    <td class="px-3 py-1.5 text-xs font-mono">
                      {{ item.latitude | number:'1.4-4' }}, {{ item.longitude | number:'1.4-4' }}
                    </td>
                  </tr>
                } @empty {
                  <tr><td colspan="6" class="px-3 py-4 text-center text-gray-500">No out-of-spec results</td></tr>
                }
              </tbody>
            </table>
          </div>
        }
      </div>

      <!-- Trend chart -->
      <div>
        <div class="flex items-center justify-between mb-2">
          <h3 class="text-sm font-semibold text-gray-700 uppercase tracking-wide">Trends</h3>
          <select
            [(ngModel)]="trendInterval"
            (ngModelChange)="loadTrends()"
            class="px-2 py-1 text-sm border border-gray-300 rounded">
            <option value="day">Daily</option>
            <option value="week">Weekly</option>
          </select>
        </div>
        @if (loadingTrends) {
          <div class="flex justify-center py-4">
            <div class="animate-spin rounded-full h-5 w-5 border-b-2 border-blue-600"></div>
          </div>
        } @else {
          <div class="bg-white border border-gray-200 rounded p-2" style="height: 300px;">
            <canvas #trendCanvas></canvas>
          </div>
        }
      </div>
    </div>
  `
})
export class AnalyticsComponent implements OnInit, OnChanges, AfterViewInit {
  @Input() projectId = '';
  @Output() locateOnMap = new EventEmitter<{latitude: number, longitude: number}>();
  @ViewChild('trendCanvas') trendCanvas!: ElementRef<HTMLCanvasElement>;

  outOfSpecItems: OutOfSpecItem[] = [];
  trendPoints: TrendPoint[] = [];
  trendInterval: 'day' | 'week' = 'day';
  loadingOos = false;
  loadingTrends = false;
  private chart: Chart | null = null;

  constructor(
    private analyticsService: AnalyticsService,
    private exportService: ExportService
  ) {}

  ngOnInit(): void {
    if (this.projectId) {
      this.loadAll();
    }
  }

  ngAfterViewInit(): void {
    // Chart canvas may not be ready on first render if loading
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['projectId'] && this.projectId && !changes['projectId'].firstChange) {
      this.loadAll();
    }
  }

  private loadAll(): void {
    this.loadOutOfSpec();
    this.loadTrends();
  }

  loadOutOfSpec(): void {
    this.loadingOos = true;
    this.analyticsService.getOutOfSpec(this.projectId).subscribe({
      next: (items) => {
        this.outOfSpecItems = items;
        this.loadingOos = false;
      },
      error: () => { this.loadingOos = false; }
    });
  }

  loadTrends(): void {
    this.loadingTrends = true;
    this.analyticsService.getTrends(this.projectId, this.trendInterval).subscribe({
      next: (points) => {
        this.trendPoints = points;
        this.loadingTrends = false;
        setTimeout(() => this.renderChart(), 0);
      },
      error: () => { this.loadingTrends = false; }
    });
  }

  private renderChart(): void {
    if (!this.trendCanvas?.nativeElement) return;
    if (this.chart) this.chart.destroy();

    const grouped = new Map<string, TrendPoint[]>();
    for (const p of this.trendPoints) {
      const existing = grouped.get(p.testTypeName) ?? [];
      existing.push(p);
      grouped.set(p.testTypeName, existing);
    }

    const colors = ['#3b82f6', '#ef4444', '#22c55e', '#f59e0b', '#a855f7', '#06b6d4'];
    const datasets: any[] = [];
    let colorIdx = 0;

    for (const [name, points] of grouped) {
      const color = colors[colorIdx % colors.length];
      datasets.push({
        label: `${name} (avg)`,
        data: points.map(p => ({ x: p.period, y: p.avg })),
        borderColor: color,
        backgroundColor: color + '20',
        tension: 0.3,
        fill: false
      });
      datasets.push({
        label: `${name} (min)`,
        data: points.map(p => ({ x: p.period, y: p.min })),
        borderColor: color + '80',
        borderDash: [5, 5],
        tension: 0.3,
        fill: false,
        pointRadius: 0
      });
      datasets.push({
        label: `${name} (max)`,
        data: points.map(p => ({ x: p.period, y: p.max })),
        borderColor: color + '80',
        borderDash: [2, 2],
        tension: 0.3,
        fill: false,
        pointRadius: 0
      });
      colorIdx++;
    }

    this.chart = new Chart(this.trendCanvas.nativeElement, {
      type: 'line',
      data: { datasets },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        scales: {
          x: { type: 'category' },
          y: { beginAtZero: true }
        },
        plugins: {
          legend: { position: 'bottom', labels: { boxWidth: 12, font: { size: 10 } } }
        }
      }
    });
  }

  exportCsv(): void {
    this.exportService.exportCsv(this.projectId);
  }

  exportGeoJson(): void {
    this.exportService.exportGeoJson(this.projectId);
  }

  onLocate(item: OutOfSpecItem): void {
    this.locateOnMap.emit({latitude: item.latitude, longitude: item.longitude});
  }

  severityLabel(severity: number): string {
    if (severity >= 3) return 'High';
    if (severity >= 2) return 'Medium';
    return 'Low';
  }

  severityBadgeClass(severity: number): string {
    if (severity >= 3) return 'bg-red-100 text-red-700';
    if (severity >= 2) return 'bg-amber-100 text-amber-700';
    return 'bg-yellow-100 text-yellow-700';
  }

  severityRowClass(severity: number, odd: boolean): string {
    if (severity >= 3) return 'bg-red-50';
    return odd ? 'bg-gray-50' : '';
  }
}
