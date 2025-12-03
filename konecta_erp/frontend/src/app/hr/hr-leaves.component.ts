import { CommonModule, DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { HrApiService, LeaveRequestDto } from '../core/services/hr-api.service';

@Component({
  selector: 'app-hr-leaves',
  standalone: true,
  imports: [CommonModule, DatePipe],
  templateUrl: './hr-leaves.component.html',
  styleUrls: ['./hr-leaves.component.scss']
})
export class HrLeavesComponent implements OnInit {
  private readonly hrApi = inject(HrApiService);

  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly leaves = signal<LeaveRequestDto[]>([]);
  readonly pendingOnly = signal(false);

  ngOnInit(): void {
    this.loadLeaves();
  }

  togglePending(): void {
    this.pendingOnly.update(value => !value);
    this.loadLeaves();
  }

  reload(): void {
    this.loadLeaves();
  }

  trackById(_: number, leave: LeaveRequestDto): string {
    return leave.id;
  }

  private loadLeaves(): void {
    this.loading.set(true);
    this.error.set(null);

    this.hrApi.getLeaveRequests({ pendingOnly: this.pendingOnly() }).subscribe({
      next: data => {
        this.leaves.set(data ?? []);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Unable to load leave requests.');
        this.loading.set(false);
      }
    });
  }
}
