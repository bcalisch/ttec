import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

interface AppConfig {
  apiBaseUrl: string;
}

declare global {
  interface Window {
    __APP_CONFIG__?: AppConfig;
  }
}

interface LoginResponse {
  token: string;
  user: { id: string; displayName: string; email: string; role: string };
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private readonly TOKEN_KEY = 'ticketing_token';
  private readonly USER_KEY = 'ticketing_user';

  login(username: string, password: string): void {
    const baseUrl = window.__APP_CONFIG__?.apiBaseUrl ?? '';
    this.http.post<LoginResponse>(`${baseUrl}/api/auth/login`, { username, password })
      .subscribe({
        next: (res) => {
          localStorage.setItem(this.TOKEN_KEY, res.token);
          localStorage.setItem(this.USER_KEY, JSON.stringify(res.user));
          window.location.href = '/';
        },
        error: () => {
          this.loginError = 'Invalid credentials';
        }
      });
  }

  loginError = '';

  logout(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.USER_KEY);
    window.location.href = '/login';
  }

  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  getUser(): { id: string; displayName: string; email: string; role: string } | null {
    const raw = localStorage.getItem(this.USER_KEY);
    if (!raw) return null;
    try {
      return JSON.parse(raw);
    } catch {
      return null;
    }
  }

  isLoggedIn(): boolean {
    const token = this.getToken();
    if (!token) return false;
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      return payload.exp * 1000 > Date.now();
    } catch {
      return false;
    }
  }
}
