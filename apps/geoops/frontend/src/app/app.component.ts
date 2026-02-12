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
