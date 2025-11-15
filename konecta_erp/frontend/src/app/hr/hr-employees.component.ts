import { CommonModule } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { HrApiService, EmployeeDto } from '../core/services/hr-api.service';

@Component({
  selector: 'app-hr-employees',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './hr-employees.component.html',
  styleUrl: './hr-employees.component.scss'
})
export class HrEmployeesComponent implements OnInit {
  readonly employees = signal<EmployeeDto[]>([]);
  readonly loading = signal(false);

  constructor(private readonly hrApi: HrApiService) {}

  ngOnInit(): void {
    this.refresh();
  }

  refresh(): void {
    this.loading.set(true);
    this.hrApi.getEmployees().subscribe({
      next: employees => this.employees.set(employees ?? []),
      error: () => this.loading.set(false),
      complete: () => this.loading.set(false)
    });
  }

  trackByEmployee(_: number, employee: EmployeeDto): string {
    return employee.id;
  }
}
