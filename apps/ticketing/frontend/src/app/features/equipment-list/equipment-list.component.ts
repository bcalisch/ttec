import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { EquipmentService } from '../../core/services/equipment.service';
import { ToastService } from '../../core/services/toast.service';
import {
  Equipment, CreateEquipmentRequest, UpdateEquipmentRequest,
  EquipmentType, EquipmentManufacturer
} from '../../core/models';

@Component({
  selector: 'app-equipment-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="p-6">
      <div class="flex items-center justify-between mb-6">
        <h1 class="text-2xl font-bold text-gray-900">Equipment</h1>
        <button (click)="showForm = !showForm"
          class="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors">
          {{ showForm ? 'Cancel' : 'Add Equipment' }}
        </button>
      </div>

      @if (showForm) {
        <div class="mb-6 p-4 bg-white rounded-lg shadow border border-gray-200">
          <h2 class="text-lg font-semibold mb-4">{{ editingId ? 'Edit Equipment' : 'Add Equipment' }}</h2>
          <form (ngSubmit)="onSubmit()" class="space-y-4">
            <div class="grid grid-cols-2 gap-4">
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">Name *</label>
                <input [(ngModel)]="form.name" name="name" required
                  class="w-full px-3 py-2 border border-gray-300 rounded-md" placeholder="Equipment name" />
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">Serial Number *</label>
                <input [(ngModel)]="form.serialNumber" name="serialNumber" required
                  class="w-full px-3 py-2 border border-gray-300 rounded-md" placeholder="Serial number" />
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">Type</label>
                <select [(ngModel)]="form.type" name="type" class="w-full px-3 py-2 border border-gray-300 rounded-md">
                  <option *ngFor="let t of types" [value]="t">{{ t }}</option>
                </select>
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">Manufacturer</label>
                <select [(ngModel)]="form.manufacturer" name="manufacturer" class="w-full px-3 py-2 border border-gray-300 rounded-md">
                  <option *ngFor="let m of manufacturers" [value]="m">{{ m }}</option>
                </select>
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">Model</label>
                <input [(ngModel)]="form.model" name="model"
                  class="w-full px-3 py-2 border border-gray-300 rounded-md" placeholder="Model name" />
              </div>
            </div>
            @if (error) {
              <p class="text-red-600 text-sm">{{ error }}</p>
            }
            <button type="submit" [disabled]="!form.name || !form.serialNumber"
              class="px-4 py-2 bg-green-600 text-white rounded-md hover:bg-green-700 disabled:opacity-50">
              {{ editingId ? 'Save Changes' : 'Add Equipment' }}
            </button>
          </form>
        </div>
      }

      @if (loading) {
        <div class="flex justify-center py-12">
          <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
        </div>
      } @else {
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          @for (item of equipmentList; track item.id) {
            <div class="p-4 bg-white rounded-lg shadow border border-gray-200">
              <div class="flex items-start justify-between mb-2">
                <h3 class="text-lg font-semibold text-gray-900">{{ item.name }}</h3>
                <span class="text-xs px-2 py-1 rounded-full font-medium bg-gray-100 text-gray-700">{{ item.type }}</span>
              </div>
              <p class="text-sm text-gray-600 mb-1">SN: {{ item.serialNumber }}</p>
              <p class="text-sm text-gray-500 mb-1">{{ item.manufacturer }}{{ item.model ? ' — ' + item.model : '' }}</p>
              <p class="text-xs text-gray-400 mb-3">Added {{ item.createdAt | date:'mediumDate' }}</p>
              <div class="flex gap-2">
                <button (click)="editEquipment(item)"
                  class="text-xs text-blue-600 hover:underline">Edit</button>
                <button (click)="deleteEquipment(item)"
                  class="text-xs text-red-600 hover:underline">Delete</button>
              </div>
            </div>
          } @empty {
            <p class="text-gray-500 col-span-full text-center py-8">No equipment yet.</p>
          }
        </div>
      }
    </div>
  `
})
export class EquipmentListComponent implements OnInit {
  equipmentList: Equipment[] = [];
  loading = true;
  showForm = false;
  editingId = '';
  error = '';
  form: CreateEquipmentRequest = {
    name: '', serialNumber: '',
    type: EquipmentType.Roller,
    manufacturer: EquipmentManufacturer.BOMAG
  };

  types = Object.values(EquipmentType);
  manufacturers = Object.values(EquipmentManufacturer);

  constructor(
    private equipmentService: EquipmentService,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.loadEquipment();
  }

  loadEquipment(): void {
    this.loading = true;
    this.equipmentService.getAll().subscribe({
      next: (list) => { this.equipmentList = list; this.loading = false; },
      error: () => { this.loading = false; }
    });
  }

  onSubmit(): void {
    this.error = '';
    if (this.editingId) {
      const req: UpdateEquipmentRequest = { ...this.form };
      this.equipmentService.update(this.editingId, req).subscribe({
        next: () => {
          this.toastService.success('Equipment updated');
          this.resetForm();
          this.loadEquipment();
        },
        error: (err) => { this.error = err.error?.message || 'Failed to update'; }
      });
    } else {
      this.equipmentService.create(this.form).subscribe({
        next: () => {
          this.toastService.success('Equipment added');
          this.resetForm();
          this.loadEquipment();
        },
        error: (err) => { this.error = err.error?.message || 'Failed to create'; }
      });
    }
  }

  editEquipment(item: Equipment): void {
    this.editingId = item.id;
    this.form = {
      name: item.name, serialNumber: item.serialNumber,
      type: item.type, manufacturer: item.manufacturer, model: item.model
    };
    this.showForm = true;
  }

  deleteEquipment(item: Equipment): void {
    if (!confirm(`Delete "${item.name}"?`)) return;
    this.equipmentService.delete(item.id).subscribe({
      next: () => { this.toastService.success('Equipment deleted'); this.loadEquipment(); },
      error: () => this.toastService.error('Failed to delete')
    });
  }

  private resetForm(): void {
    this.showForm = false;
    this.editingId = '';
    this.form = { name: '', serialNumber: '', type: EquipmentType.Roller, manufacturer: EquipmentManufacturer.BOMAG };
  }
}
