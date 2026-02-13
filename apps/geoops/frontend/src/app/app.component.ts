import { Component, inject } from '@angular/core';
import { RouterOutlet, RouterLink } from '@angular/router';
import { ToastComponent } from './features/toast/toast.component';
import { AuthService } from './core/services/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, ToastComponent],
  template: `
    @if (authService.isLoggedIn()) {
      <div class="flex h-screen bg-gray-50">
        <aside class="w-56 bg-gray-900 text-white flex flex-col shrink-0">
          <div class="p-4 border-b border-gray-700">
            <a routerLink="/" class="text-xl font-bold tracking-wide">GeoOps</a>
            <p class="text-xs text-gray-400 mt-1">TTEC Field Platform</p>
          </div>
          <nav class="flex-1 p-3 space-y-1">
            <a
              routerLink="/"
              class="flex items-center gap-2 px-3 py-2 rounded-md text-sm text-gray-300 hover:bg-gray-800 hover:text-white transition-colors">
              Projects
            </a>
          </nav>
          <div class="p-3 border-t border-gray-700">
            @if (authService.getUser(); as user) {
              <p class="text-sm text-gray-300 truncate mb-2">{{ user.name }}</p>
            }
            <button
              (click)="authService.logout()"
              class="w-full px-3 py-1.5 text-xs text-gray-400 hover:text-white hover:bg-gray-800 rounded transition-colors">
              Sign Out
            </button>
            <a href="https://github.com/bcalisch/ttec" target="_blank" rel="noopener noreferrer"
              class="flex items-center gap-1.5 mt-3 px-3 py-1.5 text-xs text-gray-500 hover:text-gray-300 transition-colors">
              <svg class="w-3.5 h-3.5" fill="currentColor" viewBox="0 0 24 24"><path d="M12 0C5.37 0 0 5.37 0 12c0 5.31 3.435 9.795 8.205 11.385.6.105.825-.255.825-.57 0-.285-.015-1.23-.015-2.235-3.015.555-3.795-.735-4.035-1.41-.135-.345-.72-1.41-1.23-1.695-.42-.225-1.02-.78-.015-.795.945-.015 1.62.87 1.845 1.23 1.08 1.815 2.805 1.305 3.495.99.105-.78.42-1.305.765-1.605-2.67-.3-5.46-1.335-5.46-5.925 0-1.305.465-2.385 1.23-3.225-.12-.3-.54-1.53.12-3.18 0 0 1.005-.315 3.3 1.23.96-.27 1.98-.405 3-.405s2.04.135 3 .405c2.295-1.56 3.3-1.23 3.3-1.23.66 1.65.24 2.88.12 3.18.765.84 1.23 1.905 1.23 3.225 0 4.605-2.805 5.625-5.475 5.925.435.375.81 1.095.81 2.22 0 1.605-.015 2.895-.015 3.3 0 .315.225.69.825.57A12.02 12.02 0 0024 12c0-6.63-5.37-12-12-12z"/></svg>
              Source Code
            </a>
          </div>
        </aside>
        <main class="flex-1 overflow-auto">
          <router-outlet />
        </main>
      </div>
    } @else {
      <router-outlet />
    }
    <app-toast />
  `,
  styles: [`
    :host {
      display: block;
      height: 100vh;
    }
  `]
})
export class AppComponent {
  authService = inject(AuthService);
}
