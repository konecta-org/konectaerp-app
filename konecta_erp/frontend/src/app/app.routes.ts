import { Routes } from '@angular/router';
import { MainLayoutComponent } from './core/layout/main-layout.component';
import { authGuard } from './core/guards/auth.guard';
import { permissionGuard } from './core/guards/permission.guard';

const loadLoginComponent = () => import('./auth/login/login.component').then(m => m.LoginComponent);
const loadRegisterComponent = () => import('./auth/register/register.component').then(m => m.RegisterComponent);
const loadUserManagementComponent = () => import('./usermanagement/user-management.component').then(m => m.UserManagementComponent);
const loadHrEmployeesComponent = () => import('./hr/hr-employees.component').then(m => m.HrEmployeesComponent);
const loadAuthApiConsoleComponent = () => import('./auth/auth-api-console.component').then(m => m.AuthApiConsoleComponent);
const loadUserManagementApiConsoleComponent = () => import('./usermanagement/user-management-api-console.component').then(m => m.UserManagementApiConsoleComponent);
const loadHrApiConsoleComponent = () => import('./hr/hr-api-console.component').then(m => m.HrApiConsoleComponent);

export const routes: Routes = [
  {
    path: 'auth',
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'login' },
      { path: 'login', loadComponent: loadLoginComponent },
      { path: 'register', loadComponent: loadRegisterComponent }
    ]
  },
  {
    path: '',
    component: MainLayoutComponent,
    canActivate: [authGuard],
    canActivateChild: [authGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'users' },
      {
        path: 'users',
        loadComponent: loadUserManagementComponent,
        canActivate: [permissionGuard],
        data: { permission: 'user-management.users.read' }
      },
      {
        path: 'hr',
        children: [
          {
            path: 'employees',
            loadComponent: loadHrEmployeesComponent,
            canActivate: [permissionGuard],
            data: { permission: 'hr.employees.read' }
          },
          { path: '', pathMatch: 'full', redirectTo: 'employees' }
        ]
      },
      {
        path: 'workspace',
        children: [
          {
            path: 'auth-api',
            loadComponent: loadAuthApiConsoleComponent,
            canActivate: [permissionGuard],
            data: { permission: 'user-management.permissions.manage' }
          },
          {
            path: 'user-api',
            loadComponent: loadUserManagementApiConsoleComponent,
            canActivate: [permissionGuard],
            data: { permission: 'user-management.users.manage' }
          },
          {
            path: 'hr-api',
            loadComponent: loadHrApiConsoleComponent,
            canActivate: [permissionGuard],
            data: { permission: 'hr.employees.manage' }
          },
          { path: '', pathMatch: 'full', redirectTo: 'auth-api' }
        ]
      }
    ]
  },
  { path: '**', redirectTo: '' }
];
