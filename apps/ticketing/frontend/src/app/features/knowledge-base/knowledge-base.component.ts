import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { KnowledgeArticleService } from '../../core/services/knowledge-article.service';
import { ToastService } from '../../core/services/toast.service';
import { KnowledgeArticle, CreateKnowledgeArticleRequest } from '../../core/models';

@Component({
  selector: 'app-knowledge-base',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="p-6">
      <div class="flex items-center justify-between mb-6">
        <h1 class="text-2xl font-bold text-gray-900">Knowledge Base</h1>
        <button (click)="showForm = !showForm"
          class="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors">
          {{ showForm ? 'Cancel' : 'New Article' }}
        </button>
      </div>

      <!-- Tag filter -->
      <div class="mb-4 flex gap-2 items-center">
        <input [(ngModel)]="filterTag" (keyup.enter)="loadArticles()" placeholder="Filter by tag..."
          class="px-3 py-1.5 text-sm border border-gray-300 rounded-md w-48" />
        <button (click)="loadArticles()" class="px-3 py-1.5 text-sm bg-gray-100 rounded-md hover:bg-gray-200">Filter</button>
        @if (filterTag) {
          <button (click)="filterTag = ''; loadArticles()" class="text-sm text-blue-600 hover:underline">Clear</button>
        }
      </div>

      @if (showForm) {
        <div class="mb-6 p-4 bg-white rounded-lg shadow border border-gray-200">
          <h2 class="text-lg font-semibold mb-4">{{ editingId ? 'Edit Article' : 'New Article' }}</h2>
          <form (ngSubmit)="onSubmit()" class="space-y-4">
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Title *</label>
              <input [(ngModel)]="form.title" name="title" required
                class="w-full px-3 py-2 border border-gray-300 rounded-md" placeholder="Article title" />
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Content *</label>
              <textarea [(ngModel)]="form.content" name="content" required rows="6"
                class="w-full px-3 py-2 border border-gray-300 rounded-md" placeholder="Article content..."></textarea>
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Tags (comma-separated)</label>
              <input [(ngModel)]="form.tags" name="tags"
                class="w-full px-3 py-2 border border-gray-300 rounded-md" placeholder="calibration,bomag,ic-roller" />
            </div>
            <label class="flex items-center gap-2 text-sm">
              <input type="checkbox" [(ngModel)]="form.isPublished" name="isPublished" class="rounded border-gray-300" />
              Published
            </label>
            <button type="submit" [disabled]="!form.title || !form.content"
              class="px-4 py-2 bg-green-600 text-white rounded-md hover:bg-green-700 disabled:opacity-50">
              {{ editingId ? 'Save Changes' : 'Create Article' }}
            </button>
          </form>
        </div>
      }

      @if (loading) {
        <div class="flex justify-center py-12">
          <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
        </div>
      } @else {
        <div class="space-y-4">
          @for (article of articles; track article.id) {
            <div class="bg-white rounded-lg shadow border border-gray-200 overflow-hidden">
              <div class="p-4 cursor-pointer" (click)="toggleExpand(article.id)">
                <div class="flex items-start justify-between">
                  <div>
                    <h3 class="text-lg font-semibold text-gray-900">{{ article.title }}</h3>
                    <div class="flex gap-1 mt-1">
                      @for (tag of article.tags.split(','); track tag) {
                        @if (tag.trim()) {
                          <span class="text-xs px-2 py-0.5 rounded-full bg-blue-50 text-blue-600">{{ tag.trim() }}</span>
                        }
                      }
                    </div>
                  </div>
                  <div class="flex items-center gap-2">
                    @if (!article.isPublished) {
                      <span class="text-xs px-2 py-1 rounded-full bg-yellow-100 text-yellow-700">Draft</span>
                    }
                    <span class="text-xs text-gray-400">{{ article.updatedAt | date:'mediumDate' }}</span>
                  </div>
                </div>
              </div>
              @if (expandedId === article.id) {
                <div class="px-4 pb-4 border-t border-gray-100 pt-3">
                  <p class="text-sm text-gray-700 whitespace-pre-wrap mb-3">{{ article.content }}</p>
                  <div class="flex gap-2">
                    <button (click)="editArticle(article)" class="text-xs text-blue-600 hover:underline">Edit</button>
                  </div>
                </div>
              }
            </div>
          } @empty {
            <p class="text-gray-500 text-center py-8">No articles found.</p>
          }
        </div>
      }
    </div>
  `
})
export class KnowledgeBaseComponent implements OnInit {
  articles: KnowledgeArticle[] = [];
  loading = true;
  showForm = false;
  editingId = '';
  expandedId = '';
  filterTag = '';
  form: CreateKnowledgeArticleRequest = { title: '', content: '', tags: '', isPublished: true };

  constructor(
    private articleService: KnowledgeArticleService,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.loadArticles();
  }

  loadArticles(): void {
    this.loading = true;
    this.articleService.getAll(this.filterTag || undefined).subscribe({
      next: (a) => { this.articles = a; this.loading = false; },
      error: () => { this.loading = false; }
    });
  }

  toggleExpand(id: string): void {
    this.expandedId = this.expandedId === id ? '' : id;
  }

  onSubmit(): void {
    if (this.editingId) {
      this.articleService.update(this.editingId, this.form).subscribe({
        next: () => {
          this.toastService.success('Article updated');
          this.resetForm();
          this.loadArticles();
        },
        error: () => this.toastService.error('Failed to update article')
      });
    } else {
      this.articleService.create(this.form).subscribe({
        next: () => {
          this.toastService.success('Article created');
          this.resetForm();
          this.loadArticles();
        },
        error: () => this.toastService.error('Failed to create article')
      });
    }
  }

  editArticle(article: KnowledgeArticle): void {
    this.editingId = article.id;
    this.form = {
      title: article.title, content: article.content,
      tags: article.tags, isPublished: article.isPublished
    };
    this.showForm = true;
  }

  private resetForm(): void {
    this.showForm = false;
    this.editingId = '';
    this.form = { title: '', content: '', tags: '', isPublished: true };
  }
}
