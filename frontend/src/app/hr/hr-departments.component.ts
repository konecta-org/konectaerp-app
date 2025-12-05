import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, effect, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../core/services/auth.service';
import {
  AssignDepartmentManagerRequest,
  CreateDepartmentRequest,
  DepartmentDto,
  HrApiService,
  UpdateDepartmentRequest
} from '../core/services/hr-api.service';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { TooltipModule } from 'primeng/tooltip';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';
import { DividerModule } from 'primeng/divider';
import { MessageModule } from 'primeng/message';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { InputTextarea } from 'primeng/inputtextarea';
import { DropdownModule } from 'primeng/dropdown';

type DepartmentFormMode = 'create' | 'edit';

@Component({
  selector: 'app-hr-departments',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TableModule,
    ButtonModule,
    TooltipModule,
    CardModule,
    TagModule,
    DividerModule,
    MessageModule,
    DialogModule,
    InputTextModule,
    InputTextarea,
    DropdownModule
  ],
  templateUrl: './hr-departments.component.html',
  styleUrls: ['./hr-departments.component.scss']
})
export class HrDepartmentsComponent implements OnInit {
  private readonly hrApi = inject(HrApiService);
  private readonly auth = inject(AuthService);
  private readonly fb = inject(FormBuilder);

  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly departments = signal<DepartmentDto[]>([]);
  readonly selectedDepartment = signal<DepartmentDto | null>(null);
  readonly feedback = signal<string | null>(null);

  readonly formVisible = signal(false);
  readonly formMode = signal<DepartmentFormMode>('create');
  readonly formSubmitting = signal(false);

  readonly managerDialogVisible = signal(false);
  readonly managerSubmitting = signal(false);

  readonly hasDepartments = computed(() => this.departments().length > 0);
  readonly detailVisible = computed(() => this.selectedDepartment() !== null);

  readonly managerOptions = computed(() => this.selectedDepartment()?.employees ?? []);

  private readonly managePermissions = ['hr.departments.manage'];
  private readonly assignManagerPermissions = ['hr.departments.manage', 'hr.departments.manager.assign'];

  readonly managePermissionLabel = this.formatPermissionList(this.managePermissions);
  readonly assignPermissionLabel = this.formatPermissionList(this.assignManagerPermissions);

  readonly canManageDepartments = computed(() => this.hasAnyPermission(this.managePermissions));
  readonly canAssignManagers = computed(() => this.hasAnyPermission(this.assignManagerPermissions));

  readonly departmentForm = this.fb.group({
    departmentName: this.fb.control('', {
      validators: [Validators.required, Validators.maxLength(100)],
      nonNullable: true
    }),
    description: this.fb.control<string | null>(null, [Validators.maxLength(500)])
  });

  readonly managerForm = this.fb.group({
    employeeId: this.fb.control<string | null>(null, Validators.required)
  });

  ngOnInit(): void {
    this.loadDepartments();

    effect(() => {
      const list = this.departments();
      const current = this.selectedDepartment();

      if (!list.length) {
        this.selectedDepartment.set(null);
        return;
      }

      if (!current) {
        this.selectedDepartment.set(list[0]);
        return;
      }

      const stillExists = list.find(dept => dept.departmentId === current.departmentId);
      if (stillExists && stillExists !== current) {
        this.selectedDepartment.set(stillExists);
      }
    });
  }

  reload(): void {
    this.loadDepartments();
  }

  trackByDepartment(_: number, department: DepartmentDto): string {
    return department.departmentId;
  }

  isSelected(department: DepartmentDto): boolean {
    return this.selectedDepartment()?.departmentId === department.departmentId;
  }

  viewDepartment(department: DepartmentDto): void {
    this.selectedDepartment.set(department);
  }

  closeDetails(): void {
    this.selectedDepartment.set(null);
  }

  employeeStatusSeverity(status?: string): 'success' | 'warning' | 'danger' | 'secondary' {
    if (!status) {
      return 'secondary';
    }

    const normalized = status.trim().toLowerCase();
    switch (normalized) {
      case 'active':
        return 'success';
      case 'onleave':
      case 'on leave':
      case 'pending':
        return 'warning';
      case 'terminated':
      case 'resigned':
      case 'inactive':
        return 'danger';
      default:
        return 'secondary';
    }
  }

  tooltipFor(action: 'manage' | 'assign', locked: boolean): string {
    if (!locked) {
      return action === 'manage' ? 'Edit department' : 'Assign manager';
    }

    const map: Record<typeof action, string> = {
      manage: this.managePermissionLabel,
      assign: this.assignPermissionLabel
    };

    return `Requires ${map[action]}`;
  }

  actionLockClass(locked: boolean): Record<'is-locked', boolean> {
    return { 'is-locked': locked };
  }

  clearFeedback(): void {
    this.feedback.set(null);
  }

  departmentControlInvalid(controlName: 'departmentName' | 'description'): boolean {
    const control = this.departmentForm.controls[controlName];
    return control.invalid && control.touched;
  }

  managerControlInvalid(controlName: 'employeeId'): boolean {
    const control = this.managerForm.controls[controlName];
    return control.invalid && control.touched;
  }

  openCreateDepartment(): void {
    if (!this.canManageDepartments()) {
      return;
    }

    this.formMode.set('create');
    this.departmentForm.reset({ departmentName: '', description: null });
    this.formVisible.set(true);
  }

  openEditDepartment(department: DepartmentDto, event?: Event): void {
    event?.stopPropagation();

    if (!this.canManageDepartments()) {
      return;
    }

    this.selectedDepartment.set(department);
    this.formMode.set('edit');
    this.departmentForm.reset({
      departmentName: department.departmentName,
      description: department.description ?? null
    });
    this.formVisible.set(true);
  }

  closeDepartmentForm(): void {
    if (this.formSubmitting()) {
      return;
    }
    this.formVisible.set(false);
  }

  submitDepartmentForm(): void {
    if (this.departmentForm.invalid) {
      this.departmentForm.markAllAsTouched();
      return;
    }

    const { departmentName, description } = this.departmentForm.getRawValue();
    if (!departmentName) {
      return;
    }

    const trimmedName = departmentName.trim();
    const trimmedDescription = description?.trim() || null;

    this.formSubmitting.set(true);

    if (this.formMode() === 'create') {
      const payload: CreateDepartmentRequest = {
        departmentName: trimmedName,
        description: trimmedDescription || undefined
      };

      this.hrApi.createDepartment(payload).subscribe({
        next: department => {
          this.departments.update(list => this.sortDepartments([...list, department]));
          this.selectedDepartment.set(department);
          this.setFeedback('Department created successfully.');
          this.formSubmitting.set(false);
          this.formVisible.set(false);
        },
        error: () => {
          this.formSubmitting.set(false);
          this.error.set('Unable to create department. Please try again.');
        }
      });
      return;
    }

    const current = this.selectedDepartment();
    if (!current) {
      this.formSubmitting.set(false);
      return;
    }

    const payload: UpdateDepartmentRequest = {
      departmentId: current.departmentId,
      departmentName: trimmedName,
      description: trimmedDescription ?? undefined
    };

    this.hrApi.updateDepartment(current.departmentId, payload).subscribe({
      next: () => {
        const updated: DepartmentDto = {
          ...current,
          departmentName: payload.departmentName,
          description: payload.description ?? undefined
        };
        this.updateDepartmentInList(updated);
        this.selectedDepartment.set(updated);
        this.setFeedback('Department updated successfully.');
        this.formSubmitting.set(false);
        this.formVisible.set(false);
      },
      error: () => {
        this.formSubmitting.set(false);
        this.error.set('Unable to update department. Please try again.');
      }
    });
  }

  openAssignManager(department: DepartmentDto, event?: Event): void {
    event?.stopPropagation();

    if (!this.canAssignManagers()) {
      return;
    }

    if (!department.employees?.length) {
      this.error.set('Assigning a manager requires department members.');
      return;
    }

    this.selectedDepartment.set(department);
    this.managerForm.reset({ employeeId: department.managerId ?? null });
    this.managerDialogVisible.set(true);
  }

  closeManagerDialog(): void {
    if (this.managerSubmitting()) {
      return;
    }
    this.managerDialogVisible.set(false);
  }

  submitAssignManager(): void {
    if (this.managerForm.invalid) {
      this.managerForm.markAllAsTouched();
      return;
    }

    const department = this.selectedDepartment();
    const employeeId = this.managerForm.value.employeeId;

    if (!department || !employeeId) {
      return;
    }

    const payload: AssignDepartmentManagerRequest = { employeeId };
    this.managerSubmitting.set(true);

    this.hrApi.assignDepartmentManager(department.departmentId, payload).subscribe({
      next: () => {
        const member = department.employees?.find(emp => emp.id === employeeId);
        const updated: DepartmentDto = {
          ...department,
          managerId: employeeId,
          managerFullName: member?.fullName ?? department.managerFullName
        };
        this.updateDepartmentInList(updated);
        this.selectedDepartment.set(updated);
        this.setFeedback('Manager assigned successfully.');
        this.managerSubmitting.set(false);
        this.managerDialogVisible.set(false);
      },
      error: () => {
        this.managerSubmitting.set(false);
        this.error.set('Unable to assign manager. Please try again.');
      }
    });
  }

  private syncSelection(next: DepartmentDto[] | null | undefined): void {
    const list = next ?? [];
    const current = this.selectedDepartment();

    if (!list.length) {
      this.selectedDepartment.set(null);
      return;
    }

    if (!current) {
      this.selectedDepartment.set(list[0]);
      return;
    }

    const updated = list.find(dept => dept.departmentId === current.departmentId);
    this.selectedDepartment.set(updated ?? list[0]);
  }

  private loadDepartments(): void {
    this.loading.set(true);
    this.error.set(null);

    this.hrApi.getDepartments().subscribe({
      next: data => {
        this.departments.set(data ?? []);
        this.syncSelection(data);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Unable to load department data.');
        this.selectedDepartment.set(null);
        this.loading.set(false);
      }
    });
  }

  private setFeedback(message: string): void {
    this.feedback.set(message);
  }

  private updateDepartmentInList(updated: DepartmentDto): void {
    this.departments.update(list =>
      this.sortDepartments(list.map(department => (department.departmentId === updated.departmentId ? updated : department)))
    );
  }

  private sortDepartments(list: DepartmentDto[]): DepartmentDto[] {
    return [...list].sort((a, b) => a.departmentName.localeCompare(b.departmentName));
  }

  private hasAnyPermission(perms: string[]): boolean {
    return perms.some(permission => this.auth.hasPermission(permission) || this.auth.hasPermissionPrefix(permission));
  }

  private formatPermissionList(perms: string[]): string {
    if (perms.length === 1) {
      return perms[0];
    }

    if (perms.length === 2) {
      return `${perms[0]} or ${perms[1]}`;
    }

    return `${perms.slice(0, -1).join(', ')}, or ${perms.at(-1)}`;
  }
}
