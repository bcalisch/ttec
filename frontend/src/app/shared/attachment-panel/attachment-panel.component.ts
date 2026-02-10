import { Component, Input, OnInit, OnChanges, SimpleChanges, inject, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AttachmentService, AttachmentResponse } from '../../core/services/attachment.service';
import { ToastService } from '../../core/services/toast.service';

@Component({
  selector: 'app-attachment-panel',
  standalone: true,
  imports: [CommonModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="space-y-3">
      <div class="flex items-center justify-between">
        <h4 class="text-sm font-medium text-gray-700">Attachments</h4>
        <label class="px-2 py-1 text-xs bg-blue-600 text-white rounded cursor-pointer hover:bg-blue-700">
          Upload
          <input type="file" (change)="onFileSelect($event)" class="hidden" accept="image/*,.pdf,.doc,.docx" />
        </label>
      </div>

      @if (uploading) {
        <div class="flex items-center gap-2 text-sm text-gray-500">
          <div class="animate-spin h-3 w-3 border-2 border-blue-600 border-t-transparent rounded-full"></div>
          Uploading...
        </div>
      }

      @if (attachments.length > 0) {
        <div class="grid grid-cols-3 gap-2">
          @for (att of attachments; track att.id) {
            <div class="relative group">
              @if (isImage(att.contentType)) {
                <img [src]="att.url" [alt]="att.id"
                  class="w-full h-20 object-cover rounded border border-gray-200" />
              } @else {
                <div class="w-full h-20 flex items-center justify-center bg-gray-100 rounded border border-gray-200">
                  <span class="text-xs text-gray-500 text-center px-1">{{ fileExtension(att.contentType) }}</span>
                </div>
              }
              <button
                (click)="deleteAttachment(att.id)"
                class="absolute top-0.5 right-0.5 w-5 h-5 bg-red-500 text-white rounded-full text-xs leading-none flex items-center justify-center opacity-0 group-hover:opacity-100 transition-opacity">
                &times;
              </button>
            </div>
          }
        </div>
      } @else if (!uploading) {
        <p class="text-xs text-gray-400">No attachments</p>
      }
    </div>
  `
})
export class AttachmentPanelComponent implements OnInit, OnChanges {
  @Input() entityType = '';
  @Input() entityId = '';

  private attachmentService = inject(AttachmentService);
  private toastService = inject(ToastService);
  private cdr = inject(ChangeDetectorRef);

  attachments: AttachmentResponse[] = [];
  uploading = false;

  ngOnInit(): void {
    this.loadAttachments();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['entityId'] && !changes['entityId'].firstChange) {
      this.loadAttachments();
    }
  }

  loadAttachments(): void {
    if (!this.entityType || !this.entityId) return;
    this.attachmentService.list(this.entityType, this.entityId).subscribe({
      next: (attachments) => {
        this.attachments = attachments;
        this.cdr.markForCheck();
      }
    });
  }

  onFileSelect(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;
    this.uploading = true;
    this.cdr.markForCheck();
    this.attachmentService.upload(this.entityType, this.entityId, file).subscribe({
      next: (att) => {
        this.attachments.push(att);
        this.uploading = false;
        this.toastService.success('File uploaded');
        this.cdr.markForCheck();
      },
      error: () => {
        this.uploading = false;
        this.toastService.error('Failed to upload file');
        this.cdr.markForCheck();
      }
    });
    input.value = '';
  }

  deleteAttachment(id: string): void {
    this.attachmentService.delete(id).subscribe({
      next: () => {
        this.attachments = this.attachments.filter(a => a.id !== id);
        this.toastService.success('Attachment deleted');
        this.cdr.markForCheck();
      },
      error: () => {
        this.toastService.error('Failed to delete attachment');
      }
    });
  }

  isImage(contentType: string): boolean {
    return contentType?.startsWith('image/') ?? false;
  }

  fileExtension(contentType: string): string {
    const map: Record<string, string> = {
      'application/pdf': 'PDF',
      'application/msword': 'DOC',
      'application/vnd.openxmlformats-officedocument.wordprocessingml.document': 'DOCX'
    };
    return map[contentType] ?? 'FILE';
  }
}
