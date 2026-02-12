import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';

interface LoginResponse {
  token: string;
  user: { id: string; displayName: string; email: string; role: string };
}

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="min-h-screen flex items-center justify-center bg-gray-100">
      <div class="bg-white p-8 rounded-lg shadow-md w-full max-w-sm">
        <h1 class="text-2xl font-bold text-gray-900 mb-2">TTEC Ticketing</h1>
        <p class="text-sm text-gray-500 mb-6">IC/PMTP Support Ticket System</p>
        <form (ngSubmit)="onSubmit()" class="space-y-4">
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Username</label>
            <input
              [(ngModel)]="username"
              name="username"
              required
              class="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
              placeholder="Enter username" />
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Password</label>
            <input
              [(ngModel)]="password"
              name="password"
              type="password"
              required
              class="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
              placeholder="Enter password" />
          </div>
          @if (error) {
            <p class="text-red-600 text-sm">{{ error }}</p>
          }
          <button
            type="submit"
            [disabled]="!username || !password || loading"
            class="w-full py-2 px-4 bg-blue-600 text-white rounded-md hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed">
            {{ loading ? 'Signing in...' : 'Sign in' }}
          </button>
        </form>
      </div>
    </div>
  `
})
export class LoginComponent {
  username = '';
  password = '';
  error = '';
  loading = false;

  constructor(private http: HttpClient, private router: Router) {}

  onSubmit(): void {
    this.error = '';
    this.loading = true;
    const baseUrl = window.__APP_CONFIG__?.apiBaseUrl ?? '';
    this.http.post<LoginResponse>(`${baseUrl}/api/auth/login`, {
      username: this.username,
      password: this.password
    }).subscribe({
      next: (res) => {
        localStorage.setItem('ticketing_token', res.token);
        localStorage.setItem('ticketing_user', JSON.stringify(res.user));
        this.router.navigate(['/']);
      },
      error: () => {
        this.error = 'Invalid credentials';
        this.loading = false;
      }
    });
  }
}
