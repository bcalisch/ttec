import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ProjectService } from '../../core/services/project.service';
import { Project, CreateProjectRequest, ProjectStatus } from '../../core/models';

@Component({
  selector: 'app-project-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  template: `
    <div class="p-6">
      <div class="flex items-center justify-between mb-6">
        <h1 class="text-2xl font-bold text-gray-900">Projects</h1>
        <button
          (click)="showForm = !showForm"
          class="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors">
          {{ showForm ? 'Cancel' : 'New Project' }}
        </button>
      </div>

      @if (showForm) {
        <div class="mb-6 p-4 bg-white rounded-lg shadow border border-gray-200">
          <h2 class="text-lg font-semibold mb-4">Create New Project</h2>
          <form (ngSubmit)="onSubmit()" class="space-y-4">
            <div class="grid grid-cols-2 gap-4">
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">Name *</label>
                <input
                  [(ngModel)]="formData.name"
                  name="name"
                  required
                  class="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
                  placeholder="Project name" />
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">Client *</label>
                <input
                  [(ngModel)]="formData.client"
                  name="client"
                  required
                  class="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
                  placeholder="Client name" />
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">Status</label>
                <select
                  [(ngModel)]="formData.status"
                  name="status"
                  class="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500">
                  <option [ngValue]="0">Draft</option>
                  <option [ngValue]="1">Active</option>
                  <option [ngValue]="2">On Hold</option>
                  <option [ngValue]="3">Closed</option>
                </select>
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">Start Date</label>
                <input
                  [(ngModel)]="formData.startDate"
                  name="startDate"
                  type="date"
                  class="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500" />
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">End Date</label>
                <input
                  [(ngModel)]="formData.endDate"
                  name="endDate"
                  type="date"
                  class="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500" />
              </div>
            </div>
            <div class="flex gap-2">
              <button
                type="submit"
                [disabled]="!formData.name || !formData.client"
                class="px-4 py-2 bg-green-600 text-white rounded-md hover:bg-green-700 disabled:opacity-50 disabled:cursor-not-allowed">
                Create Project
              </button>
            </div>
            @if (error) {
              <p class="text-red-600 text-sm">{{ error }}</p>
            }
          </form>
        </div>
      }

      @if (loading) {
        <div class="flex justify-center py-12">
          <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
        </div>
      } @else {
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          @for (project of projects; track project.id) {
            <a
              [routerLink]="['/projects', project.id]"
              class="block p-4 bg-white rounded-lg shadow border border-gray-200 hover:shadow-md transition-shadow">
              <div class="flex items-start justify-between mb-2">
                <h3 class="text-lg font-semibold text-gray-900">{{ project.name }}</h3>
                <span [class]="statusBadgeClass(project.status)" class="text-xs px-2 py-1 rounded-full font-medium">
                  {{ statusLabel(project.status) }}
                </span>
              </div>
              <p class="text-sm text-gray-600 mb-2">{{ project.client }}</p>
              @if (project.startDate || project.endDate) {
                <p class="text-xs text-gray-400">
                  {{ project.startDate | date:'mediumDate' }}
                  @if (project.endDate) {
                    &mdash; {{ project.endDate | date:'mediumDate' }}
                  }
                </p>
              }
            </a>
          } @empty {
            <p class="text-gray-500 col-span-full text-center py-8">No projects yet. Create one to get started.</p>
          }
        </div>
      }
    </div>
  `
})
export class ProjectListComponent {
  projects: Project[] = [];
  loading = true;
  showForm = false;
  error = '';
  formData: CreateProjectRequest = {
    name: '',
    client: '',
    status: ProjectStatus.Draft,
    startDate: undefined,
    endDate: undefined
  };

  constructor(
    private projectService: ProjectService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadProjects();
  }

  loadProjects(): void {
    this.loading = true;
    this.projectService.getAll().subscribe({
      next: (projects) => {
        this.projects = projects;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      }
    });
  }

  onSubmit(): void {
    if (!this.formData.name || !this.formData.client) return;
    this.error = '';
    const req: CreateProjectRequest = {
      ...this.formData,
      startDate: this.formData.startDate || undefined,
      endDate: this.formData.endDate || undefined
    };
    this.projectService.create(req).subscribe({
      next: (project) => {
        this.router.navigate(['/projects', project.id]);
      },
      error: (err) => {
        this.error = err.error?.message || 'Failed to create project';
      }
    });
  }

  statusLabel(status: ProjectStatus): string {
    return ['Draft', 'Active', 'On Hold', 'Closed'][status] ?? 'Unknown';
  }

  statusBadgeClass(status: ProjectStatus): string {
    const colors: Record<number, string> = {
      0: 'bg-gray-100 text-gray-700',
      1: 'bg-green-100 text-green-700',
      2: 'bg-yellow-100 text-yellow-700',
      3: 'bg-red-100 text-red-700'
    };
    return colors[status] ?? 'bg-gray-100 text-gray-700';
  }
}
