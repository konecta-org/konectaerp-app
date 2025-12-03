import { CommonModule, DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { HrApiService, JobOpeningDto } from '../core/services/hr-api.service';

@Component({
  selector: 'app-hr-job-openings',
  standalone: true,
  imports: [CommonModule, DatePipe],
  templateUrl: './hr-job-openings.component.html',
  styleUrls: ['./hr-job-openings.component.scss']
})
export class HrJobOpeningsComponent implements OnInit {
  private readonly hrApi = inject(HrApiService);

  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly openings = signal<JobOpeningDto[]>([]);

  ngOnInit(): void {
    this.loadOpenings();
  }

  reload(): void {
    this.loadOpenings();
  }

  trackByOpening(_: number, opening: JobOpeningDto): string {
    return opening.id;
  }

  private loadOpenings(): void {
    this.loading.set(true);
    this.error.set(null);

    this.hrApi.getJobOpenings({ includeApplications: true }).subscribe({
      next: data => {
        this.openings.set(data ?? []);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Unable to load job openings.');
        this.loading.set(false);
      }
    });
  }
}
