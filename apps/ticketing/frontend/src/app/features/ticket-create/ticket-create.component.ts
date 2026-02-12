import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { TicketService } from '../../core/services/ticket.service';
import { EquipmentService } from '../../core/services/equipment.service';
import { ToastService } from '../../core/services/toast.service';
import {
  TicketPriority, TicketCategory, Equipment, CreateTicketRequest
} from '../../core/models';

@Component({
  selector: 'app-ticket-create',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="p-6 max-w-2xl mx-auto">
      <h1 class="text-2xl font-bold text-gray-900 mb-6">Create Ticket</h1>

      <form (ngSubmit)="onSubmit()" class="space-y-4 bg-white p-6 rounded-lg shadow border border-gray-200">
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">Title *</label>
          <input [(ngModel)]="form.title" name="title" required
            class="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
            placeholder="Brief summary of the issue" />
        </div>

        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">Description *</label>
          <textarea [(ngModel)]="form.description" name="description" required rows="4"
            class="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
            placeholder="Detailed description of the issue"></textarea>
        </div>

        <div class="grid grid-cols-2 gap-4">
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Priority</label>
            <select [(ngModel)]="form.priority" name="priority"
              class="w-full px-3 py-2 border border-gray-300 rounded-md">
              <option *ngFor="let p of priorities" [value]="p">{{ p }}</option>
            </select>
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Category</label>
            <select [(ngModel)]="form.category" name="category"
              class="w-full px-3 py-2 border border-gray-300 rounded-md">
              <option *ngFor="let c of categories" [value]="c">{{ c }}</option>
            </select>
          </div>
        </div>

        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">Equipment (optional)</label>
          <select [(ngModel)]="form.equipmentId" name="equipmentId"
            class="w-full px-3 py-2 border border-gray-300 rounded-md">
            <option [ngValue]="undefined">None</option>
            <option *ngFor="let e of equipmentList" [value]="e.id">{{ e.name }} ({{ e.serialNumber }})</option>
          </select>
        </div>

        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">Assigned To (optional)</label>
          <input [(ngModel)]="form.assignedTo" name="assignedTo"
            class="w-full px-3 py-2 border border-gray-300 rounded-md"
            placeholder="Technician name" />
        </div>

        <div class="grid grid-cols-2 gap-4">
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Longitude</label>
            <input [(ngModel)]="form.longitude" name="longitude" type="number" step="0.0001"
              class="w-full px-3 py-2 border border-gray-300 rounded-md" placeholder="-97.495" />
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Latitude</label>
            <input [(ngModel)]="form.latitude" name="latitude" type="number" step="0.0001"
              class="w-full px-3 py-2 border border-gray-300 rounded-md" placeholder="35.500" />
          </div>
        </div>

        @if (form.sourceApp) {
          <div class="p-3 bg-blue-50 rounded-md text-sm">
            <span class="font-medium">Linked from:</span> {{ form.sourceApp }} / {{ form.sourceEntityType }}
          </div>
        }

        @if (error) {
          <p class="text-red-600 text-sm">{{ error }}</p>
        }

        <div class="flex gap-3 pt-2">
          <button type="submit" [disabled]="!form.title || !form.description || saving"
            class="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 disabled:opacity-50">
            {{ saving ? 'Creating...' : 'Create Ticket' }}
          </button>
          <button type="button" (click)="cancel()"
            class="px-4 py-2 border border-gray-300 rounded-md hover:bg-gray-50">
            Cancel
          </button>
        </div>
      </form>
    </div>
  `
})
export class TicketCreateComponent implements OnInit {
  form: CreateTicketRequest = {
    title: '',
    description: '',
    priority: TicketPriority.Medium,
    category: TicketCategory.Software
  };
  saving = false;
  error = '';
  equipmentList: Equipment[] = [];
  priorities = Object.values(TicketPriority);
  categories = Object.values(TicketCategory);

  constructor(
    private ticketService: TicketService,
    private equipmentService: EquipmentService,
    private toastService: ToastService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.equipmentService.getAll().subscribe({
      next: (list) => this.equipmentList = list
    });

    // Pre-fill from query params (deep-link from GeoOps or other apps)
    const params = this.route.snapshot.queryParams;
    if (params['sourceApp']) this.form.sourceApp = params['sourceApp'];
    if (params['sourceType']) this.form.sourceEntityType = params['sourceType'];
    if (params['sourceId']) this.form.sourceEntityId = params['sourceId'];
  }

  onSubmit(): void {
    this.error = '';
    this.saving = true;
    this.ticketService.create(this.form).subscribe({
      next: (ticket) => {
        this.toastService.success('Ticket created');
        this.router.navigate(['/tickets', ticket.id]);
      },
      error: (err) => {
        this.error = err.error?.message || 'Failed to create ticket';
        this.saving = false;
      }
    });
  }

  cancel(): void {
    this.router.navigate(['/tickets']);
  }
}
