import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';

export const roleGuard: CanActivateFn = async (route) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  await auth.ensureInitialized();

  const allowedRoles = route.data['roles'] as string[];
  const userRole = auth.user()?.role;

  if (userRole && allowedRoles.includes(userRole)) return true;

  router.navigate(['/unauthorized']);
  return false;
};