import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { DashboardApiService, DashboardSummary } from './services/dashboard-api.service';
import { StatCardComponent } from '../shared/components/stat-card/stat-card.component';
import { LoadingSpinnerComponent } from '../shared/components/loading-spinner/loading-spinner.component';
import { NotificationService } from '../shared/services/notification.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink, StatCardComponent, LoadingSpinnerComponent],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss'
})
export class Dashboard implements OnInit {
  loading = signal(false);
  summary = signal<DashboardSummary | null>(null);

  constructor(
    private dashboardApi: DashboardApiService,
    private notification: NotificationService
  ) {}

  ngOnInit(): void {
    this.loadDashboard();
  }

  loadDashboard(): void {
    this.loading.set(true);
    this.dashboardApi.getDashboardSummary().subscribe({
      next: (data) => {
        this.summary.set(data);
        this.loading.set(false);
      },
      error: (error) => {
        console.error('Failed to load dashboard:', error);
        this.notification.error('Failed to load dashboard data');
        this.loading.set(false);
      }
    });
  }

  refresh(): void {
    this.loadDashboard();
    this.notification.info('Dashboard refreshed');
  }
}
