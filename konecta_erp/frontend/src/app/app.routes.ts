import { Routes } from '@angular/router';
import { MainLayoutComponent } from './core/layout/main-layout.component';
import { authGuard } from './core/guards/auth.guard';
import { permissionGuard } from './core/guards/permission.guard';

const loadLoginComponent = () => import('./auth/login/login.component').then(m => m.LoginComponent);
const loadRegisterComponent = () => import('./auth/register/register.component').then(m => m.RegisterComponent);
const loadLandingComponent = () => import('./landing/landing.component').then(m => m.LandingComponent);
const loadHrWorkspaceComponent = () => import('./hr/hr-workspace.component').then(m => m.HrWorkspaceComponent);
const loadHrSummaryComponent = () => import('./hr/hr-summary.component').then(m => m.HrSummaryComponent);
const loadUserManagementComponent = () => import('./usermanagement/user-management.component').then(m => m.UserManagementComponent);
const loadHrEmployeesComponent = () => import('./hr/hr-employees.component').then(m => m.HrEmployeesComponent);
const loadHrAttendanceComponent = () => import('./hr/hr-attendance.component').then(m => m.HrAttendanceComponent);
const loadHrLeavesComponent = () => import('./hr/hr-leaves.component').then(m => m.HrLeavesComponent);
const loadHrDepartmentsComponent = () => import('./hr/hr-departments.component').then(m => m.HrDepartmentsComponent);
const loadHrJobOpeningsComponent = () => import('./hr/hr-job-openings.component').then(m => m.HrJobOpeningsComponent);
const loadHrJobApplicationsComponent = () => import('./hr/hr-job-applications.component').then(m => m.HrJobApplicationsComponent);
const loadHrInterviewsComponent = () => import('./hr/hr-interviews.component').then(m => m.HrInterviewsComponent);
const loadHrResignationsComponent = () => import('./hr/hr-resignations.component').then(m => m.HrResignationsComponent);
const loadFinanceComponent = () => import('./finance/finance').then(m => m.Finance);
const loadInventoryComponent = () => import('./inventory/inventory').then(m => m.Inventory);
const loadReportingComponent = () => import('./reporting/reporting').then(m => m.Reporting);
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
      { path: '', pathMatch: 'full', redirectTo: 'landing' },
      { path: 'landing', loadComponent: loadLandingComponent },
      {
        path: 'users',
        loadComponent: loadUserManagementComponent,
        canActivate: [permissionGuard],
        data: { permission: 'user-management.users.read' }
      },
      {
        path: 'hr',
        canActivate: [permissionGuard],
        data: { permissionPrefix: 'hr' },
        children: [
          { path: '', loadComponent: loadHrWorkspaceComponent },
          {
            path: 'summary',
            loadComponent: loadHrSummaryComponent,
            canActivate: [permissionGuard],
            data: { permission: 'hr.summary.view', permissionPrefix: 'hr' }
          },
          {
            path: 'employees',
            loadComponent: loadHrEmployeesComponent,
            canActivate: [permissionGuard],
            data: { permission: 'hr.employees.read', permissionPrefix: 'hr' }
          },
          {
            path: 'attendance',
            loadComponent: loadHrAttendanceComponent,
            canActivate: [permissionGuard],
            data: { permission: 'hr.attendance.read', permissionPrefix: 'hr' }
          },
          {
            path: 'leaves',
            loadComponent: loadHrLeavesComponent,
            canActivate: [permissionGuard],
            data: { permission: 'hr.leaves.read', permissionPrefix: 'hr' }
          },
          {
            path: 'departments',
            loadComponent: loadHrDepartmentsComponent,
            canActivate: [permissionGuard],
            data: { permission: 'hr.departments.read', permissionPrefix: 'hr' }
          },
          {
            path: 'job-openings',
            loadComponent: loadHrJobOpeningsComponent,
            canActivate: [permissionGuard],
            data: { permission: 'hr.job-openings.read', permissionPrefix: 'hr' }
          },
          {
            path: 'job-applications',
            loadComponent: loadHrJobApplicationsComponent,
            canActivate: [permissionGuard],
            data: { permission: 'hr.job-applications.read', permissionPrefix: 'hr' }
          },
          {
            path: 'interviews',
            loadComponent: loadHrInterviewsComponent,
            canActivate: [permissionGuard],
            data: { permission: 'hr.interviews.read', permissionPrefix: 'hr' }
          },
          {
            path: 'resignations',
            loadComponent: loadHrResignationsComponent,
            canActivate: [permissionGuard],
            data: { permission: 'hr.resignations.read', permissionPrefix: 'hr' }
          }
        ]
      },
      { path: 'finance', loadComponent: loadFinanceComponent },
      { path: 'inventory', loadComponent: loadInventoryComponent },
      { path: 'reporting', loadComponent: loadReportingComponent },
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
