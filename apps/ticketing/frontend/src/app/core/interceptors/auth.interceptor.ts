import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const baseUrl = window.__APP_CONFIG__?.apiBaseUrl ?? '';

  let url = req.url;
  if (url.startsWith('/api')) {
    url = `${baseUrl}${url}`;
  }

  const token = auth.getToken();
  const headers = token
    ? req.headers.set('Authorization', `Bearer ${token}`)
    : req.headers;

  return next(req.clone({ url, headers }));
};
