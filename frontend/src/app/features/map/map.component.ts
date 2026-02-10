import {
  Component, Input, Output, EventEmitter,
  OnInit, OnDestroy, OnChanges, SimpleChanges,
  ChangeDetectionStrategy, ChangeDetectorRef, ElementRef, ViewChild, AfterViewInit
} from '@angular/core';
import { CommonModule } from '@angular/common';
import * as L from 'leaflet';
import 'leaflet.markercluster';
import 'leaflet-draw';
import {
  TestResultFeature, ObservationFeature, SensorFeature, CoverageCell
} from '../../core/models';
import { ProjectService } from '../../core/services/project.service';

// Fix Leaflet default icon path issue
delete (L.Icon.Default.prototype as any)._getIconUrl;
L.Icon.Default.mergeOptions({
  iconRetinaUrl: 'assets/marker-icon-2x.png',
  iconUrl: 'assets/marker-icon.png',
  shadowUrl: 'assets/marker-shadow.png',
});

@Component({
  selector: 'app-map',
  standalone: true,
  imports: [CommonModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="relative w-full h-full" style="min-height: 400px;">
      <div #mapContainer class="w-full h-full"></div>
      <div class="absolute top-2 right-2 z-[1000] flex flex-col gap-1">
        @if (!drawingMode) {
          <button
            (click)="startDrawBoundary()"
            class="px-3 py-1.5 text-xs bg-white border border-gray-300 rounded shadow hover:bg-gray-50">
            Draw Boundary
          </button>
          <label
            class="px-3 py-1.5 text-xs bg-white border border-gray-300 rounded shadow hover:bg-gray-50 cursor-pointer text-center">
            Import GeoJSON
            <input type="file" accept=".geojson,.json" (change)="onGeoJsonImport($event)" class="hidden" />
          </label>
        } @else {
          <button
            (click)="cancelDraw()"
            class="px-3 py-1.5 text-xs bg-red-500 text-white rounded shadow hover:bg-red-600">
            Cancel Draw
          </button>
        }
      </div>
      @if (contextMenuVisible) {
        <div
          class="absolute z-[1100] bg-white rounded-lg shadow-lg border border-gray-200 py-1 min-w-[160px]"
          [style.left.px]="contextMenuX"
          [style.top.px]="contextMenuY">
          <button
            (click)="onContextMenuAddTest()"
            class="w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-blue-50 hover:text-blue-700 flex items-center gap-2">
            <span class="text-base">+</span> Add New Test
          </button>
        </div>
      }
    </div>
  `,
  styles: [`
    :host {
      display: block;
      width: 100%;
      height: 100%;
    }
  `]
})
export class MapComponent implements OnInit, AfterViewInit, OnChanges, OnDestroy {
  @ViewChild('mapContainer') mapContainer!: ElementRef;

  @Input() features: TestResultFeature[] = [];
  @Input() observations: ObservationFeature[] = [];
  @Input() sensors: SensorFeature[] = [];
  @Input() boundary = '';
  @Input() projectId = '';
  @Input() coverageCells: CoverageCell[] = [];

  @Output() boundsChanged = new EventEmitter<string>();
  @Output() featureSelected = new EventEmitter<TestResultFeature>();
  @Output() addTestRequested = new EventEmitter<{latitude: number, longitude: number}>();

  contextMenuVisible = false;
  contextMenuX = 0;
  contextMenuY = 0;
  private contextMenuLatLng: L.LatLng | null = null;

  private map!: L.Map;
  private markerCluster!: L.MarkerClusterGroup;
  private observationLayer!: L.LayerGroup;
  private sensorLayer!: L.LayerGroup;
  private boundaryLayer!: L.LayerGroup;
  private coverageLayer!: L.LayerGroup;
  private boundsTimer: ReturnType<typeof setTimeout> | null = null;
  drawingMode = false;
  private drawHandler: any = null;
  private initialized = false;

  constructor(private projectService: ProjectService, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {}

  ngAfterViewInit(): void {
    this.initMap();
    this.initialized = true;
    this.updateMarkers();
    this.updateBoundary();
    this.updateObservations();
    this.updateSensors();
    this.updateCoverage();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (!this.initialized) return;
    if (changes['features']) this.updateMarkers();
    if (changes['boundary']) this.updateBoundary();
    if (changes['observations']) this.updateObservations();
    if (changes['sensors']) this.updateSensors();
    if (changes['coverageCells']) this.updateCoverage();
  }

  ngOnDestroy(): void {
    if (this.boundsTimer) clearTimeout(this.boundsTimer);
    if (this.map) this.map.remove();
  }

  private initMap(): void {
    this.map = L.map(this.mapContainer.nativeElement, {
      center: [39.8283, -98.5795],
      zoom: 5
    });

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '&copy; OpenStreetMap contributors',
      maxZoom: 19
    }).addTo(this.map);

    this.markerCluster = (L as any).markerClusterGroup();
    this.map.addLayer(this.markerCluster);

    this.observationLayer = L.layerGroup().addTo(this.map);
    this.sensorLayer = L.layerGroup().addTo(this.map);
    this.boundaryLayer = L.layerGroup().addTo(this.map);
    this.coverageLayer = L.layerGroup().addTo(this.map);

    this.map.on('moveend', () => {
      if (this.boundsTimer) clearTimeout(this.boundsTimer);
      this.boundsTimer = setTimeout(() => {
        const b = this.map.getBounds();
        const bbox = `${b.getWest()},${b.getSouth()},${b.getEast()},${b.getNorth()}`;
        this.boundsChanged.emit(bbox);
      }, 300);
    });

    this.map.on('contextmenu', (e: L.LeafletMouseEvent) => {
      e.originalEvent.preventDefault();
      this.contextMenuLatLng = e.latlng;
      this.contextMenuX = e.originalEvent.offsetX;
      this.contextMenuY = e.originalEvent.offsetY;
      this.contextMenuVisible = true;
      this.cdr.markForCheck();
    });

    // Close context menu on regular click or map move
    this.map.on('click', () => {
      this.contextMenuVisible = false;
      this.cdr.markForCheck();
    });
    this.map.on('movestart', () => {
      this.contextMenuVisible = false;
      this.cdr.markForCheck();
    });

    // Handle draw:created event
    this.map.on('draw:created' as any, (e: any) => {
      const layer = e.layer;
      const geoJson = JSON.stringify(layer.toGeoJSON().geometry);
      this.drawingMode = false;
      if (this.drawHandler) {
        this.drawHandler.disable();
        this.drawHandler = null;
      }
      if (this.projectId) {
        this.projectService.createBoundary(this.projectId, geoJson).subscribe({
          next: (boundary) => {
            this.boundary = boundary.geoJson;
            this.updateBoundary();
          }
        });
      }
    });
  }

  private updateMarkers(): void {
    if (!this.markerCluster) return;
    this.markerCluster.clearLayers();

    const statusColors: Record<string, string> = {
      'Pass': '#22c55e',
      'Warn': '#f59e0b',
      'Fail': '#ef4444'
    };

    for (const f of this.features) {
      const color = statusColors[f.status] ?? '#6b7280';
      const marker = L.circleMarker([f.latitude, f.longitude], {
        radius: 8,
        fillColor: color,
        color: '#fff',
        weight: 2,
        opacity: 1,
        fillOpacity: 0.8
      });
      marker.bindPopup(`
        <strong>${f.testTypeName}</strong><br/>
        Value: ${f.value} ${f.unit}<br/>
        Status: ${f.status}<br/>
        ${new Date(f.timestamp).toLocaleString()}
      `);
      marker.on('click', () => this.featureSelected.emit(f));
      this.markerCluster.addLayer(marker);
    }
  }

  private updateBoundary(): void {
    if (!this.boundaryLayer) return;
    this.boundaryLayer.clearLayers();
    if (!this.boundary) return;

    try {
      const geoJson = JSON.parse(this.boundary);
      const layer = L.geoJSON(geoJson, {
        style: {
          color: '#3b82f6',
          weight: 2,
          fillColor: '#3b82f6',
          fillOpacity: 0.1
        }
      });
      this.boundaryLayer.addLayer(layer);
      this.map.fitBounds(layer.getBounds(), { padding: [20, 20] });
    } catch {
      // Invalid GeoJSON, ignore
    }
  }

  private updateObservations(): void {
    if (!this.observationLayer) return;
    this.observationLayer.clearLayers();

    for (const o of this.observations) {
      const marker = L.circleMarker([o.latitude, o.longitude], {
        radius: 7,
        fillColor: '#3b82f6',
        color: '#fff',
        weight: 2,
        opacity: 1,
        fillOpacity: 0.8
      });
      marker.bindPopup(`
        <strong>Observation</strong><br/>
        ${o.note}<br/>
        ${o.tags ? 'Tags: ' + o.tags + '<br/>' : ''}
        ${new Date(o.timestamp).toLocaleString()}
      `);
      this.observationLayer.addLayer(marker);
    }
  }

  private updateSensors(): void {
    if (!this.sensorLayer) return;
    this.sensorLayer.clearLayers();

    for (const s of this.sensors) {
      const marker = L.circleMarker([s.latitude, s.longitude], {
        radius: 7,
        fillColor: '#a855f7',
        color: '#fff',
        weight: 2,
        opacity: 1,
        fillOpacity: 0.8
      });
      marker.bindPopup(`
        <strong>Sensor</strong><br/>
        Type: ${s.type}
      `);
      this.sensorLayer.addLayer(marker);
    }
  }

  private updateCoverage(): void {
    if (!this.coverageLayer) return;
    this.coverageLayer.clearLayers();

    for (const cell of this.coverageCells) {
      const bounds: L.LatLngBoundsExpression = [
        [cell.minLat, cell.minLon],
        [cell.maxLat, cell.maxLon]
      ];
      const color = cell.count > 0 ? '#22c55e' : '#ef4444';
      const rect = L.rectangle(bounds, {
        color,
        weight: 1,
        fillColor: color,
        fillOpacity: 0.2
      });
      this.coverageLayer.addLayer(rect);
    }
  }

  onContextMenuAddTest(): void {
    if (this.contextMenuLatLng) {
      this.addTestRequested.emit({
        latitude: this.contextMenuLatLng.lat,
        longitude: this.contextMenuLatLng.lng
      });
    }
    this.contextMenuVisible = false;
  }

  startDrawBoundary(): void {
    this.drawingMode = true;
    this.drawHandler = new (L.Draw as any).Polygon(this.map, {
      allowIntersection: false,
      showArea: true,
      shapeOptions: {
        color: '#3b82f6',
        weight: 2,
        fillColor: '#3b82f6',
        fillOpacity: 0.1
      }
    });
    this.drawHandler.enable();
  }

  cancelDraw(): void {
    this.drawingMode = false;
    if (this.drawHandler) {
      this.drawHandler.disable();
      this.drawHandler = null;
    }
  }

  onGeoJsonImport(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file || !this.projectId) return;

    const reader = new FileReader();
    reader.onload = () => {
      const geoJsonStr = reader.result as string;
      try {
        // Validate it's valid JSON
        JSON.parse(geoJsonStr);
        this.projectService.createBoundary(this.projectId, geoJsonStr).subscribe({
          next: (boundary) => {
            this.boundary = boundary.geoJson;
            this.updateBoundary();
          }
        });
      } catch {
        // Invalid JSON
      }
    };
    reader.readAsText(file);
    input.value = '';
  }

  fitBoundsToPoint(lat: number, lon: number): void {
    if (this.map) {
      this.map.setView([lat, lon], 15);
    }
  }
}
