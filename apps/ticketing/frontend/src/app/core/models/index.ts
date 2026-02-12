export enum TicketStatus {
  Open = 'Open',
  InProgress = 'InProgress',
  AwaitingCustomer = 'AwaitingCustomer',
  AwaitingParts = 'AwaitingParts',
  Resolved = 'Resolved',
  Closed = 'Closed'
}

export enum TicketPriority {
  Low = 'Low',
  Medium = 'Medium',
  High = 'High',
  Critical = 'Critical'
}

export enum TicketCategory {
  Software = 'Software',
  Hardware = 'Hardware',
  Calibration = 'Calibration',
  Training = 'Training',
  FieldSupport = 'FieldSupport',
  Other = 'Other'
}

export enum EquipmentType {
  Roller = 'Roller',
  Paver = 'Paver',
  MillingMachine = 'MillingMachine',
  Sensor = 'Sensor',
  Software = 'Software',
  Other = 'Other'
}

export enum EquipmentManufacturer {
  BOMAG = 'BOMAG',
  CAT = 'CAT',
  HAMM = 'HAMM',
  Volvo = 'Volvo',
  Dynapac = 'Dynapac',
  Other = 'Other'
}

export interface Ticket {
  id: string;
  title: string;
  description: string;
  status: TicketStatus;
  priority: TicketPriority;
  category: TicketCategory;
  reportedBy: string;
  assignedTo?: string;
  longitude?: number;
  latitude?: number;
  equipmentId?: string;
  sourceApp?: string;
  sourceEntityType?: string;
  sourceEntityId?: string;
  createdAt: string;
  updatedAt: string;
  resolvedAt?: string;
  slaDeadline?: string;
  isOverdue: boolean;
}

export interface CreateTicketRequest {
  title: string;
  description: string;
  priority: TicketPriority;
  category: TicketCategory;
  equipmentId?: string;
  longitude?: number;
  latitude?: number;
  assignedTo?: string;
  sourceApp?: string;
  sourceEntityType?: string;
  sourceEntityId?: string;
}

export interface UpdateTicketRequest {
  title: string;
  description: string;
  status: TicketStatus;
  priority: TicketPriority;
  category: TicketCategory;
  assignedTo?: string;
  equipmentId?: string;
  longitude?: number;
  latitude?: number;
}

export interface TicketComment {
  id: string;
  ticketId: string;
  author: string;
  body: string;
  isInternal: boolean;
  createdAt: string;
}

export interface CreateCommentRequest {
  body: string;
  isInternal: boolean;
}

export interface TimeEntry {
  id: string;
  ticketId: string;
  hours: number;
  hourlyRate: number;
  description: string;
  technician: string;
  createdAt: string;
}

export interface CreateTimeEntryRequest {
  hours: number;
  description: string;
}

export interface Equipment {
  id: string;
  name: string;
  serialNumber: string;
  type: EquipmentType;
  manufacturer: EquipmentManufacturer;
  model?: string;
  createdAt: string;
}

export interface CreateEquipmentRequest {
  name: string;
  serialNumber: string;
  type: EquipmentType;
  manufacturer: EquipmentManufacturer;
  model?: string;
}

export interface UpdateEquipmentRequest {
  name: string;
  serialNumber: string;
  type: EquipmentType;
  manufacturer: EquipmentManufacturer;
  model?: string;
}

export interface KnowledgeArticle {
  id: string;
  title: string;
  content: string;
  tags: string;
  isPublished: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateKnowledgeArticleRequest {
  title: string;
  content: string;
  tags: string;
  isPublished: boolean;
}

export interface UpdateKnowledgeArticleRequest {
  title: string;
  content: string;
  tags: string;
  isPublished: boolean;
}

export interface TicketBilling {
  ticketId: string;
  ticketTitle: string;
  baseCharge: number;
  hourlyTotal: number;
  totalCharge: number;
  timeEntries: BillingLineItem[];
}

export interface BillingLineItem {
  description: string;
  hours: number;
  rate: number;
  total: number;
}

export interface BillingSummary {
  ticketCount: number;
  totalBaseCharges: number;
  totalHourlyCharges: number;
  grandTotal: number;
}

export interface SlaSummary {
  total: number;
  withinSla: number;
  overdue: number;
  noSla: number;
}
