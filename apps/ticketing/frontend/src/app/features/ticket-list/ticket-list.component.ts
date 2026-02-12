import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { TicketService } from '../../core/services/ticket.service';
import { Ticket, TicketStatus, TicketPriority } from '../../core/models';

@Component({
  selector: 'app-ticket-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  template: `
    <div class="p-6">
      <div class="flex items-center justify-between mb-6">
        <h1 class="text-2xl font-bold text-gray-900">Tickets</h1>
        <a routerLink="/tickets/new"
          class="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors">
          New Ticket
        </a>
      </div>

      <!-- Filters -->
      <div class="mb-4 flex gap-3 items-center flex-wrap">
        <select [(ngModel)]="filterStatus" (ngModelChange)="loadTickets()"
          class="px-3 py-1.5 text-sm border border-gray-300 rounded-md">
          <option value="">All Statuses</option>
          <option *ngFor="let s of statuses" [value]="s">{{ s }}</option>
        </select>
        <select [(ngModel)]="filterPriority" (ngModelChange)="loadTickets()"
          class="px-3 py-1.5 text-sm border border-gray-300 rounded-md">
          <option value="">All Priorities</option>
          <option *ngFor="let p of priorities" [value]="p">{{ p }}</option>
        </select>
        <label class="flex items-center gap-1 text-sm text-gray-600">
          <input type="checkbox" [(ngModel)]="showOverdueOnly" (ngModelChange)="applyFilters()"
            class="rounded border-gray-300" />
          Overdue only
        </label>
        <span class="text-sm text-gray-500 ml-auto">{{ filteredTickets.length }} tickets</span>
      </div>

      @if (loading) {
        <div class="flex justify-center py-12">
          <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
        </div>
      } @else {
        <div class="bg-white rounded-lg shadow border border-gray-200 overflow-hidden">
          <table class="w-full text-sm">
            <thead class="bg-gray-50 border-b border-gray-200">
              <tr>
                <th class="text-left px-4 py-3 font-medium text-gray-600">Title</th>
                <th class="text-left px-4 py-3 font-medium text-gray-600">Status</th>
                <th class="text-left px-4 py-3 font-medium text-gray-600">Priority</th>
                <th class="text-left px-4 py-3 font-medium text-gray-600">Category</th>
                <th class="text-left px-4 py-3 font-medium text-gray-600">Assigned To</th>
                <th class="text-left px-4 py-3 font-medium text-gray-600">Created</th>
                <th class="text-left px-4 py-3 font-medium text-gray-600">SLA</th>
              </tr>
            </thead>
            <tbody>
              @for (ticket of filteredTickets; track ticket.id) {
                <tr class="border-b border-gray-100 hover:bg-gray-50 cursor-pointer"
                    [routerLink]="['/tickets', ticket.id]">
                  <td class="px-4 py-3">
                    <div class="font-medium text-gray-900">{{ ticket.title }}</div>
                    @if (ticket.sourceApp) {
                      <div class="text-xs text-gray-400 mt-0.5">from {{ ticket.sourceApp }}</div>
                    }
                  </td>
                  <td class="px-4 py-3">
                    <span [class]="statusBadge(ticket.status)" class="text-xs px-2 py-1 rounded-full font-medium">
                      {{ ticket.status }}
                    </span>
                  </td>
                  <td class="px-4 py-3">
                    <span [class]="priorityBadge(ticket.priority)" class="text-xs px-2 py-1 rounded-full font-medium">
                      {{ ticket.priority }}
                    </span>
                  </td>
                  <td class="px-4 py-3 text-gray-600">{{ ticket.category }}</td>
                  <td class="px-4 py-3 text-gray-600">{{ ticket.assignedTo || '—' }}</td>
                  <td class="px-4 py-3 text-gray-500">{{ ticket.createdAt | date:'short' }}</td>
                  <td class="px-4 py-3">
                    @if (ticket.isOverdue) {
                      <span class="text-xs px-2 py-1 rounded-full font-medium bg-red-100 text-red-700">OVERDUE</span>
                    } @else if (ticket.slaDeadline) {
                      <span class="text-xs text-gray-500">{{ ticket.slaDeadline | date:'short' }}</span>
                    } @else {
                      <span class="text-xs text-gray-400">—</span>
                    }
                  </td>
                </tr>
              } @empty {
                <tr>
                  <td colspan="7" class="px-4 py-8 text-center text-gray-500">No tickets found.</td>
                </tr>
              }
            </tbody>
          </table>
        </div>
      }
    </div>
  `
})
export class TicketListComponent implements OnInit {
  tickets: Ticket[] = [];
  filteredTickets: Ticket[] = [];
  loading = true;
  filterStatus = '';
  filterPriority = '';
  showOverdueOnly = false;

  statuses = Object.values(TicketStatus);
  priorities = Object.values(TicketPriority);

  constructor(private ticketService: TicketService) {}

  ngOnInit(): void {
    this.loadTickets();
  }

  loadTickets(): void {
    this.loading = true;
    const filters: Record<string, string> = {};
    if (this.filterStatus) filters['status'] = this.filterStatus;
    if (this.filterPriority) filters['priority'] = this.filterPriority;
    this.ticketService.getAll(filters).subscribe({
      next: (tickets) => {
        this.tickets = tickets;
        this.applyFilters();
        this.loading = false;
      },
      error: () => { this.loading = false; }
    });
  }

  applyFilters(): void {
    this.filteredTickets = this.showOverdueOnly
      ? this.tickets.filter(t => t.isOverdue)
      : this.tickets;
  }

  statusBadge(status: TicketStatus): string {
    const map: Record<string, string> = {
      'Open': 'bg-blue-100 text-blue-700',
      'InProgress': 'bg-yellow-100 text-yellow-700',
      'AwaitingCustomer': 'bg-purple-100 text-purple-700',
      'AwaitingParts': 'bg-orange-100 text-orange-700',
      'Resolved': 'bg-green-100 text-green-700',
      'Closed': 'bg-gray-100 text-gray-700'
    };
    return map[status] ?? 'bg-gray-100 text-gray-700';
  }

  priorityBadge(priority: TicketPriority): string {
    const map: Record<string, string> = {
      'Low': 'bg-gray-100 text-gray-600',
      'Medium': 'bg-blue-100 text-blue-600',
      'High': 'bg-orange-100 text-orange-600',
      'Critical': 'bg-red-100 text-red-700'
    };
    return map[priority] ?? 'bg-gray-100 text-gray-600';
  }
}
