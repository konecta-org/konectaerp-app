import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const permissionGuard: CanActivateFn = (route) => {
  const permission = route.data?.['permission'] as string | undefined;
  const auth = inject(AuthService);
  const router = inject(Router);

  if (!permission || auth.hasPermission(permission)) {
    return true;
  }

  router.navigate(['/']);
  return false;
};
