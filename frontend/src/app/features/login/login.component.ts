import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="min-h-screen flex items-center justify-center bg-gray-100">
      <div class="bg-white rounded-lg shadow-lg w-full max-w-sm p-8">
        <div class="text-center mb-6">
          <h1 class="text-2xl font-bold text-gray-900">GeoOps</h1>
          <p class="text-sm text-gray-500 mt-1">TTEC Field Platform</p>
        </div>

        <form (ngSubmit)="onSubmit()" class="space-y-4">
          <div>
            <label for="email" class="block text-sm font-medium text-gray-700 mb-1">Email</label>
            <input
              id="email"
              type="email"
              [(ngModel)]="email"
              name="email"
              required
              class="w-full px-3 py-2 border border-gray-300 rounded-md text-sm focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
              placeholder="you@example.com" />
          </div>

          <div>
            <label for="password" class="block text-sm font-medium text-gray-700 mb-1">Password</label>
            <input
              id="password"
              type="password"
              [(ngModel)]="password"
              name="password"
              required
              class="w-full px-3 py-2 border border-gray-300 rounded-md text-sm focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
              placeholder="Enter password" />
          </div>

          @if (errorMessage) {
            <div class="p-3 bg-red-50 border border-red-200 rounded-md text-sm text-red-700">
              {{ errorMessage }}
            </div>
          }

          <button
            type="submit"
            [disabled]="submitting || !email || !password"
            class="w-full px-4 py-2 text-sm bg-blue-600 text-white rounded-md hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed">
            @if (submitting) {
              <span class="inline-flex items-center justify-center gap-1">
                <span class="animate-spin h-3 w-3 border-2 border-white border-t-transparent rounded-full"></span>
                Signing in...
              </span>
            } @else {
              Sign In
            }
          </button>
        </form>

        <p class="mt-4 text-xs text-gray-400 text-center">
          Dev credentials: admin&#64;test.com / password
        </p>
      </div>
    </div>
  `
})
export class LoginComponent {
  private authService = inject(AuthService);
  private router = inject(Router);

  email = '';
  password = '';
  submitting = false;
  errorMessage = '';

  onSubmit(): void {
    if (!this.email || !this.password) return;
    this.submitting = true;
    this.errorMessage = '';

    this.authService.login(this.email, this.password).subscribe({
      next: () => {
        this.submitting = false;
        this.router.navigate(['/']);
      },
      error: (err) => {
        this.submitting = false;
        this.errorMessage = err.error?.message || 'Invalid email or password.';
      }
    });
  }
}
