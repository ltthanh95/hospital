// auth.interceptor.ts
import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const authReq = req.clone({ withCredentials: true });

  return next(authReq).pipe(
    catchError(err => {
      const isSilentAuthCheck = req.url.endsWith('/auth/me');
      if (err.status === 401 && !isSilentAuthCheck) {
        router.navigate(['/login']);
      }
      return throwError(() => err);
    })
  );
};