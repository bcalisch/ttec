import { TestBed } from '@angular/core/testing';
import { ToastService, Toast } from './toast.service';

describe('ToastService', () => {
  let service: ToastService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ToastService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('success() should emit a success toast', (done) => {
    service.toast$.subscribe((toast: Toast) => {
      expect(toast.type).toBe('success');
      expect(toast.message).toBe('Done!');
      expect(toast.id).toBeGreaterThan(0);
      done();
    });
    service.success('Done!');
  });

  it('error() should emit an error toast', (done) => {
    service.toast$.subscribe((toast: Toast) => {
      expect(toast.type).toBe('error');
      expect(toast.message).toBe('Failed');
      done();
    });
    service.error('Failed');
  });

  it('info() should emit an info toast', (done) => {
    service.toast$.subscribe((toast: Toast) => {
      expect(toast.type).toBe('info');
      expect(toast.message).toBe('Info msg');
      done();
    });
    service.info('Info msg');
  });

  it('warning() should emit a warning toast', (done) => {
    service.toast$.subscribe((toast: Toast) => {
      expect(toast.type).toBe('warning');
      expect(toast.message).toBe('Watch out');
      done();
    });
    service.warning('Watch out');
  });

  it('should auto-increment toast IDs', () => {
    const ids: number[] = [];
    const sub = service.toast$.subscribe(t => ids.push(t.id));
    service.success('A');
    service.error('B');
    service.info('C');
    sub.unsubscribe();
    expect(ids.length).toBe(3);
    expect(ids[1]).toBe(ids[0] + 1);
    expect(ids[2]).toBe(ids[1] + 1);
  });
});
