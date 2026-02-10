import { Component, OnInit, OnDestroy, ElementRef, ViewChild, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { ProjectService } from '../../core/services/project.service';
import { FeatureService, FeatureQueryParams } from '../../core/services/feature.service';
import {
  Project, ProjectBoundary, TestResultFeature,
  ObservationFeature, SensorFeature, ProjectStatus
} from '../../core/models';
import { ToastService } from '../../core/services/toast.service';
import { MapComponent } from '../map/map.component';
import { FilterPanelComponent } from '../filter-panel/filter-panel.component';
import { DataTableComponent } from '../data-table/data-table.component';
import { FeatureDetailComponent } from '../feature-detail/feature-detail.component';
import { AnalyticsComponent } from '../analytics/analytics.component';
import { CsvUploadComponent } from '../csv-upload/csv-upload.component';
import { AddTestFormComponent } from '../add-test-form/add-test-form.component';
import { AddObservationFormComponent } from '../add-observation-form/add-observation-form.component';
import { AddSensorFormComponent } from '../add-sensor-form/add-sensor-form.component';

@Component({
  selector: 'app-project-dashboard',
  standalone: true,
  imports: [
    CommonModule, FormsModule, MapComponent, FilterPanelComponent,
    DataTableComponent, FeatureDetailComponent,
    AnalyticsComponent, CsvUploadComponent, AddTestFormComponent,
    AddObservationFormComponent, AddSensorFormComponent
  ],
  template: `
    <div class="h-full flex flex-col overflow-hidden">
      @if (project) {
        <div class="flex items-center justify-between p-4 border-b border-gray-200 bg-white shrink-0">
          <div>
            <h1 class="text-xl font-bold text-gray-900">{{ project.name }}</h1>
            <p class="text-sm text-gray-500">{{ project.client }}</p>
          </div>
          <div class="flex gap-2">
            <button
              (click)="openEditProject()"
              class="px-3 py-1.5 text-sm border border-gray-300 rounded-md hover:bg-gray-50">
              Edit Project
            </button>
            <button
              (click)="deleteProject()"
              class="px-3 py-1.5 text-sm border border-red-300 text-red-600 rounded-md hover:bg-red-50">
              Delete
            </button>
            <button
              (click)="showCsvUpload = true"
              class="px-3 py-1.5 text-sm bg-blue-600 text-white rounded-md hover:bg-blue-700">
              Upload CSV
            </button>
          </div>
        </div>

        <div class="shrink-0">
          <app-filter-panel (filtersChanged)="onFiltersChanged($event)" />
        </div>

        <!-- Map area -->
        <div #mapArea class="relative overflow-hidden" [style.height.px]="mapHeight">
          <app-map
            [features]="testResults"
            [observations]="observations"
            [sensors]="sensors"
            [boundary]="boundaryGeoJson"
            [boundaryId]="boundaryId"
            [projectId]="project.id"
            [coverageCells]="[]"
            (boundsChanged)="onBoundsChanged($event)"
            (featureSelected)="onFeatureSelected($event)"
            (addTestRequested)="onAddTestRequested($event)"
            (observationRequested)="onAddObservationRequested($event)"
            (sensorRequested)="onAddSensorRequested($event)"
            (boundaryChanged)="onBoundaryChanged($event)" />
          @if (loading) {
            <div class="absolute top-4 right-4 bg-white rounded-full shadow px-3 py-1.5 flex items-center gap-2 z-[1000]">
              <div class="animate-spin rounded-full h-4 w-4 border-b-2 border-blue-600"></div>
              <span class="text-sm text-gray-600">Loading...</span>
            </div>
          }
        </div>

        <!-- Drag handle -->
        <div
          class="shrink-0 h-1.5 bg-gray-200 hover:bg-blue-400 cursor-row-resize flex items-center justify-center transition-colors"
          (mousedown)="onDragStart($event)">
          <div class="w-8 h-0.5 bg-gray-400 rounded-full"></div>
        </div>

        <!-- Tabs area -->
        <div class="flex-1 flex flex-col overflow-hidden min-h-0">
          <div class="flex border-b border-gray-200 bg-white shrink-0">
            <button
              (click)="activeTab = 'table'"
              [class]="activeTab === 'table' ? 'border-b-2 border-blue-600 text-blue-600' : 'text-gray-500'"
              class="px-4 py-2 text-sm font-medium">
              Data Table
            </button>
            <button
              (click)="activeTab = 'analytics'"
              [class]="activeTab === 'analytics' ? 'border-b-2 border-blue-600 text-blue-600' : 'text-gray-500'"
              class="px-4 py-2 text-sm font-medium">
              Analytics
            </button>
          </div>
          <div class="flex-1 bg-white overflow-y-auto min-h-0">
            @if (activeTab === 'table') {
              <app-data-table
                [features]="testResults"
                [totalItems]="totalTests"
                [page]="currentPage"
                [pageSize]="pageSize"
                (rowSelected)="onFeatureSelected($event)"
                (pageChanged)="onPageChanged($event)" />
            } @else {
              <app-analytics [projectId]="project.id" />
            }
          </div>
        </div>

        @if (selectedFeature) {
          <app-feature-detail
            [feature]="selectedFeature"
            (closed)="selectedFeature = null" />
        }

        @if (showCsvUpload) {
          <app-csv-upload
            [projectId]="project.id"
            (closed)="showCsvUpload = false; fetchFeatures()"
            (uploaded)="showCsvUpload = false; fetchFeatures()" />
        }

        @if (showAddTest) {
          <app-add-test-form
            [projectId]="project.id"
            [latitude]="addTestLat"
            [longitude]="addTestLng"
            (closed)="showAddTest = false"
            (saved)="showAddTest = false; fetchFeatures()" />
        }

        @if (showAddObservation) {
          <app-add-observation-form
            [projectId]="project.id"
            [latitude]="addTestLat"
            [longitude]="addTestLng"
            (closed)="showAddObservation = false"
            (saved)="showAddObservation = false; fetchFeatures()" />
        }

        @if (showAddSensor) {
          <app-add-sensor-form
            [projectId]="project.id"
            [latitude]="addTestLat"
            [longitude]="addTestLng"
            (closed)="showAddSensor = false"
            (saved)="showAddSensor = false; fetchFeatures()" />
        }
        @if (showEditProject && project) {
          <div class="fixed inset-0 z-[2000] flex items-center justify-center bg-black/50" (click)="showEditProject = false">
            <div class="bg-white rounded-lg shadow-xl w-full max-w-md" (click)="$event.stopPropagation()">
              <div class="flex items-center justify-between p-4 border-b border-gray-200">
                <h2 class="text-lg font-semibold text-gray-900">Edit Project</h2>
                <button (click)="showEditProject = false" class="text-gray-400 hover:text-gray-600 text-xl">&times;</button>
              </div>
              <form (ngSubmit)="saveProject()" class="p-4 space-y-4">
                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-1">Name *</label>
                  <input [(ngModel)]="editName" name="editName" required
                    class="w-full px-3 py-2 border border-gray-300 rounded-md text-sm focus:ring-2 focus:ring-blue-500 focus:border-blue-500" />
                </div>
                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-1">Client *</label>
                  <input [(ngModel)]="editClient" name="editClient" required
                    class="w-full px-3 py-2 border border-gray-300 rounded-md text-sm focus:ring-2 focus:ring-blue-500 focus:border-blue-500" />
                </div>
                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-1">Status</label>
                  <select [(ngModel)]="editStatus" name="editStatus"
                    class="w-full px-3 py-2 border border-gray-300 rounded-md text-sm focus:ring-2 focus:ring-blue-500 focus:border-blue-500">
                    <option [ngValue]="0">Draft</option>
                    <option [ngValue]="1">Active</option>
                    <option [ngValue]="2">On Hold</option>
                    <option [ngValue]="3">Closed</option>
                  </select>
                </div>
                <div class="grid grid-cols-2 gap-3">
                  <div>
                    <label class="block text-sm font-medium text-gray-700 mb-1">Start Date</label>
                    <input [(ngModel)]="editStartDate" name="editStartDate" type="date"
                      class="w-full px-3 py-2 border border-gray-300 rounded-md text-sm focus:ring-2 focus:ring-blue-500 focus:border-blue-500" />
                  </div>
                  <div>
                    <label class="block text-sm font-medium text-gray-700 mb-1">End Date</label>
                    <input [(ngModel)]="editEndDate" name="editEndDate" type="date"
                      class="w-full px-3 py-2 border border-gray-300 rounded-md text-sm focus:ring-2 focus:ring-blue-500 focus:border-blue-500" />
                  </div>
                </div>
                <div class="flex justify-end gap-2 pt-2">
                  <button type="button" (click)="showEditProject = false"
                    class="px-4 py-2 text-sm border border-gray-300 rounded-md hover:bg-gray-50">Cancel</button>
                  <button type="submit" [disabled]="!editName || !editClient"
                    class="px-4 py-2 text-sm bg-blue-600 text-white rounded-md hover:bg-blue-700 disabled:opacity-50">Save</button>
                </div>
              </form>
            </div>
          </div>
        }
      } @else if (loading) {
        <div class="flex justify-center items-center h-64">
          <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
        </div>
      }
    </div>
  `
})
export class ProjectDashboardComponent implements OnInit, OnDestroy, AfterViewInit {
  @ViewChild('mapArea') mapArea!: ElementRef;

  project: Project | null = null;
  testResults: TestResultFeature[] = [];
  observations: ObservationFeature[] = [];
  sensors: SensorFeature[] = [];
  boundaryGeoJson = '';
  boundaryId = '';
  loading = false;
  activeTab: 'table' | 'analytics' = 'table';
  selectedFeature: TestResultFeature | null = null;
  showCsvUpload = false;
  showAddTest = false;
  showAddObservation = false;
  showAddSensor = false;
  showEditProject = false;
  editName = '';
  editClient = '';
  editStatus: ProjectStatus = ProjectStatus.Draft;
  editStartDate = '';
  editEndDate = '';
  addTestLat = 0;
  addTestLng = 0;
  mapHeight = 400;
  currentPage = 1;
  pageSize = 50;
  totalTests = 0;

  private currentBbox = '';
  private currentFilters: Record<string, string> = {};
  private destroy$ = new Subject<void>();
  private fetchTimeout: ReturnType<typeof setTimeout> | null = null;
  private dragging = false;
  private dragStartY = 0;
  private dragStartHeight = 0;
  private boundMouseMove: ((e: MouseEvent) => void) | null = null;
  private boundMouseUp: (() => void) | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private projectService: ProjectService,
    private featureService: FeatureService,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.route.paramMap.pipe(takeUntil(this.destroy$)).subscribe(params => {
      const id = params.get('id');
      if (id) this.loadProject(id);
    });
  }

  ngAfterViewInit(): void {
    // Set initial map height to 60% of available space
    setTimeout(() => {
      const container = this.mapArea?.nativeElement?.parentElement;
      if (container) {
        this.mapHeight = Math.max(200, container.clientHeight * 0.6);
      }
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    if (this.fetchTimeout) clearTimeout(this.fetchTimeout);
    this.cleanupDrag();
  }

  // --- Drag handle for resizing map/tabs ---

  onDragStart(event: MouseEvent): void {
    event.preventDefault();
    this.dragging = true;
    this.dragStartY = event.clientY;
    this.dragStartHeight = this.mapHeight;

    this.boundMouseMove = (e: MouseEvent) => this.onDragMove(e);
    this.boundMouseUp = () => this.onDragEnd();
    document.addEventListener('mousemove', this.boundMouseMove);
    document.addEventListener('mouseup', this.boundMouseUp);
  }

  private onDragMove(event: MouseEvent): void {
    if (!this.dragging) return;
    const delta = event.clientY - this.dragStartY;
    const newHeight = this.dragStartHeight + delta;
    // Clamp between 150px and container height minus 100px for tabs
    const container = this.mapArea?.nativeElement?.parentElement;
    const maxHeight = container ? container.clientHeight - 100 : 800;
    this.mapHeight = Math.max(150, Math.min(newHeight, maxHeight));
  }

  private onDragEnd(): void {
    this.dragging = false;
    this.cleanupDrag();
    // Trigger Leaflet map resize
    window.dispatchEvent(new Event('resize'));
  }

  private cleanupDrag(): void {
    if (this.boundMouseMove) {
      document.removeEventListener('mousemove', this.boundMouseMove);
      this.boundMouseMove = null;
    }
    if (this.boundMouseUp) {
      document.removeEventListener('mouseup', this.boundMouseUp);
      this.boundMouseUp = null;
    }
  }

  // --- Data loading ---

  loadProject(id: string): void {
    this.loading = true;
    this.projectService.getById(id).subscribe({
      next: (project) => {
        this.project = project;
        this.loading = false;
        this.loadBoundaries(id);
        this.fetchFeatures();
      },
      error: () => { this.loading = false; }
    });
  }

  loadBoundaries(id: string): void {
    this.projectService.getBoundaries(id).subscribe({
      next: (boundaries) => {
        if (boundaries.length > 0) {
          this.boundaryGeoJson = boundaries[0].geoJson;
          this.boundaryId = boundaries[0].id;
        }
      }
    });
  }

  onBoundsChanged(bbox: string): void {
    this.currentBbox = bbox;
    this.debouncedFetch();
  }

  onFiltersChanged(filters: Record<string, string>): void {
    this.currentFilters = filters;
    this.currentPage = 1;
    this.debouncedFetch();
  }

  onPageChanged(page: number): void {
    this.currentPage = page;
    this.fetchFeatures();
  }

  onFeatureSelected(feature: TestResultFeature): void {
    this.selectedFeature = feature;
  }

  onAddTestRequested(location: {latitude: number, longitude: number}): void {
    this.addTestLat = location.latitude;
    this.addTestLng = location.longitude;
    this.showAddTest = true;
  }

  onAddObservationRequested(location: {latitude: number, longitude: number}): void {
    this.addTestLat = location.latitude;
    this.addTestLng = location.longitude;
    this.showAddObservation = true;
  }

  onAddSensorRequested(location: {latitude: number, longitude: number}): void {
    this.addTestLat = location.latitude;
    this.addTestLng = location.longitude;
    this.showAddSensor = true;
  }

  openEditProject(): void {
    if (!this.project) return;
    this.editName = this.project.name;
    this.editClient = this.project.client;
    this.editStatus = this.project.status;
    this.editStartDate = this.project.startDate?.split('T')[0] ?? '';
    this.editEndDate = this.project.endDate?.split('T')[0] ?? '';
    this.showEditProject = true;
  }

  saveProject(): void {
    if (!this.project || !this.editName || !this.editClient) return;
    this.projectService.update(this.project.id, {
      name: this.editName,
      client: this.editClient,
      status: this.editStatus,
      startDate: this.editStartDate || undefined,
      endDate: this.editEndDate || undefined
    }).subscribe({
      next: (updated) => {
        this.project = updated;
        this.showEditProject = false;
        this.toastService.success('Project updated');
      },
      error: () => {
        this.toastService.error('Failed to update project');
      }
    });
  }

  deleteProject(): void {
    if (!this.project) return;
    if (!confirm(`Delete project "${this.project.name}"? This cannot be undone.`)) return;
    this.projectService.delete(this.project.id).subscribe({
      next: () => {
        this.toastService.success('Project deleted');
        this.router.navigate(['/']);
      },
      error: () => {
        this.toastService.error('Failed to delete project');
      }
    });
  }

  onBoundaryChanged(geoJson: string): void {
    this.boundaryGeoJson = geoJson;
    if (!geoJson) {
      this.boundaryId = '';
    } else if (this.project) {
      // Reload boundaries to get the new ID
      this.loadBoundaries(this.project.id);
    }
  }

  fetchFeatures(): void {
    if (!this.project) return;
    this.loading = true;
    const params: FeatureQueryParams = {
      bbox: this.currentBbox || undefined,
      from: this.currentFilters['from'] || undefined,
      to: this.currentFilters['to'] || undefined,
      types: this.currentFilters['types'] || undefined,
      testTypeId: this.currentFilters['testTypeId'] || undefined,
      status: this.currentFilters['status'] || undefined,
      page: this.currentPage,
      pageSize: this.pageSize
    };
    this.featureService.getFeatures(this.project.id, params).subscribe({
      next: (response) => {
        this.testResults = response.tests ?? [];
        this.observations = response.observations ?? [];
        this.sensors = response.sensors ?? [];
        this.totalTests = response.totalTests ?? 0;
        this.currentPage = response.page ?? 1;
        this.loading = false;
      },
      error: () => { this.loading = false; }
    });
  }

  private debouncedFetch(): void {
    if (this.fetchTimeout) clearTimeout(this.fetchTimeout);
    this.fetchTimeout = setTimeout(() => this.fetchFeatures(), 500);
  }
}
