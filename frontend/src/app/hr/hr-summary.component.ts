import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { HrApiService, HrSummaryDto } from '../core/services/hr-api.service';

@Component({
  selector: 'app-hr-summary',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './hr-summary.component.html',
  styleUrls: ['./hr-summary.component.scss']
})
export class HrSummaryComponent implements OnInit {
  private readonly hrApi = inject(HrApiService);

  readonly loading = signal(false);
  readonly summary = signal<HrSummaryDto | null>(null);
  readonly error = signal<string | null>(null);

  ngOnInit(): void {
    this.loadSummary();
  }

  reload(): void {
    this.loadSummary();
  }

  private loadSummary(): void {
    this.loading.set(true);
    this.error.set(null);

    this.hrApi.getSummary().subscribe({
      next: summary => {
        this.summary.set(summary);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Unable to load HR summary data.');
        this.loading.set(false);
      }
    });
  }
}
