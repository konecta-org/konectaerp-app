import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { HrApiService, DepartmentDto } from '../core/services/hr-api.service';

@Component({
  selector: 'app-hr-departments',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './hr-departments.component.html',
  styleUrls: ['./hr-departments.component.scss']
})
export class HrDepartmentsComponent implements OnInit {
  private readonly hrApi = inject(HrApiService);

  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly departments = signal<DepartmentDto[]>([]);

  ngOnInit(): void {
    this.loadDepartments();
  }

  reload(): void {
    this.loadDepartments();
  }

  trackByDepartment(_: number, department: DepartmentDto): string {
    return department.departmentId;
  }

  private loadDepartments(): void {
    this.loading.set(true);
    this.error.set(null);

    this.hrApi.getDepartments().subscribe({
      next: data => {
        this.departments.set(data ?? []);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Unable to load department data.');
        this.loading.set(false);
      }
    });
  }
}
