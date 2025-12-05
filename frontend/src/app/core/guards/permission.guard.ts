import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const permissionGuard: CanActivateFn = (route) => {
  const permission = route.data?.['permission'] as string | undefined;
  const permissionPrefix = route.data?.['permissionPrefix'] as string | undefined;
  const roles = route.data?.['roles'] as string[] | undefined;
  const auth = inject(AuthService);
  const router = inject(Router);

  const passesPermission = permission ? auth.hasPermission(permission) : false;
  const passesPrefix = permissionPrefix ? auth.hasPermissionPrefix(permissionPrefix) : false;
  const passesRole = !roles || roles.some(role => auth.hasRole(role));

  const requirementMissing = permission || permissionPrefix || roles;
  const permissionSatisfied = !requirementMissing || passesPermission || passesPrefix;

  if (permissionSatisfied && passesRole) {
    return true;
  }

  router.navigate(['/']);
  return false;
};
