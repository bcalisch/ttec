import { Component, OnInit, AfterViewInit, ElementRef, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TicketService } from '../../core/services/ticket.service';
import { BillingService } from '../../core/services/billing.service';
import {
  Ticket, TicketStatus, TicketPriority, SlaSummary, BillingSummary
} from '../../core/models';
import { Chart, registerables } from 'chart.js';

Chart.register(...registerables);

@Component({
  selector: 'app-sla-dashboard',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="p-6">
      <h1 class="text-2xl font-bold text-gray-900 mb-6">Dashboard</h1>

      <!-- Summary cards -->
      <div class="grid grid-cols-2 md:grid-cols-4 gap-4 mb-8">
        <div class="bg-white p-4 rounded-lg shadow border border-gray-200">
          <div class="text-sm text-gray-500">Total Tickets</div>
          <div class="text-2xl font-bold text-gray-900">{{ tickets.length }}</div>
        </div>
        <div class="bg-white p-4 rounded-lg shadow border border-gray-200">
          <div class="text-sm text-gray-500">Open</div>
          <div class="text-2xl font-bold text-blue-600">{{ openCount }}</div>
        </div>
        <div class="bg-white p-4 rounded-lg shadow border border-gray-200">
          <div class="text-sm text-gray-500">Overdue</div>
          <div class="text-2xl font-bold" [class.text-red-600]="overdueCount > 0" [class.text-green-600]="overdueCount === 0">
            {{ overdueCount }}
          </div>
        </div>
        <div class="bg-white p-4 rounded-lg shadow border border-gray-200">
          <div class="text-sm text-gray-500">Total Billing</div>
          <div class="text-2xl font-bold text-gray-900">{{ '$' }}{{ billingSummary?.grandTotal?.toFixed(2) || '0.00' }}</div>
        </div>
      </div>

      <!-- SLA Summary -->
      @if (slaSummary) {
        <div class="grid grid-cols-4 gap-4 mb-8">
          <div class="bg-white p-4 rounded-lg shadow border border-gray-200">
            <div class="text-sm text-gray-500">Total with SLA</div>
            <div class="text-xl font-bold">{{ slaSummary.total }}</div>
          </div>
          <div class="bg-white p-4 rounded-lg shadow border border-green-200 border-l-4 border-l-green-500">
            <div class="text-sm text-gray-500">Within SLA</div>
            <div class="text-xl font-bold text-green-600">{{ slaSummary.withinSla }}</div>
          </div>
          <div class="bg-white p-4 rounded-lg shadow border border-red-200 border-l-4 border-l-red-500">
            <div class="text-sm text-gray-500">Overdue</div>
            <div class="text-xl font-bold text-red-600">{{ slaSummary.overdue }}</div>
          </div>
          <div class="bg-white p-4 rounded-lg shadow border border-gray-200">
            <div class="text-sm text-gray-500">No SLA</div>
            <div class="text-xl font-bold text-gray-500">{{ slaSummary.noSla }}</div>
          </div>
        </div>
      }

      <!-- Charts -->
      <div class="grid grid-cols-1 md:grid-cols-2 gap-6 mb-8">
        <div class="bg-white p-4 rounded-lg shadow border border-gray-200">
          <h3 class="text-sm font-semibold text-gray-500 mb-3">Tickets by Status</h3>
          <canvas #statusChart height="200"></canvas>
        </div>
        <div class="bg-white p-4 rounded-lg shadow border border-gray-200">
          <h3 class="text-sm font-semibold text-gray-500 mb-3">Tickets by Priority</h3>
          <canvas #priorityChart height="200"></canvas>
        </div>
      </div>

      <!-- Billing breakdown -->
      @if (billingSummary) {
        <div class="bg-white p-4 rounded-lg shadow border border-gray-200">
          <h3 class="text-sm font-semibold text-gray-500 mb-3">Billing Summary</h3>
          <div class="grid grid-cols-3 gap-4 text-center">
            <div>
              <div class="text-sm text-gray-500">Base Charges</div>
              <div class="text-xl font-bold">{{ '$' }}{{ billingSummary.totalBaseCharges.toFixed(2) }}</div>
            </div>
            <div>
              <div class="text-sm text-gray-500">Hourly Charges</div>
              <div class="text-xl font-bold">{{ '$' }}{{ billingSummary.totalHourlyCharges.toFixed(2) }}</div>
            </div>
            <div>
              <div class="text-sm text-gray-500">Grand Total</div>
              <div class="text-xl font-bold text-green-600">{{ '$' }}{{ billingSummary.grandTotal.toFixed(2) }}</div>
            </div>
          </div>
        </div>
      }
    </div>
  `
})
export class SlaDashboardComponent implements OnInit, AfterViewInit {
  @ViewChild('statusChart') statusChartRef!: ElementRef<HTMLCanvasElement>;
  @ViewChild('priorityChart') priorityChartRef!: ElementRef<HTMLCanvasElement>;

  tickets: Ticket[] = [];
  slaSummary: SlaSummary | null = null;
  billingSummary: BillingSummary | null = null;
  openCount = 0;
  overdueCount = 0;
  private chartsReady = false;

  constructor(
    private ticketService: TicketService,
    private billingService: BillingService
  ) {}

  ngOnInit(): void {
    this.ticketService.getAll().subscribe({
      next: (t) => {
        this.tickets = t;
        this.openCount = t.filter(x => x.status === TicketStatus.Open || x.status === TicketStatus.InProgress).length;
        this.overdueCount = t.filter(x => x.isOverdue).length;
        if (this.chartsReady) this.renderCharts();
      }
    });
    this.ticketService.getSlaSummary().subscribe({
      next: (s) => this.slaSummary = s
    });
    this.billingService.getSummary().subscribe({
      next: (b) => this.billingSummary = b
    });
  }

  ngAfterViewInit(): void {
    this.chartsReady = true;
    if (this.tickets.length > 0) this.renderCharts();
  }

  private renderCharts(): void {
    this.renderStatusChart();
    this.renderPriorityChart();
  }

  private renderStatusChart(): void {
    const counts = new Map<string, number>();
    for (const s of Object.values(TicketStatus)) counts.set(s, 0);
    for (const t of this.tickets) counts.set(t.status, (counts.get(t.status) ?? 0) + 1);

    new Chart(this.statusChartRef.nativeElement, {
      type: 'doughnut',
      data: {
        labels: [...counts.keys()],
        datasets: [{
          data: [...counts.values()],
          backgroundColor: ['#3B82F6', '#F59E0B', '#8B5CF6', '#F97316', '#10B981', '#6B7280']
        }]
      },
      options: {
        responsive: true,
        plugins: { legend: { position: 'bottom' } }
      }
    });
  }

  private renderPriorityChart(): void {
    const counts = new Map<string, number>();
    for (const p of Object.values(TicketPriority)) counts.set(p, 0);
    for (const t of this.tickets) counts.set(t.priority, (counts.get(t.priority) ?? 0) + 1);

    new Chart(this.priorityChartRef.nativeElement, {
      type: 'bar',
      data: {
        labels: [...counts.keys()],
        datasets: [{
          label: 'Tickets',
          data: [...counts.values()],
          backgroundColor: ['#9CA3AF', '#3B82F6', '#F97316', '#EF4444']
        }]
      },
      options: {
        responsive: true,
        plugins: { legend: { display: false } },
        scales: { y: { beginAtZero: true, ticks: { stepSize: 1 } } }
      }
    });
  }
}
