import { CommonModule, DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { HrApiService, InterviewDto } from '../core/services/hr-api.service';

@Component({
  selector: 'app-hr-interviews',
  standalone: true,
  imports: [CommonModule, DatePipe],
  templateUrl: './hr-interviews.component.html',
  styleUrls: ['./hr-interviews.component.scss']
})
export class HrInterviewsComponent implements OnInit {
  private readonly hrApi = inject(HrApiService);

  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly interviews = signal<InterviewDto[]>([]);

  ngOnInit(): void {
    this.loadInterviews();
  }

  reload(): void {
    this.loadInterviews();
  }

  trackByInterview(_: number, interview: InterviewDto): string {
    return interview.id;
  }

  private loadInterviews(): void {
    this.loading.set(true);
    this.error.set(null);

    this.hrApi.getInterviews().subscribe({
      next: data => {
        this.interviews.set(data ?? []);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Unable to load interviews.');
        this.loading.set(false);
      }
    });
  }
}
