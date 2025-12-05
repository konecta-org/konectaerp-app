import { CommonModule } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../core/services/auth.service';
import { HrApiService, HrSummaryDto } from '../core/services/hr-api.service';

interface HrFeatureTile {
  readonly key: string;
  readonly title: string;
  readonly description: string;
  readonly route: string;
  readonly icon: string;
  readonly accent: string;
  readonly permission?: string;
  readonly permissionPrefix?: string;
}

@Component({
  selector: 'app-hr-workspace',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './hr-workspace.component.html',
  styleUrls: ['./hr-workspace.component.scss']
})
export class HrWorkspaceComponent implements OnInit {
  private readonly auth = inject(AuthService);
  private readonly hrApi = inject(HrApiService);
  private readonly router = inject(Router);

  readonly loading = signal(false);
  readonly summary = signal<HrSummaryDto | null>(null);
  readonly loadError = signal<string | null>(null);

  readonly tiles: HrFeatureTile[] = [
    {
      key: 'summary',
      title: 'HR Overview',
      description: 'Headcount, active staff, departments, and pending resignations.',
      route: '/hr/summary',
      icon: 'OV',
      accent: 'linear-gradient(135deg, #0055b3, #1d4ed8)',
      permission: 'hr.summary.view',
      permissionPrefix: 'hr'
    },
    {
      key: 'employees',
      title: 'Employees',
      description: 'Directory of employees and their current assignments.',
      route: '/hr/employees',
      icon: 'EM',
      accent: 'linear-gradient(135deg, #22c55e, #16a34a)',
      permission: 'hr.employees.read'
    },
    {
      key: 'attendance',
      title: 'Attendance',
      description: 'Check-ins, check-outs, and daily presence tracking.',
      route: '/hr/attendance',
      icon: 'AT',
      accent: 'linear-gradient(135deg, #f97316, #ea580c)',
      permission: 'hr.attendance.read'
    },
    {
      key: 'leaves',
      title: 'Leave Requests',
      description: 'Pending and historical leave applications from staff.',
      route: '/hr/leaves',
      icon: 'LV',
      accent: 'linear-gradient(135deg, #14b8a6, #0f766e)',
      permission: 'hr.leaves.read'
    },
    {
      key: 'departments',
      title: 'Departments',
      description: 'Org structure, managers, and assigned teammates.',
      route: '/hr/departments',
      icon: 'DP',
      accent: 'linear-gradient(135deg, #8b5cf6, #7c3aed)',
      permission: 'hr.departments.read'
    },
    {
      key: 'job-openings',
      title: 'Job Openings',
      description: 'Open requisitions and hiring progress.',
      route: '/hr/job-openings',
      icon: 'JO',
      accent: 'linear-gradient(135deg, #2563eb, #1e40af)',
      permission: 'hr.job-openings.read'
    },
    {
      key: 'job-applications',
      title: 'Applications',
      description: 'Candidate submissions and screening outcomes.',
      route: '/hr/job-applications',
      icon: 'JA',
      accent: 'linear-gradient(135deg, #64748b, #475569)',
      permission: 'hr.job-applications.read'
    },
    {
      key: 'interviews',
      title: 'Interviews',
      description: 'Scheduled interviews with their status and stakeholders.',
      route: '/hr/interviews',
      icon: 'IN',
      accent: 'linear-gradient(135deg, #facc15, #eab308)',
      permission: 'hr.interviews.read'
    },
    {
      key: 'resignations',
      title: 'Resignations',
      description: 'Pending resignation reviews and decisions.',
      route: '/hr/resignations',
      icon: 'RS',
      accent: 'linear-gradient(135deg, #ef4444, #b91c1c)',
      permission: 'hr.resignations.read'
    }
  ];

  ngOnInit(): void {
    this.loadSummary();
  }

  canAccess(tile: HrFeatureTile): boolean {
    const permissionAllowed = tile.permission ? this.auth.hasPermission(tile.permission) : true;
    const prefixAllowed = tile.permissionPrefix ? this.auth.hasPermissionPrefix(tile.permissionPrefix) : true;
    return permissionAllowed || prefixAllowed;
  }

  isLocked(tile: HrFeatureTile): boolean {
    return !this.canAccess(tile);
  }

  navigate(tile: HrFeatureTile): void {
    if (!this.canAccess(tile)) {
      return;
    }

    void this.router.navigate([tile.route]);
  }

  private loadSummary(): void {
    if (!this.auth.hasPermission('hr.summary.view') && !this.auth.hasPermissionPrefix('hr')) {
      return;
    }

    this.loading.set(true);
    this.loadError.set(null);
    this.hrApi.getSummary().subscribe({
      next: summary => {
        this.summary.set(summary);
        this.loading.set(false);
      },
      error: () => {
        this.loadError.set('Unable to load HR summary data.');
        this.loading.set(false);
      }
    });
  }
}
