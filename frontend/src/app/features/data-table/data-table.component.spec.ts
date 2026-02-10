import { TestBed, ComponentFixture } from '@angular/core/testing';
import { DataTableComponent } from './data-table.component';
import { TestResultFeature } from '../../core/models';

describe('DataTableComponent', () => {
  let component: DataTableComponent;
  let fixture: ComponentFixture<DataTableComponent>;

  const mockFeatures: TestResultFeature[] = [
    {
      id: '1', testTypeId: 'tt1', testTypeName: 'pH', unit: 'pH',
      timestamp: '2024-06-15T10:00:00Z', value: 7.2, status: 'Pass',
      longitude: -100.5, latitude: 40.1
    },
    {
      id: '2', testTypeId: 'tt2', testTypeName: 'Turbidity', unit: 'NTU',
      timestamp: '2024-06-16T10:00:00Z', value: 15.0, status: 'Fail',
      longitude: -100.6, latitude: 40.2
    }
  ];

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DataTableComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(DataTableComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should render rows when features are provided', () => {
    component.features = mockFeatures;
    component.totalItems = 2;
    fixture.detectChanges();

    const rows = fixture.nativeElement.querySelectorAll('tbody tr');
    expect(rows.length).toBe(2);
  });

  it('should show empty message when no features', () => {
    component.features = [];
    fixture.detectChanges();

    const emptyRow = fixture.nativeElement.querySelector('tbody td');
    expect(emptyRow.textContent).toContain('No test results');
  });

  it('should toggle sort direction when clicking same column', () => {
    component.sortField = 'value';
    component.sortAsc = true;
    component.sort('value');
    expect(component.sortAsc).toBeFalse();
  });

  it('should change sort field and reset to ascending', () => {
    component.sortField = 'value';
    component.sortAsc = false;
    component.sort('timestamp');
    expect(component.sortField).toBe('timestamp');
    expect(component.sortAsc).toBeTrue();
  });

  it('should sort features by value ascending', () => {
    component.features = mockFeatures;
    component.sortField = 'value';
    component.sortAsc = true;
    const sorted = component.sortedFeatures;
    expect(sorted[0].value).toBe(7.2);
    expect(sorted[1].value).toBe(15.0);
  });

  it('should sort features by value descending', () => {
    component.features = mockFeatures;
    component.sortField = 'value';
    component.sortAsc = false;
    const sorted = component.sortedFeatures;
    expect(sorted[0].value).toBe(15.0);
    expect(sorted[1].value).toBe(7.2);
  });

  it('should emit rowSelected when a row is clicked', () => {
    component.features = mockFeatures;
    component.totalItems = 2;
    fixture.detectChanges();

    let selected: TestResultFeature | null = null;
    component.rowSelected.subscribe(f => selected = f);

    const row = fixture.nativeElement.querySelector('tbody tr');
    row.click();
    expect(selected).toBeTruthy();
  });

  it('should calculate pagination correctly', () => {
    component.totalItems = 120;
    component.pageSize = 50;
    component.page = 1;
    expect(component.totalPages).toBe(3);
    expect(component.startItem).toBe(1);
    expect(component.endItem).toBe(50);
  });

  it('should calculate last page endItem correctly', () => {
    component.totalItems = 120;
    component.pageSize = 50;
    component.page = 3;
    expect(component.startItem).toBe(101);
    expect(component.endItem).toBe(120);
  });

  it('should emit pageChanged on goToPage', () => {
    component.totalItems = 100;
    component.pageSize = 50;
    component.page = 1;

    let emittedPage = 0;
    component.pageChanged.subscribe(p => emittedPage = p);
    component.goToPage(2);
    expect(emittedPage).toBe(2);
  });

  it('should not emit pageChanged for invalid pages', () => {
    component.totalItems = 100;
    component.pageSize = 50;
    component.page = 1;

    let emitted = false;
    component.pageChanged.subscribe(() => emitted = true);
    component.goToPage(0);
    expect(emitted).toBeFalse();
    component.goToPage(3);
    expect(emitted).toBeFalse();
  });
});
