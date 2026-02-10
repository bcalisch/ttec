export enum ProjectStatus {
  Draft = 0,
  Active = 1,
  OnHold = 2,
  Closed = 3
}

export enum TestStatus {
  Pass = 0,
  Warn = 1,
  Fail = 2
}

export interface Project {
  id: string;
  name: string;
  client: string;
  status: ProjectStatus;
  startDate?: string;
  endDate?: string;
}

export interface ProjectBoundary {
  id: string;
  projectId: string;
  geoJson: string;
}

export interface TestType {
  id: string;
  name: string;
  unit: string;
  minThreshold?: number;
  maxThreshold?: number;
}

export interface TestResultFeature {
  id: string;
  testTypeId: string;
  testTypeName: string;
  unit: string;
  timestamp: string;
  value: number;
  status: string;
  longitude: number;
  latitude: number;
}

export interface ObservationFeature {
  id: string;
  timestamp: string;
  note: string;
  tags?: string;
  longitude: number;
  latitude: number;
}

export interface SensorFeature {
  id: string;
  type: string;
  longitude: number;
  latitude: number;
}

export interface FeaturesResponse {
  tests: TestResultFeature[];
  observations: ObservationFeature[];
  sensors: SensorFeature[];
  totalTests: number;
  page: number;
  pageSize: number;
}

export interface OutOfSpecItem {
  id: string;
  testTypeName: string;
  value: number;
  threshold: number;
  severity: number;
  longitude: number;
  latitude: number;
  timestamp: string;
}

export interface CoverageCell {
  minLon: number;
  minLat: number;
  maxLon: number;
  maxLat: number;
  count: number;
}

export interface TrendPoint {
  period: string;
  testTypeName: string;
  avg: number;
  min: number;
  max: number;
}

export interface CreateProjectRequest {
  name: string;
  client: string;
  status: ProjectStatus;
  startDate?: string;
  endDate?: string;
}

export interface CreateTestResultRequest {
  testTypeId: string;
  timestamp: string;
  value: number;
  longitude: number;
  latitude: number;
  source?: string;
  technician?: string;
}

export interface BatchIngestRequest {
  idempotencyKey: string;
  items: CreateTestResultRequest[];
}

export interface CreateObservationRequest {
  latitude: number;
  longitude: number;
  note: string;
  tags?: string;
  timestamp: string;
}

export interface CreateSensorRequest {
  latitude: number;
  longitude: number;
  type: string;
  metadataJson?: string;
}
