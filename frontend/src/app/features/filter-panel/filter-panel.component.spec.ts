import { TestBed, ComponentFixture } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { FilterPanelComponent } from './filter-panel.component';

describe('FilterPanelComponent', () => {
  let component: FilterPanelComponent;
  let fixture: ComponentFixture<FilterPanelComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FilterPanelComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()]
    }).compileComponents();

    fixture = TestBed.createComponent(FilterPanelComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should not emit status filter when all statuses checked', () => {
    component.showPass = true;
    component.showWarn = true;
    component.showFail = true;

    let emitted: Record<string, string> = {};
    component.filtersChanged.subscribe(f => emitted = f);
    component.emitFilters();

    expect(emitted['status']).toBeUndefined();
  });

  it('should emit status=Warn,Fail when Pass is unchecked', () => {
    component.showPass = false;
    component.showWarn = true;
    component.showFail = true;

    let emitted: Record<string, string> = {};
    component.filtersChanged.subscribe(f => emitted = f);
    component.emitFilters();

    expect(emitted['status']).toBe('Warn,Fail');
  });

  it('should emit status=Pass when Warn and Fail are unchecked', () => {
    component.showPass = true;
    component.showWarn = false;
    component.showFail = false;

    let emitted: Record<string, string> = {};
    component.filtersChanged.subscribe(f => emitted = f);
    component.emitFilters();

    expect(emitted['status']).toBe('Pass');
  });

  it('should emit status=Pass,Fail when Warn is unchecked', () => {
    component.showPass = true;
    component.showWarn = false;
    component.showFail = true;

    let emitted: Record<string, string> = {};
    component.filtersChanged.subscribe(f => emitted = f);
    component.emitFilters();

    expect(emitted['status']).toBe('Pass,Fail');
  });

  it('should emit empty status when no statuses checked', () => {
    component.showPass = false;
    component.showWarn = false;
    component.showFail = false;

    let emitted: Record<string, string> = {};
    component.filtersChanged.subscribe(f => emitted = f);
    component.emitFilters();

    // When no statuses are checked, length is 0 which is < 3, so it emits with empty join
    expect(emitted['status']).toBe('');
  });

  it('should not emit types filter when all layers checked', () => {
    component.showTests = true;
    component.showObservations = true;
    component.showSensors = true;

    let emitted: Record<string, string> = {};
    component.filtersChanged.subscribe(f => emitted = f);
    component.emitFilters();

    expect(emitted['types']).toBeUndefined();
  });

  it('should emit types=tests,sensors when observations unchecked', () => {
    component.showTests = true;
    component.showObservations = false;
    component.showSensors = true;

    let emitted: Record<string, string> = {};
    component.filtersChanged.subscribe(f => emitted = f);
    component.emitFilters();

    expect(emitted['types']).toBe('tests,sensors');
  });

  it('should include from and to when set', () => {
    component.from = '2024-01-01';
    component.to = '2024-12-31';

    let emitted: Record<string, string> = {};
    component.filtersChanged.subscribe(f => emitted = f);
    component.emitFilters();

    expect(emitted['from']).toBe('2024-01-01');
    expect(emitted['to']).toBe('2024-12-31');
  });

  it('should include testTypeId when set', () => {
    component.testTypeId = 'tt-abc';

    let emitted: Record<string, string> = {};
    component.filtersChanged.subscribe(f => emitted = f);
    component.emitFilters();

    expect(emitted['testTypeId']).toBe('tt-abc');
  });

  it('should not include from/to/testTypeId when empty', () => {
    component.from = '';
    component.to = '';
    component.testTypeId = '';

    let emitted: Record<string, string> = {};
    component.filtersChanged.subscribe(f => emitted = f);
    component.emitFilters();

    expect(emitted['from']).toBeUndefined();
    expect(emitted['to']).toBeUndefined();
    expect(emitted['testTypeId']).toBeUndefined();
  });
});
