import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import * as L from 'leaflet';
import { TicketService } from '../../core/services/ticket.service';
import { CommentService } from '../../core/services/comment.service';
import { TimeEntryService } from '../../core/services/time-entry.service';
import { BillingService } from '../../core/services/billing.service';
import { ToastService } from '../../core/services/toast.service';
import {
  Ticket, TicketComment, TimeEntry, TicketBilling,
  TicketStatus, TicketPriority, TicketCategory,
  UpdateTicketRequest, CreateCommentRequest, CreateTimeEntryRequest
} from '../../core/models';

@Component({
  selector: 'app-ticket-detail',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="p-6" *ngIf="ticket">
      <!-- Header -->
      <div class="flex items-start justify-between mb-6">
        <div>
          <button (click)="goBack()" class="text-sm text-blue-600 hover:underline mb-2">&larr; Back to tickets</button>
          <h1 class="text-2xl font-bold text-gray-900">{{ ticket.title }}</h1>
          <div class="flex items-center gap-2 mt-1">
            <span [class]="statusBadge(ticket.status)" class="text-xs px-2 py-1 rounded-full font-medium">{{ ticket.status }}</span>
            <span [class]="priorityBadge(ticket.priority)" class="text-xs px-2 py-1 rounded-full font-medium">{{ ticket.priority }}</span>
            @if (ticket.isOverdue) {
              <span class="text-xs px-2 py-1 rounded-full font-medium bg-red-100 text-red-700">OVERDUE</span>
            }
          </div>
        </div>
        <div class="flex gap-2">
          <button (click)="editing = !editing"
            class="px-3 py-1.5 text-sm border border-gray-300 rounded-md hover:bg-gray-50">
            {{ editing ? 'Cancel Edit' : 'Edit' }}
          </button>
          <button (click)="deleteTicket()"
            class="px-3 py-1.5 text-sm border border-red-300 text-red-600 rounded-md hover:bg-red-50">
            Delete
          </button>
        </div>
      </div>

      <div class="grid grid-cols-3 gap-6">
        <!-- Main content -->
        <div class="col-span-2 space-y-6">
          <!-- Edit form / Description -->
          @if (editing) {
            <div class="bg-white p-4 rounded-lg shadow border border-gray-200 space-y-3">
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">Title</label>
                <input [(ngModel)]="editForm.title" class="w-full px-3 py-2 border border-gray-300 rounded-md text-sm" />
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">Description</label>
                <textarea [(ngModel)]="editForm.description" rows="4" class="w-full px-3 py-2 border border-gray-300 rounded-md text-sm"></textarea>
              </div>
              <div class="grid grid-cols-3 gap-3">
                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-1">Status</label>
                  <select [(ngModel)]="editForm.status" class="w-full px-3 py-2 border border-gray-300 rounded-md text-sm">
                    <option *ngFor="let s of statuses" [value]="s">{{ s }}</option>
                  </select>
                </div>
                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-1">Priority</label>
                  <select [(ngModel)]="editForm.priority" class="w-full px-3 py-2 border border-gray-300 rounded-md text-sm">
                    <option *ngFor="let p of priorities" [value]="p">{{ p }}</option>
                  </select>
                </div>
                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-1">Category</label>
                  <select [(ngModel)]="editForm.category" class="w-full px-3 py-2 border border-gray-300 rounded-md text-sm">
                    <option *ngFor="let c of categories" [value]="c">{{ c }}</option>
                  </select>
                </div>
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">Assigned To</label>
                <input [(ngModel)]="editForm.assignedTo" class="w-full px-3 py-2 border border-gray-300 rounded-md text-sm" />
              </div>
              <button (click)="saveEdit()" class="px-4 py-2 bg-blue-600 text-white rounded-md text-sm hover:bg-blue-700">Save Changes</button>
            </div>
          } @else {
            <div class="bg-white p-4 rounded-lg shadow border border-gray-200">
              <h3 class="text-sm font-semibold text-gray-500 mb-2">Description</h3>
              <p class="text-gray-700 whitespace-pre-wrap">{{ ticket.description }}</p>
            </div>
          }

          @if (ticket.sourceApp) {
            <div class="bg-blue-50 p-3 rounded-lg text-sm flex items-center gap-2">
              <span class="font-medium">Linked from:</span>
              @if (getSourceLink()) {
                <a [href]="getSourceLink()" target="_blank"
                  class="text-blue-600 hover:underline">
                  {{ ticket.sourceApp }} / {{ ticket.sourceEntityType }}
                </a>
              } @else {
                <span>{{ ticket.sourceApp }} / {{ ticket.sourceEntityType }}</span>
              }
              <span class="text-gray-400">({{ ticket.sourceEntityId }})</span>
            </div>
          }

          <!-- Location map -->
          @if (ticket.latitude && ticket.longitude) {
            <div class="bg-white p-4 rounded-lg shadow border border-gray-200">
              <h3 class="text-sm font-semibold text-gray-500 mb-2">Location</h3>
              <div id="ticket-map" class="h-48 rounded-md bg-gray-100"></div>
              <p class="text-xs text-gray-400 mt-1">{{ ticket.latitude }}, {{ ticket.longitude }}</p>
            </div>
          }

          <!-- Comments -->
          <div class="bg-white p-4 rounded-lg shadow border border-gray-200">
            <h3 class="text-sm font-semibold text-gray-500 mb-3">Comments ({{ comments.length }})</h3>
            <div class="space-y-3 mb-4">
              @for (comment of comments; track comment.id) {
                <div class="border border-gray-100 rounded-md p-3" [class.bg-yellow-50]="comment.isInternal">
                  <div class="flex items-center justify-between mb-1">
                    <span class="text-sm font-medium text-gray-700">{{ comment.author }}</span>
                    <div class="flex items-center gap-2">
                      @if (comment.isInternal) {
                        <span class="text-xs text-yellow-600 font-medium">Internal</span>
                      }
                      <span class="text-xs text-gray-400">{{ comment.createdAt | date:'short' }}</span>
                      <button (click)="deleteComment(comment.id)" class="text-xs text-red-500 hover:underline">Delete</button>
                    </div>
                  </div>
                  <p class="text-sm text-gray-600">{{ comment.body }}</p>
                </div>
              } @empty {
                <p class="text-sm text-gray-400">No comments yet.</p>
              }
            </div>
            <div class="border-t border-gray-100 pt-3">
              <textarea [(ngModel)]="newComment" rows="2" placeholder="Add a comment..."
                class="w-full px-3 py-2 border border-gray-300 rounded-md text-sm mb-2"></textarea>
              <div class="flex items-center gap-3">
                <button (click)="addComment()" [disabled]="!newComment.trim()"
                  class="px-3 py-1.5 bg-blue-600 text-white rounded-md text-sm hover:bg-blue-700 disabled:opacity-50">
                  Add Comment
                </button>
                <label class="flex items-center gap-1 text-sm text-gray-600">
                  <input type="checkbox" [(ngModel)]="commentInternal" class="rounded border-gray-300" />
                  Internal note
                </label>
              </div>
            </div>
          </div>

          <!-- Time Entries -->
          <div class="bg-white p-4 rounded-lg shadow border border-gray-200">
            <h3 class="text-sm font-semibold text-gray-500 mb-3">Time Entries</h3>
            <div class="space-y-2 mb-4">
              @for (entry of timeEntries; track entry.id) {
                <div class="flex items-center justify-between border border-gray-100 rounded-md p-2 text-sm">
                  <div>
                    <span class="font-medium">{{ entry.hours }}h</span>
                    <span class="text-gray-500 ml-2">{{ entry.description }}</span>
                  </div>
                  <div class="text-gray-400 text-xs">
                    {{ entry.technician }} &middot; {{ entry.createdAt | date:'short' }}
                    &middot; {{ '$' }}{{ (entry.hours * entry.hourlyRate).toFixed(2) }}
                  </div>
                </div>
              } @empty {
                <p class="text-sm text-gray-400">No time entries.</p>
              }
            </div>
            <div class="border-t border-gray-100 pt-3 flex gap-2 items-end">
              <div>
                <label class="block text-xs text-gray-500 mb-1">Hours</label>
                <input [(ngModel)]="newTimeHours" type="number" min="0.25" max="24" step="0.25"
                  class="w-20 px-2 py-1.5 border border-gray-300 rounded-md text-sm" />
              </div>
              <div class="flex-1">
                <label class="block text-xs text-gray-500 mb-1">Description</label>
                <input [(ngModel)]="newTimeDesc" placeholder="What was done?"
                  class="w-full px-2 py-1.5 border border-gray-300 rounded-md text-sm" />
              </div>
              <button (click)="addTimeEntry()" [disabled]="!newTimeHours || !newTimeDesc"
                class="px-3 py-1.5 bg-green-600 text-white rounded-md text-sm hover:bg-green-700 disabled:opacity-50">
                Log Time
              </button>
            </div>
          </div>
        </div>

        <!-- Sidebar -->
        <div class="space-y-4">
          <div class="bg-white p-4 rounded-lg shadow border border-gray-200 text-sm space-y-2">
            <div class="flex justify-between"><span class="text-gray-500">Status</span><span class="font-medium">{{ ticket.status }}</span></div>
            <div class="flex justify-between"><span class="text-gray-500">Priority</span><span class="font-medium">{{ ticket.priority }}</span></div>
            <div class="flex justify-between"><span class="text-gray-500">Category</span><span class="font-medium">{{ ticket.category }}</span></div>
            <div class="flex justify-between"><span class="text-gray-500">Reported By</span><span class="font-medium">{{ ticket.reportedBy }}</span></div>
            <div class="flex justify-between"><span class="text-gray-500">Assigned To</span><span class="font-medium">{{ ticket.assignedTo || '—' }}</span></div>
            <div class="flex justify-between"><span class="text-gray-500">Created</span><span>{{ ticket.createdAt | date:'medium' }}</span></div>
            <div class="flex justify-between"><span class="text-gray-500">Updated</span><span>{{ ticket.updatedAt | date:'medium' }}</span></div>
            @if (ticket.slaDeadline) {
              <div class="flex justify-between"><span class="text-gray-500">SLA Deadline</span><span [class.text-red-600]="ticket.isOverdue">{{ ticket.slaDeadline | date:'medium' }}</span></div>
            }
            @if (ticket.resolvedAt) {
              <div class="flex justify-between"><span class="text-gray-500">Resolved</span><span>{{ ticket.resolvedAt | date:'medium' }}</span></div>
            }
          </div>

          <!-- Billing -->
          @if (billing) {
            <div class="bg-white p-4 rounded-lg shadow border border-gray-200 text-sm">
              <h3 class="font-semibold text-gray-500 mb-2">Billing</h3>
              <div class="space-y-1">
                <div class="flex justify-between"><span class="text-gray-500">Base charge</span><span>{{ '$' }}{{ billing.baseCharge.toFixed(2) }}</span></div>
                <div class="flex justify-between"><span class="text-gray-500">Hourly total</span><span>{{ '$' }}{{ billing.hourlyTotal.toFixed(2) }}</span></div>
                <div class="flex justify-between border-t border-gray-100 pt-1 font-semibold"><span>Total</span><span>{{ '$' }}{{ billing.totalCharge.toFixed(2) }}</span></div>
              </div>
            </div>
          }
        </div>
      </div>
    </div>

    @if (loading) {
      <div class="flex justify-center items-center h-64">
        <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
      </div>
    }
  `
})
export class TicketDetailComponent implements OnInit {
  ticket: Ticket | null = null;
  comments: TicketComment[] = [];
  timeEntries: TimeEntry[] = [];
  billing: TicketBilling | null = null;
  loading = true;
  editing = false;

  editForm: UpdateTicketRequest = {
    title: '', description: '', status: TicketStatus.Open,
    priority: TicketPriority.Medium, category: TicketCategory.Software
  };

  newComment = '';
  commentInternal = false;
  newTimeHours = 0;
  newTimeDesc = '';

  statuses = Object.values(TicketStatus);
  priorities = Object.values(TicketPriority);
  categories = Object.values(TicketCategory);

  private ticketId = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private ticketService: TicketService,
    private commentService: CommentService,
    private timeEntryService: TimeEntryService,
    private billingService: BillingService,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.ticketId = this.route.snapshot.paramMap.get('id') ?? '';
    this.loadTicket();
    this.loadComments();
    this.loadTimeEntries();
    this.loadBilling();
  }

  loadTicket(): void {
    this.loading = true;
    this.ticketService.getById(this.ticketId).subscribe({
      next: (t) => {
        this.ticket = t;
        this.editForm = {
          title: t.title, description: t.description, status: t.status,
          priority: t.priority, category: t.category, assignedTo: t.assignedTo,
          equipmentId: t.equipmentId, longitude: t.longitude, latitude: t.latitude
        };
        this.loading = false;
        this.initMap();
      },
      error: () => {
        this.loading = false;
        this.router.navigate(['/tickets']);
      }
    });
  }

  loadComments(): void {
    this.commentService.getAll(this.ticketId).subscribe({
      next: (c) => this.comments = c
    });
  }

  loadTimeEntries(): void {
    this.timeEntryService.getAll(this.ticketId).subscribe({
      next: (e) => this.timeEntries = e
    });
  }

  loadBilling(): void {
    this.billingService.getTicketBilling(this.ticketId).subscribe({
      next: (b) => this.billing = b
    });
  }

  saveEdit(): void {
    this.ticketService.update(this.ticketId, this.editForm).subscribe({
      next: (t) => {
        this.ticket = t;
        this.editing = false;
        this.toastService.success('Ticket updated');
        this.loadBilling();
      },
      error: () => this.toastService.error('Failed to update ticket')
    });
  }

  deleteTicket(): void {
    if (!confirm('Delete this ticket? This cannot be undone.')) return;
    this.ticketService.delete(this.ticketId).subscribe({
      next: () => {
        this.toastService.success('Ticket deleted');
        this.router.navigate(['/tickets']);
      },
      error: () => this.toastService.error('Failed to delete ticket')
    });
  }

  addComment(): void {
    const req: CreateCommentRequest = { body: this.newComment.trim(), isInternal: this.commentInternal };
    this.commentService.create(this.ticketId, req).subscribe({
      next: () => {
        this.newComment = '';
        this.commentInternal = false;
        this.loadComments();
      },
      error: () => this.toastService.error('Failed to add comment')
    });
  }

  deleteComment(commentId: string): void {
    this.commentService.delete(this.ticketId, commentId).subscribe({
      next: () => this.loadComments(),
      error: () => this.toastService.error('Failed to delete comment')
    });
  }

  addTimeEntry(): void {
    const req: CreateTimeEntryRequest = { hours: this.newTimeHours, description: this.newTimeDesc };
    this.timeEntryService.create(this.ticketId, req).subscribe({
      next: () => {
        this.newTimeHours = 0;
        this.newTimeDesc = '';
        this.loadTimeEntries();
        this.loadBilling();
      },
      error: () => this.toastService.error('Failed to log time')
    });
  }

  goBack(): void {
    this.router.navigate(['/tickets']);
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

  getSourceLink(): string | null {
    if (!this.ticket?.sourceApp || !this.ticket?.sourceEntityId) return null;
    const geoopsUrl = (window as any).__APP_CONFIG__?.geoopsAppUrl as string;
    if (!geoopsUrl) return null;

    if (this.ticket.sourceApp === 'geoops') {
      if (this.ticket.sourceEntityType === 'project') {
        return `${geoopsUrl}/projects/${this.ticket.sourceEntityId}`;
      }
    }
    return null;
  }

  private initMap(): void {
    if (!this.ticket?.latitude || !this.ticket?.longitude) return;
    setTimeout(() => {
      const container = document.getElementById('ticket-map');
      if (!container) return;

      // Fix default marker icon paths (broken by bundlers)
      const DefaultIcon = L.icon({
        iconUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon.png',
        iconRetinaUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon-2x.png',
        shadowUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-shadow.png',
        iconSize: [25, 41],
        iconAnchor: [12, 41],
        popupAnchor: [1, -34],
        shadowSize: [41, 41]
      });

      const map = L.map(container).setView([this.ticket!.latitude!, this.ticket!.longitude!], 14);
      L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; OpenStreetMap contributors'
      }).addTo(map);
      L.marker([this.ticket!.latitude!, this.ticket!.longitude!], { icon: DefaultIcon }).addTo(map);
    }, 100);
  }
}
