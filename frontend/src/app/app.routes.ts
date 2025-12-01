import { Routes } from '@angular/router';
import { MainLayoutComponent } from './core/layout/main-layout.component';
import { authGuard } from './core/guards/auth.guard';
import { permissionGuard } from './core/guards/permission.guard';
import { AccessDeniedComponent } from './core/pages/access-denied/access-denied.component';

const loadLoginComponent = () => import('./auth/login/login.component').then(m => m.LoginComponent);
const loadRegisterComponent = () => import('./auth/register/register.component').then(m => m.RegisterComponent);
const loadDashboardComponent = () => import('./dashboard/dashboard').then(m => m.Dashboard);
const loadUserManagementComponent = () => import('./usermanagement/user-management.component').then(m => m.UserManagementComponent);
const loadInventoryComponent = () => import('./inventory/inventory').then(m => m.Inventory);
const loadFinanceComponent = () => import('./finance/finance').then(m => m.Finance);
const loadReportingComponent = () => import('./reporting/reporting').then(m => m.Reporting);
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
    path: 'access-denied',
    component: AccessDeniedComponent
  },
  {
    path: '',
    component: MainLayoutComponent,
    canActivate: [authGuard],
    canActivateChild: [authGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
      {
        path: 'dashboard',
        loadComponent: loadDashboardComponent
        // No permission guard - accessible to all authenticated users
      },
      {
        path: 'users',
        loadComponent: loadUserManagementComponent,
        canActivate: [permissionGuard],
        data: { permission: 'user-management.users.read' }
      },
      {
        path: 'inventory',
        loadComponent: loadInventoryComponent
        // Removed permission guard for testing
      },
      {
        path: 'finance',
        loadComponent: loadFinanceComponent
        // Removed permission guard for testing
      },
      {
        path: 'reporting',
        loadComponent: loadReportingComponent
        // Removed permission guard for testing
      },
      {
        path: 'hr',
        children: [
          {
            path: 'employees',
            loadComponent: loadHrEmployeesComponent
            // Removed permission guard for testing
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
