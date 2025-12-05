import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FinanceApiService, FinanceSummaryDto } from '../core/services/finance-api.service';

@Component({
  selector: 'app-finance-summary',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './finance-summary.component.html',
  styleUrls: ['./finance-summary.component.scss']
})
export class FinanceSummaryComponent implements OnInit {
  private readonly financeApi = inject(FinanceApiService);

  readonly loading = signal(false);
  readonly summary = signal<FinanceSummaryDto | null>(null);
  readonly error = signal<string | null>(null);

  ngOnInit(): void {
    this.loadSummary();
  }

  reload(): void {
    this.loadSummary();
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(value);
  }

  formatPercent(value: number): string {
    return `${(value * 100).toFixed(1)}%`;
  }

  private loadSummary(): void {
    this.loading.set(true);
    this.error.set(null);

    this.financeApi.getSummary().subscribe({
      next: summary => {
        this.summary.set(summary);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Unable to load finance summary data.');
        this.loading.set(false);
      }
    });
  }
}
