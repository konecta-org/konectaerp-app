import { CommonModule } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../core/services/auth.service';
import { FinanceApiService, FinanceSummaryDto } from '../core/services/finance-api.service';

interface FinanceFeatureTile {
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
  selector: 'app-finance-workspace',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './finance-workspace.component.html',
  styleUrls: ['./finance-workspace.component.scss']
})
export class FinanceWorkspaceComponent implements OnInit {
  private readonly auth = inject(AuthService);
  private readonly financeApi = inject(FinanceApiService);
  private readonly router = inject(Router);

  readonly loading = signal(false);
  readonly summary = signal<FinanceSummaryDto | null>(null);
  readonly loadError = signal<string | null>(null);

  readonly tiles: FinanceFeatureTile[] = [
    {
      key: 'budgets',
      title: 'Budgets',
      description: 'Fiscal year budgets, allocations, and spending tracking.',
      route: '/finance/budgets',
      icon: 'BU',
      accent: 'linear-gradient(135deg, #22c55e, #16a34a)',
      permission: 'finance.budgets.read'
    },
    {
      key: 'expenses',
      title: 'Expenses',
      description: 'Track and manage company expenses and reimbursements.',
      route: '/finance/expenses',
      icon: 'EX',
      accent: 'linear-gradient(135deg, #f97316, #ea580c)',
      permission: 'finance.expenses.read'
    },
    {
      key: 'invoices',
      title: 'Invoices',
      description: 'Customer invoices, payments, and receivables management.',
      route: '/finance/invoices',
      icon: 'IN',
      accent: 'linear-gradient(135deg, #8b5cf6, #7c3aed)',
      permission: 'finance.invoices.read'
    },
    {
      key: 'payroll',
      title: 'Payroll Runs',
      description: 'Employee payroll processing and payment history.',
      route: '/finance/payroll',
      icon: 'PR',
      accent: 'linear-gradient(135deg, #14b8a6, #0f766e)',
      permission: 'finance.payroll.read'
    }
  ];

  ngOnInit(): void {
    this.loadSummary();
  }

  canAccess(tile: FinanceFeatureTile): boolean {
    // TODO: Re-enable permission checks for production
    // const permissionAllowed = tile.permission ? this.auth.hasPermission(tile.permission) : true;
    // const prefixAllowed = tile.permissionPrefix ? this.auth.hasPermissionPrefix(tile.permissionPrefix) : true;
    // return permissionAllowed || prefixAllowed;
    return true; // Development: allow all access
  }

  isLocked(tile: FinanceFeatureTile): boolean {
    return false; // Development: nothing is locked
  }

  navigate(tile: FinanceFeatureTile): void {
    void this.router.navigate([tile.route]);
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(value);
  }

  formatPercent(value: number): string {
    return `${(value * 100).toFixed(1)}%`;
  }

  private loadSummary(): void {
    // TODO: Re-enable permission checks for production
    // if (!this.auth.hasPermission('finance.summary.view') && !this.auth.hasPermissionPrefix('finance')) {
    //   return;
    // }

    this.loading.set(true);
    this.loadError.set(null);
    this.financeApi.getSummary().subscribe({
      next: summary => {
        this.summary.set(summary);
        this.loading.set(false);
      },
      error: () => {
        this.loadError.set('Unable to load finance summary data.');
        this.loading.set(false);
      }
    });
  }
}
