import { CommonModule, DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { HrApiService, ResignationRequestDto } from '../core/services/hr-api.service';

@Component({
  selector: 'app-hr-resignations',
  standalone: true,
  imports: [CommonModule, DatePipe],
  templateUrl: './hr-resignations.component.html',
  styleUrls: ['./hr-resignations.component.scss']
})
export class HrResignationsComponent implements OnInit {
  private readonly hrApi = inject(HrApiService);

  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly resignations = signal<ResignationRequestDto[]>([]);
  readonly statusFilter = signal<string>('');

  ngOnInit(): void {
    this.loadResignations();
  }

  setStatusFilter(status: string): void {
    this.statusFilter.set(status);
    this.loadResignations();
  }

  reload(): void {
    this.loadResignations();
  }

  trackById(_: number, resignation: ResignationRequestDto): string {
    return resignation.id;
  }

  private loadResignations(): void {
    this.loading.set(true);
    this.error.set(null);

    const status = this.statusFilter();
    this.hrApi.getResignations({ status: status || undefined }).subscribe({
      next: data => {
        this.resignations.set(data ?? []);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Unable to load resignation requests.');
        this.loading.set(false);
      }
    });
  }
}
