import { CommonModule, DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { HrApiService, JobApplicationDto } from '../core/services/hr-api.service';

@Component({
  selector: 'app-hr-job-applications',
  standalone: true,
  imports: [CommonModule, DatePipe],
  templateUrl: './hr-job-applications.component.html',
  styleUrls: ['./hr-job-applications.component.scss']
})
export class HrJobApplicationsComponent implements OnInit {
  private readonly hrApi = inject(HrApiService);

  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly applications = signal<JobApplicationDto[]>([]);

  ngOnInit(): void {
    this.loadApplications();
  }

  reload(): void {
    this.loadApplications();
  }

  trackByApplication(_: number, application: JobApplicationDto): string {
    return application.id;
  }

  private loadApplications(): void {
    this.loading.set(true);
    this.error.set(null);

    this.hrApi.getJobApplications().subscribe({
      next: data => {
        this.applications.set(data ?? []);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Unable to load job applications.');
        this.loading.set(false);
      }
    });
  }
}
