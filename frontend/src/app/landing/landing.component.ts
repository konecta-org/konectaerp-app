import { CommonModule } from '@angular/common';
import { Component, computed, inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../core/services/auth.service';

interface WorkspaceTile {
  readonly key: string;
  readonly title: string;
  readonly description: string;
  readonly route: string;
  readonly icon: string;
  readonly accent: string;
  readonly permissions?: string[];
  readonly roles?: string[];
  readonly permissionPrefixes?: string[];
}

@Component({
  selector: 'app-landing',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './landing.component.html',
  styleUrl: './landing.component.scss'
})
export class LandingComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly session = computed(() => this.auth.currentSession());

  readonly tiles: WorkspaceTile[] = [
    {
      key: 'hr',
      title: 'Human Resources',
      description: 'Manage employee profiles, onboarding, and organizational structure.',
      route: '/hr',
      icon: 'HR',
      accent: 'linear-gradient(135deg, #0055b3, #2563eb)',
      permissions: ['hr.employees.read'],
      roles: ['HrAdmin', 'HrStaff'],
      permissionPrefixes: ['hr']
    },
    {
      key: 'finance',
      title: 'Finance',
      description: 'Track budgets, expenses, and financial approvals across the organization.',
      route: '/finance',
      icon: 'FN',
      accent: 'linear-gradient(135deg, #ffc94a, #f97316)'
    },
    {
      key: 'inventory',
      title: 'Inventory',
      description: 'Monitor stock levels, supplier activity, and fulfillment workflows.',
      route: '/inventory',
      icon: 'IN',
      accent: 'linear-gradient(135deg, #22c55e, #16a34a)',
      roles: ['DepartmentManager']
    },
    {
      key: 'reporting',
      title: 'Reporting & Analytics',
      description: 'Explore performance dashboards and exported analytics reports.',
      route: '/reporting',
      icon: 'RP',
      accent: 'linear-gradient(135deg, #7c3aed, #4c1d95)',
      roles: ['ReportingManager', 'ReportingAnalyst', 'ReportingStaff'],
      permissionPrefixes: ['reporting']
    },
    {
      key: 'users',
      title: 'User Management',
      description: 'Administer user accounts, roles, and permissions across teams.',
      route: '/users',
      icon: 'UM',
      accent: 'linear-gradient(135deg, #0ea5e9, #0369a1)',
      permissions: ['user-management.users.read']
    }
  ];

  canAccess(tile: WorkspaceTile): boolean {
    if (this.auth.hasRole('SystemAdmin')) {
      return true;
    }

    const hasAnyPermission = tile.permissions?.some(permission => this.auth.hasPermission(permission)) ?? false;
    const hasAnyRole = tile.roles?.some(role => this.auth.hasRole(role)) ?? false;
    const hasAnyPermissionPrefix = tile.permissionPrefixes?.some(prefix => this.auth.hasPermissionPrefix(prefix)) ?? false;

    if (!tile.permissions && !tile.roles && !tile.permissionPrefixes) {
      return true;
    }

    return hasAnyPermission || hasAnyRole || hasAnyPermissionPrefix;
  }

  navigate(tile: WorkspaceTile): void {
    if (!this.canAccess(tile)) {
      return;
    }

    void this.router.navigate([tile.route]);
  }
}
