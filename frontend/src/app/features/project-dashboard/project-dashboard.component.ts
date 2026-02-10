import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { ProjectService } from '../../core/services/project.service';
import { FeatureService, FeatureQueryParams } from '../../core/services/feature.service';
import {
  Project, ProjectBoundary, TestResultFeature,
  ObservationFeature, SensorFeature
} from '../../core/models';
import { MapComponent } from '../map/map.component';
import { FilterPanelComponent } from '../filter-panel/filter-panel.component';
import { DataTableComponent } from '../data-table/data-table.component';
import { FeatureDetailComponent } from '../feature-detail/feature-detail.component';
import { AnalyticsComponent } from '../analytics/analytics.component';
import { CsvUploadComponent } from '../csv-upload/csv-upload.component';

@Component({
  selector: 'app-project-dashboard',
  standalone: true,
  imports: [
    CommonModule, MapComponent, FilterPanelComponent,
    DataTableComponent, FeatureDetailComponent,
    AnalyticsComponent, CsvUploadComponent
  ],
  template: `
    <div class="h-full flex flex-col">
      @if (project) {
        <div class="flex items-center justify-between p-4 border-b border-gray-200 bg-white">
          <div>
            <h1 class="text-xl font-bold text-gray-900">{{ project.name }}</h1>
            <p class="text-sm text-gray-500">{{ project.client }}</p>
          </div>
          <div class="flex gap-2">
            <button
              (click)="showCsvUpload = true"
              class="px-3 py-1.5 text-sm bg-blue-600 text-white rounded-md hover:bg-blue-700">
              Upload CSV
            </button>
          </div>
        </div>

        <app-filter-panel (filtersChanged)="onFiltersChanged($event)" />

        <div class="relative flex-1" style="min-height: 50vh;">
          <app-map
            [features]="testResults"
            [observations]="observations"
            [sensors]="sensors"
            [boundary]="boundaryGeoJson"
            [projectId]="project.id"
            [coverageCells]="[]"
            (boundsChanged)="onBoundsChanged($event)"
            (featureSelected)="onFeatureSelected($event)" />
          @if (loading) {
            <div class="absolute top-4 right-4 bg-white rounded-full shadow px-3 py-1.5 flex items-center gap-2 z-[1000]">
              <div class="animate-spin rounded-full h-4 w-4 border-b-2 border-blue-600"></div>
              <span class="text-sm text-gray-600">Loading...</span>
            </div>
          }
        </div>

        <div class="border-t border-gray-200">
          <div class="flex border-b border-gray-200 bg-white">
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
          <div class="bg-white" style="max-height: 40vh; overflow-y: auto;">
            @if (activeTab === 'table') {
              <app-data-table
                [features]="testResults"
                (rowSelected)="onFeatureSelected($event)" />
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
      } @else if (loading) {
        <div class="flex justify-center items-center h-64">
          <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
        </div>
      }
    </div>
  `
})
export class ProjectDashboardComponent implements OnInit, OnDestroy {
  project: Project | null = null;
  testResults: TestResultFeature[] = [];
  observations: ObservationFeature[] = [];
  sensors: SensorFeature[] = [];
  boundaryGeoJson = '';
  loading = false;
  activeTab: 'table' | 'analytics' = 'table';
  selectedFeature: TestResultFeature | null = null;
  showCsvUpload = false;

  private currentBbox = '';
  private currentFilters: Record<string, string> = {};
  private destroy$ = new Subject<void>();
  private fetchTimeout: ReturnType<typeof setTimeout> | null = null;

  constructor(
    private route: ActivatedRoute,
    private projectService: ProjectService,
    private featureService: FeatureService
  ) {}

  ngOnInit(): void {
    this.route.paramMap.pipe(takeUntil(this.destroy$)).subscribe(params => {
      const id = params.get('id');
      if (id) this.loadProject(id);
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    if (this.fetchTimeout) clearTimeout(this.fetchTimeout);
  }

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
    this.debouncedFetch();
  }

  onFeatureSelected(feature: TestResultFeature): void {
    this.selectedFeature = feature;
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
      status: this.currentFilters['status'] || undefined
    };
    this.featureService.getFeatures(this.project.id, params).subscribe({
      next: (response) => {
        this.testResults = response.tests ?? [];
        this.observations = response.observations ?? [];
        this.sensors = response.sensors ?? [];
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
