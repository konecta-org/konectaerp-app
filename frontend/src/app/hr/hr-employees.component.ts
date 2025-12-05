import { CommonModule, DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnDestroy, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../core/services/auth.service';
import {
  AddEmployeeRequest,
  DepartmentDto,
  EmployeeBonusItemRequest,
  EmployeeDeductionItemRequest,
  EmployeeDto,
  EmploymentStatus,
  FireEmployeeRequest,
  HrApiService,
  IssueEmployeeBonusesRequest,
  IssueEmployeeDeductionsRequest,
  UpdateEmployeeRequest
} from '../core/services/hr-api.service';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { TooltipModule } from 'primeng/tooltip';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { DropdownModule } from 'primeng/dropdown';
import { CalendarModule } from 'primeng/calendar';
import { CheckboxModule } from 'primeng/checkbox';
import { InputTextarea } from 'primeng/inputtextarea';
import { TagModule } from 'primeng/tag';
import { DividerModule } from 'primeng/divider';
import { MessageModule } from 'primeng/message';
import { CardModule } from 'primeng/card';

type FormMode = 'create' | 'edit';

@Component({
  selector: 'app-hr-employees',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    DatePipe,
    TableModule,
    ButtonModule,
    TooltipModule,
    DialogModule,
    InputTextModule,
    InputNumberModule,
    DropdownModule,
    CalendarModule,
    CheckboxModule,
    InputTextarea,
    TagModule,
    DividerModule,
    MessageModule,
    CardModule
  ],
  templateUrl: './hr-employees.component.html',
  styleUrls: ['./hr-employees.component.scss']
})
export class HrEmployeesComponent implements OnInit, OnDestroy {
  private readonly hrApi = inject(HrApiService);
  private readonly auth = inject(AuthService);
  private readonly fb = inject(FormBuilder);

  private feedbackTimeout: ReturnType<typeof setTimeout> | null = null;

  readonly employees = signal<EmployeeDto[]>([]);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly departments = signal<DepartmentDto[]>([]);
  readonly departmentsLoading = signal(false);
  readonly departmentsLoaded = signal(false);
  readonly selectedEmployee = signal<EmployeeDto | null>(null);
  readonly feedback = signal<string | null>(null);

  readonly formVisible = signal(false);
  readonly formMode = signal<FormMode>('create');
  readonly formSubmitting = signal(false);

  readonly fireDialogVisible = signal(false);
  readonly fireSubmitting = signal(false);

  readonly bonusDialogVisible = signal(false);
  readonly bonusSubmitting = signal(false);

  readonly deductionDialogVisible = signal(false);
  readonly deductionSubmitting = signal(false);

  readonly employmentStatusOptions = [
    { value: EmploymentStatus.Active, label: 'Active' },
    { value: EmploymentStatus.OnLeave, label: 'On Leave' },
    { value: EmploymentStatus.Resigned, label: 'Resigned' },
    { value: EmploymentStatus.Terminated, label: 'Terminated' }
  ];

  private readonly managePermissions = ['hr.employees.manage'];
  private readonly bonusPermissions = ['hr.employees.manage', 'hr.employees.bonuses.issue'];
  private readonly deductionPermissions = ['hr.employees.manage', 'hr.employees.deductions.issue'];
  private readonly firePermissions = ['hr.employees.manage', 'hr.employees.terminate'];

  readonly managePermissionLabel = this.formatPermissionList(this.managePermissions);
  readonly bonusPermissionLabel = this.formatPermissionList(this.bonusPermissions);
  readonly deductionPermissionLabel = this.formatPermissionList(this.deductionPermissions);
  readonly firePermissionLabel = this.formatPermissionList(this.firePermissions);

  readonly canManageEmployees = computed(() => this.hasAnyPermission(this.managePermissions));
  readonly canIssueBonuses = computed(() => this.hasAnyPermission(this.bonusPermissions));
  readonly canIssueDeductions = computed(() => this.hasAnyPermission(this.deductionPermissions));
  readonly canFireEmployees = computed(() => this.hasAnyPermission(this.firePermissions));

  tooltipFor(action: 'manage' | 'bonus' | 'deduction' | 'fire', locked: boolean): string {
    if (!locked) {
      switch (action) {
        case 'manage':
          return 'Edit employee';
        case 'bonus':
          return 'Issue bonus';
        case 'deduction':
          return 'Issue deduction';
        case 'fire':
          return 'Terminate employment';
      }
    }

    const labelMap: Record<typeof action, string> = {
      manage: this.managePermissionLabel,
      bonus: this.bonusPermissionLabel,
      deduction: this.deductionPermissionLabel,
      fire: this.firePermissionLabel
    };

    return `Requires ${labelMap[action]}`;
  }

  actionLockClass(locked: boolean): Record<'is-locked', boolean> {
    return { 'is-locked': locked };
  }

  readonly departmentOptions = computed(() =>
    this.departments().map(department => ({ label: department.departmentName, value: department.departmentId }))
  );

  readonly employeeForm = this.fb.group({
    id: this.fb.control<string | null>(null),
    fullName: ['', [Validators.required, Validators.maxLength(100)]],
    workEmail: ['', [Validators.required, Validators.email]],
    personalEmail: ['', [Validators.required, Validators.email]],
    position: ['', [Validators.required, Validators.maxLength(100)]],
    phoneNumber: [''],
    salary: [0, [Validators.required, Validators.min(0)]],
    hireDate: this.fb.control<Date | null>(null),
    status: [EmploymentStatus.Active, Validators.required],
    departmentId: ['', Validators.required],
    exitDate: this.fb.control<Date | null>(null),
    exitReason: [''],
    eligibleForRehire: [false],
    userId: ['']
  });

  readonly fireForm = this.fb.group({
    reason: [''],
    eligibleForRehire: [false]
  });

  readonly bonusForm = this.fb.group({
    bonusType: ['General', Validators.required],
    amount: [null as number | null, [Validators.required, Validators.min(0.01)]],
    awardedOn: this.fb.control<Date | null>(null),
    period: [''],
    reference: [''],
    awardedBy: [''],
    notes: [''],
    sourceSystem: [''],
    issuedBy: ['']
  });

  readonly deductionForm = this.fb.group({
    deductionType: ['General', Validators.required],
    amount: [null as number | null, [Validators.required, Validators.min(0.01)]],
    appliedOn: this.fb.control<Date | null>(null),
    period: [''],
    reference: [''],
    appliedBy: [''],
    notes: [''],
    sourceSystem: [''],
    isRecurring: [false],
    issuedBy: ['']
  });

  ngOnInit(): void {
    this.loadEmployees();
  }

  ngOnDestroy(): void {
    if (this.feedbackTimeout) {
      clearTimeout(this.feedbackTimeout);
    }
  }

  refresh(): void {
    this.loadEmployees();
  }

  trackByEmployee(_: number, employee: EmployeeDto): string {
    return employee.id;
  }

  statusLabel(status: EmploymentStatus): string {
    return this.employmentStatusOptions.find(option => option.value === status)?.label ?? 'Unknown';
  }

  statusClass(status: EmploymentStatus): string {
    switch (status) {
      case EmploymentStatus.Active:
        return 'status--active';
      case EmploymentStatus.OnLeave:
        return 'status--leave';
      case EmploymentStatus.Resigned:
        return 'status--resigned';
      case EmploymentStatus.Terminated:
        return 'status--terminated';
      default:
        return 'status--default';
    }
  }

  statusSeverity(status: EmploymentStatus): 'success' | 'info' | 'warning' | 'danger' {
    switch (status) {
      case EmploymentStatus.Active:
        return 'success';
      case EmploymentStatus.OnLeave:
        return 'warning';
      case EmploymentStatus.Resigned:
        return 'info';
      case EmploymentStatus.Terminated:
        return 'danger';
      default:
        return 'info';
    }
  }

  viewDetails(employee: EmployeeDto): void {
    this.selectedEmployee.set(employee);
  }

  closeDetails(): void {
    this.selectedEmployee.set(null);
  }

  openCreateEmployee(): void {
    if (!this.requirePermission(this.managePermissions, 'Creating employees')) {
      return;
    }

    this.formMode.set('create');
    this.employeeForm.reset({
      id: null,
      fullName: '',
      workEmail: '',
      personalEmail: '',
      position: '',
      phoneNumber: '',
      salary: 0,
      hireDate: null,
      status: EmploymentStatus.Active,
      departmentId: '',
      exitDate: null,
      exitReason: '',
      eligibleForRehire: false,
      userId: ''
    });
    this.ensureDepartmentsLoaded();
    this.formVisible.set(true);
  }

  openEditEmployee(employee: EmployeeDto, event?: Event): void {
    event?.stopPropagation();
    if (!this.requirePermission(this.managePermissions, 'Updating employees')) {
      return;
    }

    this.formMode.set('edit');
    this.ensureDepartmentsLoaded();
    this.employeeForm.reset({
      id: employee.id,
      fullName: employee.fullName,
      workEmail: employee.workEmail,
      personalEmail: employee.personalEmail,
      position: employee.position,
      phoneNumber: employee.phoneNumber ?? '',
      salary: employee.salary,
      hireDate: this.toDate(employee.hireDate),
      status: employee.status,
      departmentId: employee.departmentId,
      exitDate: this.toDate(employee.exitDate),
      exitReason: employee.exitReason ?? '',
      eligibleForRehire: employee.eligibleForRehire ?? false,
      userId: employee.userId ?? ''
    });
    this.formVisible.set(true);
  }

  closeEmployeeForm(): void {
    this.formVisible.set(false);
  }

  submitEmployeeForm(): void {
    if (this.employeeForm.invalid) {
      this.employeeForm.markAllAsTouched();
      return;
    }

    const mode = this.formMode();
    const value = this.employeeForm.getRawValue();
    const departmentName = this.departments()
      .find(dept => dept.departmentId === value.departmentId)?.departmentName ?? '';

    if (mode === 'create') {
      const request: AddEmployeeRequest = {
        fullName: value.fullName!,
        workEmail: value.workEmail!,
        personalEmail: value.personalEmail!,
        position: value.position!,
        phoneNumber: value.phoneNumber || undefined,
        salary: Number(value.salary ?? 0),
        hireDate: this.toIsoString(value.hireDate),
        status: value.status ?? EmploymentStatus.Active,
        departmentId: value.departmentId!
      };

      this.formSubmitting.set(true);
      this.hrApi.createEmployee(request).subscribe({
        next: created => {
          this.formSubmitting.set(false);
          this.formVisible.set(false);
          this.flashFeedback('Employee created successfully.');
          this.employees.update(list => this.sortEmployees([...list, created]));
          this.selectedEmployee.set(created);
        },
        error: error => {
          this.formSubmitting.set(false);
          this.handleActionError(error, 'Unable to create employee. Please try again.', this.managePermissions);
        }
      });
      return;
    }

    if (!value.id) {
      return;
    }

    const request: UpdateEmployeeRequest = {
      id: value.id,
      fullName: value.fullName!,
      workEmail: value.workEmail!,
      personalEmail: value.personalEmail!,
      position: value.position!,
      phoneNumber: value.phoneNumber || undefined,
      salary: Number(value.salary ?? 0),
      hireDate: this.toIsoString(value.hireDate),
      status: value.status ?? EmploymentStatus.Active,
      departmentId: value.departmentId!,
      userId: value.userId || undefined,
      exitDate: this.toIsoString(value.exitDate),
      exitReason: value.exitReason || undefined,
      eligibleForRehire: value.eligibleForRehire ?? undefined
    };

    this.formSubmitting.set(true);
    this.hrApi.updateEmployee(request.id, request).subscribe({
      next: () => {
        this.formSubmitting.set(false);
        this.formVisible.set(false);
        this.flashFeedback('Employee updated successfully.');
        this.reloadEmployee(request.id, departmentName);
      },
      error: error => {
        this.formSubmitting.set(false);
        this.handleActionError(error, 'Unable to update employee. Please try again.', this.managePermissions);
      }
    });
  }

  openFireDialog(employee: EmployeeDto, event?: Event): void {
    event?.stopPropagation();
    if (!this.requirePermission(this.firePermissions, 'Terminating employees')) {
      return;
    }

    this.selectedEmployee.set(employee);
    this.fireForm.reset({
      reason: '',
      eligibleForRehire: false
    });
    this.fireDialogVisible.set(true);
  }

  closeFireDialog(): void {
    this.fireDialogVisible.set(false);
  }

  submitFire(): void {
    const employee = this.selectedEmployee();
    if (!employee) {
      return;
    }

    const value = this.fireForm.getRawValue();
    const request: FireEmployeeRequest = {
      reason: value.reason || undefined,
      eligibleForRehire: value.eligibleForRehire ?? undefined
    };

    this.fireSubmitting.set(true);
    this.hrApi.fireEmployee(employee.id, request).subscribe({
      next: () => {
        this.fireSubmitting.set(false);
        this.fireDialogVisible.set(false);
        this.flashFeedback(`Employee ${employee.fullName} has been terminated.`);
        this.reloadEmployee(employee.id);
      },
      error: error => {
        this.fireSubmitting.set(false);
        this.handleActionError(error, 'Unable to terminate employee. Please try again.', this.firePermissions);
      }
    });
  }

  openBonusDialog(employee: EmployeeDto, event?: Event): void {
    event?.stopPropagation();
    if (!this.requirePermission(this.bonusPermissions, 'Issuing bonuses')) {
      return;
    }

    this.selectedEmployee.set(employee);
    this.bonusForm.reset({
      bonusType: 'General',
      amount: null,
      awardedOn: null,
      period: '',
      reference: '',
      awardedBy: '',
      notes: '',
      sourceSystem: '',
      issuedBy: ''
    });
    this.bonusDialogVisible.set(true);
  }

  closeBonusDialog(): void {
    this.bonusDialogVisible.set(false);
  }

  submitBonus(): void {
    const employee = this.selectedEmployee();
    if (!employee) {
      return;
    }

    if (this.bonusForm.invalid) {
      this.bonusForm.markAllAsTouched();
      return;
    }

    const value = this.bonusForm.getRawValue();
    const bonus: EmployeeBonusItemRequest = {
      bonusType: value.bonusType!,
      amount: Number(value.amount),
      awardedOn: this.toIsoString(value.awardedOn),
      period: value.period || undefined,
      reference: value.reference || undefined,
      awardedBy: value.awardedBy || undefined,
      notes: value.notes || undefined,
      sourceSystem: value.sourceSystem || undefined
    };

    const request: IssueEmployeeBonusesRequest = {
      bonuses: [bonus],
      issuedBy: value.issuedBy || undefined
    };

    this.bonusSubmitting.set(true);
    this.hrApi.issueEmployeeBonuses(employee.id, request).subscribe({
      next: () => {
        this.bonusSubmitting.set(false);
        this.bonusDialogVisible.set(false);
        this.flashFeedback(`Bonus issued to ${employee.fullName}.`);
      },
      error: error => {
        this.bonusSubmitting.set(false);
        this.handleActionError(error, 'Unable to issue bonus. Please try again.', this.bonusPermissions);
      }
    });
  }

  openDeductionDialog(employee: EmployeeDto, event?: Event): void {
    event?.stopPropagation();
    if (!this.requirePermission(this.deductionPermissions, 'Issuing deductions')) {
      return;
    }

    this.selectedEmployee.set(employee);
    this.deductionForm.reset({
      deductionType: 'General',
      amount: null,
      appliedOn: null,
      period: '',
      reference: '',
      appliedBy: '',
      notes: '',
      sourceSystem: '',
      isRecurring: false,
      issuedBy: ''
    });
    this.deductionDialogVisible.set(true);
  }

  closeDeductionDialog(): void {
    this.deductionDialogVisible.set(false);
  }

  submitDeduction(): void {
    const employee = this.selectedEmployee();
    if (!employee) {
      return;
    }

    if (this.deductionForm.invalid) {
      this.deductionForm.markAllAsTouched();
      return;
    }

    const value = this.deductionForm.getRawValue();
    const deduction: EmployeeDeductionItemRequest = {
      deductionType: value.deductionType!,
      amount: Number(value.amount),
      appliedOn: this.toIsoString(value.appliedOn),
      period: value.period || undefined,
      reference: value.reference || undefined,
      appliedBy: value.appliedBy || undefined,
      notes: value.notes || undefined,
      sourceSystem: value.sourceSystem || undefined,
      isRecurring: value.isRecurring ?? false
    };

    const request: IssueEmployeeDeductionsRequest = {
      deductions: [deduction],
      issuedBy: value.issuedBy || undefined
    };

    this.deductionSubmitting.set(true);
    this.hrApi.issueEmployeeDeductions(employee.id, request).subscribe({
      next: () => {
        this.deductionSubmitting.set(false);
        this.deductionDialogVisible.set(false);
        this.flashFeedback(`Deduction issued to ${employee.fullName}.`);
      },
      error: error => {
        this.deductionSubmitting.set(false);
        this.handleActionError(error, 'Unable to issue deduction. Please try again.', this.deductionPermissions);
      }
    });
  }

  clearFeedback(): void {
    this.feedback.set(null);
    if (this.feedbackTimeout) {
      clearTimeout(this.feedbackTimeout);
      this.feedbackTimeout = null;
    }
  }

  employeeControlInvalid(controlName: string): boolean {
    const control = this.employeeForm.get(controlName);
    return !!control && control.invalid && (control.touched || control.dirty || this.formSubmitting());
  }

  bonusControlInvalid(controlName: string): boolean {
    const control = this.bonusForm.get(controlName);
    return !!control && control.invalid && (control.touched || control.dirty || this.bonusSubmitting());
  }

  deductionControlInvalid(controlName: string): boolean {
    const control = this.deductionForm.get(controlName);
    return !!control && control.invalid && (control.touched || control.dirty || this.deductionSubmitting());
  }

  private loadEmployees(): void {
    this.loading.set(true);
    this.error.set(null);

    this.hrApi.getEmployees().subscribe({
      next: employees => {
        const list = this.sortEmployees(employees ?? []);
        this.employees.set(list);
        const currentSelection = this.selectedEmployee();
        if (currentSelection) {
          const refreshed = list.find(employee => employee.id === currentSelection.id) ?? null;
          this.selectedEmployee.set(refreshed);
        }
        this.loading.set(false);
      },
      error: error => {
        this.loading.set(false);
        if (this.isAuthorizationError(error)) {
          this.error.set('You are not allowed to view employees. Contact an administrator to request HR permissions.');
        } else {
          this.error.set('Unable to load employees. Please try again later.');
        }
      }
    });
  }

  private reloadEmployee(id: string, fallbackDepartmentName?: string): void {
    this.hrApi.getEmployee(id).subscribe({
      next: employee => {
        this.updateEmployeeInList(employee);
      },
      error: () => {
        if (fallbackDepartmentName) {
          const updated = this.employeeForm.value;
          this.employees.update(list => list.map(item => {
            if (item.id !== id) {
              return item;
            }

            return {
              ...item,
              fullName: updated.fullName ?? item.fullName,
              workEmail: updated.workEmail ?? item.workEmail,
              personalEmail: updated.personalEmail ?? item.personalEmail,
              position: updated.position ?? item.position,
              phoneNumber: updated.phoneNumber ?? item.phoneNumber,
              salary: Number(updated.salary ?? item.salary),
              hireDate: updated.hireDate ? new Date(updated.hireDate).toISOString() : item.hireDate,
              status: (updated.status as EmploymentStatus) ?? item.status,
              departmentId: updated.departmentId ?? item.departmentId,
              departmentName: fallbackDepartmentName,
              exitDate: updated.exitDate ? new Date(updated.exitDate).toISOString() : item.exitDate,
              exitReason: updated.exitReason ?? item.exitReason,
              eligibleForRehire: updated.eligibleForRehire ?? item.eligibleForRehire,
              userId: updated.userId || item.userId
            };
          }));
        }
        this.flashFeedback('Employee updated, but details may be stale. Consider refreshing.');
      }
    });
  }

  private ensureDepartmentsLoaded(): void {
    if (this.departmentsLoaded()) {
      return;
    }

    if (this.departmentsLoading()) {
      return;
    }

    this.departmentsLoading.set(true);
    this.hrApi.getDepartments().subscribe({
      next: departments => {
        this.departments.set(departments ?? []);
        this.departmentsLoaded.set(true);
        this.departmentsLoading.set(false);
      },
      error: () => {
        this.departmentsLoading.set(false);
        this.flashFeedback('Unable to load departments. Department selection may be limited.');
      }
    });
  }

  private hasAnyPermission(perms: string[]): boolean {
    return perms.some(permission => this.auth.hasPermission(permission) || this.auth.hasPermissionPrefix(permission));
  }

  private requirePermission(perms: string[], actionDescription: string): boolean {
    if (this.hasAnyPermission(perms)) {
      return true;
    }

    this.flashFeedback(`${actionDescription} requires permissions: ${this.formatPermissionList(perms)}.`);
    return false;
  }

  private formatPermissionList(perms: string[]): string {
    if (perms.length === 1) {
      return `'${perms[0]}'`;
    }

    return perms.map(perm => `'${perm}'`).join(' or ');
  }

  private flashFeedback(message: string): void {
    this.feedback.set(message);
    if (this.feedbackTimeout) {
      clearTimeout(this.feedbackTimeout);
    }
    this.feedbackTimeout = setTimeout(() => {
      this.feedback.set(null);
      this.feedbackTimeout = null;
    }, 5000);
  }

  private sortEmployees(employees: EmployeeDto[]): EmployeeDto[] {
    return [...employees].sort((a, b) => a.fullName.localeCompare(b.fullName));
  }

  private updateEmployeeInList(employee: EmployeeDto): void {
    this.employees.update(list => this.sortEmployees(list.map(item => (item.id === employee.id ? employee : item))));
    this.selectedEmployee.set(employee);
  }

  private toDate(value?: string | null): Date | null {
    if (!value) {
      return null;
    }

    const date = new Date(value);
    return Number.isNaN(date.getTime()) ? null : date;
  }

  private isAuthorizationError(error: unknown): error is HttpErrorResponse {
    return error instanceof HttpErrorResponse && (error.status === 401 || error.status === 403);
  }

  private handleActionError(error: unknown, fallbackMessage: string, perms?: string[]): void {
    if (this.isAuthorizationError(error) && perms?.length) {
      this.flashFeedback(`Not allowed. This action requires ${this.formatPermissionList(perms)}.`);
      return;
    }

    this.flashFeedback(fallbackMessage);
  }

  private toIsoString(value: unknown): string | undefined {
    if (!value) {
      return undefined;
    }

    if (value instanceof Date) {
      return Number.isNaN(value.getTime()) ? undefined : value.toISOString();
    }

    const parsed = new Date(value as string);
    return Number.isNaN(parsed.getTime()) ? undefined : parsed.toISOString();
  }
}
