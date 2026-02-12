import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AuthService } from './core/services/auth.service';
import { ToastComponent } from './features/toast/toast.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterModule, ToastComponent],
  template: `
    <div class="h-screen flex">
      @if (auth.isLoggedIn()) {
        <aside class="w-56 bg-gray-900 text-white flex flex-col shrink-0">
          <div class="p-4 border-b border-gray-700">
            <h1 class="text-lg font-bold tracking-tight">TTEC Ticketing</h1>
          </div>
          <nav class="flex-1 py-4 space-y-1 px-2">
            <a routerLink="/tickets" routerLinkActive="bg-gray-700"
              class="flex items-center gap-2 px-3 py-2 rounded-md text-sm hover:bg-gray-800 transition-colors">
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                  d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2" />
              </svg>
              Tickets
            </a>
            <a routerLink="/equipment" routerLinkActive="bg-gray-700"
              class="flex items-center gap-2 px-3 py-2 rounded-md text-sm hover:bg-gray-800 transition-colors">
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                  d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.066 2.573c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.573 1.066c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.066-2.573c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z" />
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
              </svg>
              Equipment
            </a>
            <a routerLink="/knowledge-base" routerLinkActive="bg-gray-700"
              class="flex items-center gap-2 px-3 py-2 rounded-md text-sm hover:bg-gray-800 transition-colors">
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                  d="M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.747 0 3.332.477 4.5 1.253v13C19.832 18.477 18.247 18 16.5 18c-1.746 0-3.332.477-4.5 1.253" />
              </svg>
              Knowledge Base
            </a>
            <a routerLink="/dashboard" routerLinkActive="bg-gray-700"
              class="flex items-center gap-2 px-3 py-2 rounded-md text-sm hover:bg-gray-800 transition-colors">
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                  d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
              </svg>
              Dashboard
            </a>
          </nav>
          <div class="p-4 border-t border-gray-700">
            <div class="text-xs text-gray-400 mb-2">{{ auth.getUser()?.displayName }}</div>
            <button (click)="auth.logout()"
              class="w-full text-left text-sm text-gray-300 hover:text-white transition-colors">
              Sign out
            </button>
          </div>
        </aside>
      }
      <main class="flex-1 overflow-auto">
        <router-outlet />
      </main>
    </div>
    <app-toast />
  `
})
export class AppComponent {
  constructor(public auth: AuthService) {}
}
