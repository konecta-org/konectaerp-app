import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { HrApiService, AttendanceRecordDto } from '../core/services/hr-api.service';

@Component({
  selector: 'app-hr-attendance',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './hr-attendance.component.html',
  styleUrls: ['./hr-attendance.component.scss']
})
export class HrAttendanceComponent implements OnInit {
  private readonly hrApi = inject(HrApiService);

  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly records = signal<AttendanceRecordDto[]>([]);

  ngOnInit(): void {
    this.loadAttendance();
  }

  reload(): void {
    this.loadAttendance();
  }

  trackById(_: number, record: AttendanceRecordDto): string {
    return record.id;
  }

  private loadAttendance(): void {
    this.loading.set(true);
    this.error.set(null);

    this.hrApi.getAttendance().subscribe({
      next: data => {
        this.records.set(data ?? []);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Unable to load attendance records.');
        this.loading.set(false);
      }
    });
  }
}
