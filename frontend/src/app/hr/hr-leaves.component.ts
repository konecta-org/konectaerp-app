import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, effect, inject, signal } from '@angular/core';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../core/services/auth.service';
import {
  CreateLeaveRequestRequest,
  EmployeeDto,
  HrApiService,
  LeaveRequestDto,
  UpdateLeaveRequestRequest
} from '../core/services/hr-api.service';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { TooltipModule } from 'primeng/tooltip';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';
import { DialogModule } from 'primeng/dialog';
import { DatePickerModule } from 'primeng/datepicker';
import { SelectModule } from 'primeng/select';
import { TextareaModule } from 'primeng/textarea';
import { MessageModule } from 'primeng/message';
import { InputSwitchModule } from 'primeng/inputswitch';

type LeaveTypeCode = 'Vacation' | 'Sick' | 'Unpaid' | 'Parental' | 'Bereavement' | 'Other';
type LeaveStatusCode = 'Pending' | 'Approved' | 'Rejected' | 'Cancelled';
type LeaveFormMode = 'create' | 'edit';

@Component({
  selector: 'app-hr-leaves',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    TableModule,
    ButtonModule,
    TooltipModule,
    CardModule,
    TagModule,
    DialogModule,
    DatePickerModule,
    SelectModule,
    TextareaModule,
    MessageModule,
    InputSwitchModule
  ],
  templateUrl: './hr-leaves.component.html',
  styleUrls: ['./hr-leaves.component.scss']
})
export class HrLeavesComponent implements OnInit {
  private readonly hrApi = inject(HrApiService);
  private readonly auth = inject(AuthService);
  private readonly fb = inject(FormBuilder);

  private readonly managePermissions = ['hr.leaves.manage'];
  private readonly deletePermissions = ['hr.leaves.delete', 'hr.leaves.manage'];

  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly feedback = signal<string | null>(null);
  readonly leaves = signal<LeaveRequestDto[]>([]);
  readonly employees = signal<EmployeeDto[]>([]);
  readonly selectedLeave = signal<LeaveRequestDto | null>(null);
  readonly pendingOnly = signal(false);

  readonly currentEmployeeId = signal<string | null>(null);
  readonly currentEmployeeName = signal<string | null>(null);
  readonly currentEmployeeLoading = signal(false);
  readonly currentEmployeeError = signal<string | null>(null);

  readonly formVisible = signal(false);
  readonly formMode = signal<LeaveFormMode>('create');
  readonly formSubmitting = signal(false);

  readonly deleteVisible = signal(false);
  readonly deleteSubmitting = signal(false);

  private readonly syncEmployeeToFormEffect = effect(() => {
    if (this.formMode() !== 'create' || this.employeeSelectVisible()) {
      return;
    }

    const employeeId = this.currentEmployeeId();
    if (!employeeId) {
      return;
    }

    const control = this.leaveForm.controls.employeeId;
    if (control.value !== employeeId) {
      control.setValue(employeeId, { emitEvent: false });
      control.markAsPristine();
      control.markAsUntouched();
    }

    if (this.currentEmployeeError()) {
      this.currentEmployeeError.set(null);
    }
  });

  readonly hasLeaves = computed(() => this.leaves().length > 0);
  readonly detailVisible = computed(() => this.selectedLeave() !== null);
  readonly canManageLeaves = computed(() => this.hasAnyPermission(this.managePermissions));
  readonly canDeleteLeaves = computed(() => this.hasAnyPermission(this.deletePermissions));
  readonly managePermissionLabel = this.formatPermissionList(this.managePermissions);
  readonly deletePermissionLabel = this.formatPermissionList(this.deletePermissions);

  get pendingFilterValue(): boolean {
    return this.pendingOnly();
  }

  set pendingFilterValue(value: boolean) {
    this.setPendingFilter(value);
  }

  readonly employeeOptions = computed(() =>
    this.employees().map(employee => ({
      label: employee.fullName,
      value: employee.id,
      description: employee.position
    }))
  );

  readonly formEmployeeName = computed(() => {
    if (this.formMode() === 'edit') {
      return this.selectedLeave()?.employeeName ?? null;
    }

    return this.currentEmployeeName();
  });

  readonly employeeSelectVisible = computed(
    () =>
      this.formMode() === 'create' && (!this.currentEmployeeId() || !!this.currentEmployeeError() || !this.formEmployeeName())
  );

  readonly leaveTypeOptions: { label: string; value: LeaveTypeCode }[] = [
    { label: 'Vacation', value: 'Vacation' },
    { label: 'Sick leave', value: 'Sick' },
    { label: 'Unpaid leave', value: 'Unpaid' },
    { label: 'Parental leave', value: 'Parental' },
    { label: 'Bereavement leave', value: 'Bereavement' },
    { label: 'Other', value: 'Other' }
  ];

  readonly leaveStatusOptions: { label: string; value: LeaveStatusCode }[] = [
    { label: 'Pending', value: 'Pending' },
    { label: 'Approved', value: 'Approved' },
    { label: 'Rejected', value: 'Rejected' },
    { label: 'Cancelled', value: 'Cancelled' }
  ];

  private readonly leaveTypeEnumMap: Record<LeaveTypeCode, number> = {
    Vacation: 0,
    Sick: 1,
    Unpaid: 2,
    Parental: 3,
    Bereavement: 4,
    Other: 5
  };

  private readonly leaveStatusEnumMap: Record<LeaveStatusCode, number> = {
    Pending: 0,
    Approved: 1,
    Rejected: 2,
    Cancelled: 3
  };

  readonly leaveForm = this.fb.group({
    employeeId: this.fb.control<string | null>(null, Validators.required),
    leaveType: this.fb.control<LeaveTypeCode>('Vacation', { nonNullable: true }),
    startDate: this.fb.control<Date | null>(null, Validators.required),
    endDate: this.fb.control<Date | null>(null, Validators.required),
    reason: this.fb.control<string | null>(null, [Validators.maxLength(2000)]),
    status: this.fb.control<LeaveStatusCode>('Pending', { nonNullable: true }),
    approvedByEmployeeId: this.fb.control<string | null>(null)
  });

  readonly today = new Date();

  ngOnInit(): void {
    this.loadEmployees();
    this.determineCurrentEmployee();
    this.loadLeaves();
  }

  reload(): void {
    this.loadLeaves();
  }

  setPendingFilter(pendingOnly: boolean): void {
    if (this.pendingOnly() === pendingOnly) {
      return;
    }

    this.pendingOnly.set(pendingOnly);
    this.loadLeaves();
  }

  clearFeedback(): void {
    this.feedback.set(null);
  }

  trackById(_: number, leave: LeaveRequestDto): string {
    return leave.id;
  }

  viewLeave(leave: LeaveRequestDto): void {
    this.selectedLeave.set(leave);
  }

  closeDetails(): void {
    this.selectedLeave.set(null);
  }

  openCreateDialog(): void {
    const employeeId = this.currentEmployeeId();
    if (!employeeId) {
      this.currentEmployeeError.set('Select your employee record from the list below.');
    }

    this.selectedLeave.set(null);
    this.formMode.set('create');
    this.toggleStatusInputs(true);
    this.leaveForm.reset({
      employeeId: employeeId ?? null,
      leaveType: 'Vacation',
      startDate: null,
      endDate: null,
      reason: null,
      status: 'Pending',
      approvedByEmployeeId: null
    });
    this.formVisible.set(true);
  }

  openEditDialog(leave: LeaveRequestDto, event?: Event): void {
    event?.stopPropagation();

    if (!this.canManageLeaves()) {
      return;
    }

    this.selectedLeave.set(leave);
    this.formMode.set('edit');
    this.toggleStatusInputs(false);

    this.leaveForm.reset({
      employeeId: leave.employeeId,
      leaveType: this.leaveTypeValueFromDto(leave.leaveType),
      startDate: this.parseDate(leave.startDate),
      endDate: this.parseDate(leave.endDate),
      reason: leave.reason ?? null,
      status: this.leaveStatusValueFromDto(leave.status),
      approvedByEmployeeId: leave.approvedByEmployeeId ?? null
    });
    this.formVisible.set(true);
  }

  closeFormDialog(): void {
    if (this.formSubmitting()) {
      return;
    }

    this.formVisible.set(false);
    this.leaveForm.reset({
      employeeId: null,
      leaveType: 'Vacation',
      startDate: null,
      endDate: null,
      reason: null,
      status: 'Pending',
      approvedByEmployeeId: null
    });
    this.toggleStatusInputs(false);
  }

  submitLeaveForm(): void {
    if (this.leaveForm.invalid) {
      this.leaveForm.markAllAsTouched();
      return;
    }

    const formValue = this.leaveForm.getRawValue();
    const start = formValue.startDate ? new Date(formValue.startDate) : null;
    const end = formValue.endDate ? new Date(formValue.endDate) : null;

    if (!start || !end) {
      this.leaveForm.controls.startDate.markAsTouched();
      this.leaveForm.controls.endDate.markAsTouched();
      return;
    }

    if (start.getTime() > end.getTime()) {
      this.leaveForm.controls.endDate.setErrors({ range: true });
      this.leaveForm.controls.endDate.markAsTouched();
      return;
    }

    const reason = formValue.reason?.trim();
    const employeeId = formValue.employeeId ?? this.currentEmployeeId();

    if (!employeeId) {
      this.currentEmployeeError.set('We could not determine your employee profile. Please contact HR.');
      return;
    }

    const payloadBase = {
      employeeId,
      leaveType: this.leaveTypeEnumMap[formValue.leaveType ?? 'Vacation'],
      startDate: this.toIsoDate(start),
      endDate: this.toIsoDate(end),
      reason: reason && reason.length ? reason : null
    } satisfies CreateLeaveRequestRequest;

    this.formSubmitting.set(true);

    if (this.formMode() === 'create') {
      this.hrApi.createLeaveRequest(payloadBase).subscribe({
        next: leave => {
          const normalized = this.normalizeLeave(leave);
          this.addOrUpdateLeave(normalized);
          this.selectedLeave.set(normalized);
          this.feedback.set('Leave request recorded successfully.');
          this.formSubmitting.set(false);
          this.formVisible.set(false);
          this.toggleStatusInputs(false);
        },
        error: () => {
          this.formSubmitting.set(false);
          this.error.set('Unable to create leave request. Please try again.');
        }
      });
      return;
    }

    const current = this.selectedLeave();
    if (!current) {
      this.formSubmitting.set(false);
      return;
    }

    const payload: UpdateLeaveRequestRequest = {
      ...payloadBase,
      id: current.id,
      status: this.leaveStatusEnumMap[formValue.status ?? 'Pending'],
      approvedByEmployeeId: formValue.approvedByEmployeeId || null
    };

    this.hrApi.updateLeaveRequest(current.id, payload).subscribe({
      next: () => {
        const updatedRaw: LeaveRequestDto = {
          ...current,
          employeeId: payload.employeeId,
          employeeName: this.resolveEmployeeName(payload.employeeId) ?? current.employeeName,
          leaveType: payload.leaveType,
          startDate: payload.startDate,
          endDate: payload.endDate,
          reason: payload.reason ?? null,
          status: payload.status,
          approvedByEmployeeId: payload.approvedByEmployeeId ?? null,
          approvedByName:
            payload.approvedByEmployeeId ? this.resolveEmployeeName(payload.approvedByEmployeeId) : current.approvedByName,
          updatedAt: new Date().toISOString()
        };

        const normalized = this.normalizeLeave(updatedRaw);
        this.updateLeaveInList(normalized);
        this.selectedLeave.set(normalized);
        this.feedback.set('Leave request updated successfully.');
        this.formSubmitting.set(false);
        this.formVisible.set(false);
      },
      error: () => {
        this.formSubmitting.set(false);
        this.error.set('Unable to update leave request. Please try again.');
      }
    });
  }

  openDeleteDialog(leave: LeaveRequestDto, event?: Event): void {
    event?.stopPropagation();

    if (!this.canDeleteLeaves()) {
      return;
    }

    this.selectedLeave.set(leave);
    this.deleteVisible.set(true);
  }

  closeDeleteDialog(): void {
    if (this.deleteSubmitting()) {
      return;
    }

    this.deleteVisible.set(false);
  }

  confirmDelete(): void {
    const leave = this.selectedLeave();
    if (!leave) {
      return;
    }

    this.deleteSubmitting.set(true);

    this.hrApi.deleteLeaveRequest(leave.id).subscribe({
      next: () => {
        const filtered = this.leaves().filter(item => item.id !== leave.id);
        this.leaves.set(filtered);
        this.syncSelection(filtered);
        this.feedback.set('Leave request deleted successfully.');
        this.deleteSubmitting.set(false);
        this.deleteVisible.set(false);
      },
      error: () => {
        this.deleteSubmitting.set(false);
        this.error.set('Unable to delete leave request. Please try again.');
      }
    });
  }

  setStatus(leave: LeaveRequestDto, status: LeaveStatusCode, event?: Event): void {
    event?.stopPropagation();

    if (!this.canManageLeaves()) {
      return;
    }

    const currentStatus = this.leaveStatusValueFromDto(leave.status);
    if (currentStatus === status) {
      return;
    }

    const payload = this.buildUpdatePayload(leave, {
      status: this.leaveStatusEnumMap[status]
    });

    this.hrApi.updateLeaveRequest(leave.id, payload).subscribe({
      next: () => {
        const updatedRaw: LeaveRequestDto = {
          ...leave,
          status: payload.status,
          approvedByEmployeeId: payload.approvedByEmployeeId ?? leave.approvedByEmployeeId ?? null,
          approvedByName:
            payload.approvedByEmployeeId
              ? this.resolveEmployeeName(payload.approvedByEmployeeId)
              : leave.approvedByName,
          updatedAt: new Date().toISOString()
        };

        const normalized = this.normalizeLeave(updatedRaw);
        this.updateLeaveInList(normalized);
        this.selectedLeave.set(normalized);
        this.feedback.set(this.statusActionMessage(status));
      },
      error: () => {
        this.error.set('Unable to update leave status. Please try again.');
      }
    });
  }

  statusTooltip(leave: LeaveRequestDto, status: LeaveStatusCode): string {
    if (!this.canManageLeaves()) {
      return `Requires ${this.managePermissionLabel}`;
    }

    if (this.leaveStatusValueFromDto(leave.status) === status) {
      return 'Already set to this status';
    }

    switch (status) {
      case 'Approved':
        return 'Approve leave request';
      case 'Rejected':
        return 'Reject leave request';
      case 'Cancelled':
        return 'Cancel leave request';
      default:
        return 'Update status';
    }
  }

  controlInvalid(controlName: keyof typeof this.leaveForm.controls): boolean {
    const control = this.leaveForm.controls[controlName];
    return control.invalid && control.touched;
  }

  actionLockClass(locked: boolean): Record<'is-locked', boolean> {
    return { 'is-locked': locked };
  }

  leaveTypeLabel(value?: string | number | null): string {
    if (value === null || value === undefined) {
      return 'Vacation';
    }

    if (typeof value === 'number') {
      const entry = Object.entries(this.leaveTypeEnumMap).find(([, enumValue]) => enumValue === value);
      return entry ? this.leaveTypeLabel(entry[0] as LeaveTypeCode) : 'Vacation';
    }

    const normalized = value.toString();
    const option = this.leaveTypeOptions.find(opt => opt.value === normalized);
    return option ? option.label : normalized.toString();
  }

  leaveStatusLabel(value?: string | number | null): string {
    if (value === null || value === undefined) {
      return 'Pending';
    }

    if (typeof value === 'number') {
      const entry = Object.entries(this.leaveStatusEnumMap).find(([, enumValue]) => enumValue === value);
      return entry ? this.leaveStatusLabel(entry[0] as LeaveStatusCode) : 'Pending';
    }

    const normalized = value.toString().trim().toLowerCase();
    switch (normalized) {
      case 'approved':
        return 'Approved';
      case 'rejected':
        return 'Rejected';
      case 'cancelled':
      case 'canceled':
        return 'Cancelled';
      case 'pending':
        return 'Pending';
      default:
        return value.toString();
    }
  }

  leaveStatusSeverity(value?: string | number | null): 'info' | 'success' | 'warning' | 'danger' {
    const label = this.leaveStatusLabel(value).toLowerCase();
    switch (label) {
      case 'approved':
        return 'success';
      case 'rejected':
        return 'danger';
      case 'cancelled':
        return 'info';
      default:
        return 'warning';
    }
  }

  deleteTooltip(): string {
    if (!this.canDeleteLeaves()) {
      return `Requires ${this.deletePermissionLabel}`;
    }

    return 'Delete leave request';
  }

  private loadLeaves(): void {
    this.loading.set(true);
    this.error.set(null);

    this.hrApi.getLeaveRequests({ pendingOnly: this.pendingOnly() }).subscribe({
      next: data => {
        const normalized = (data ?? []).map(item => this.normalizeLeave(item));
        const sorted = this.sortLeaves(normalized);
        this.leaves.set(sorted);
        this.syncSelection(sorted);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Unable to load leave requests.');
        this.loading.set(false);
      }
    });
  }

  private loadEmployees(): void {
    this.hrApi.getEmployees().subscribe({
      next: employees => {
        const list = employees ?? [];
        this.employees.set(list);
        this.syncCurrentEmployee(list);
      }
    });
  }

  private determineCurrentEmployee(): void {
    const session = this.auth.currentSession();
    if (!session?.userId) {
      this.currentEmployeeError.set('Sign in with an employee-linked account to auto-fill this field.');
      return;
    }

    this.currentEmployeeLoading.set(true);
    this.hrApi.getEmployees().subscribe({
      next: employees => {
        const list = employees ?? [];
        const match = list.find(employee => employee.userId?.toLowerCase() === session.userId.toLowerCase());
        if (match) {
          this.currentEmployeeId.set(match.id);
          this.currentEmployeeName.set(match.fullName);
          this.currentEmployeeError.set(null);
        } else {
          this.currentEmployeeError.set('Select your employee record from the list below.');
        }
        this.currentEmployeeLoading.set(false);
      },
      error: () => {
        this.currentEmployeeLoading.set(false);
        this.currentEmployeeError.set('Select your employee record from the list below.');
      }
    });
  }

  private normalizeLeave(leave: LeaveRequestDto): LeaveRequestDto {
    return {
      ...leave,
      leaveType: this.leaveTypeLabel(leave.leaveType),
      status: this.leaveStatusLabel(leave.status),
      reason: leave.reason ?? null,
      approvedByEmployeeId: leave.approvedByEmployeeId ?? null,
      approvedByName:
        leave.approvedByName ?? (leave.approvedByEmployeeId ? this.resolveEmployeeName(leave.approvedByEmployeeId) : null),
      requestedAt: leave.requestedAt ?? leave.startDate,
      updatedAt: leave.updatedAt ?? null
    };
  }

  private sortLeaves(leaves: LeaveRequestDto[]): LeaveRequestDto[] {
    const getTime = (value?: string | null) => {
      if (!value) {
        return 0;
      }

      const time = new Date(value).getTime();
      return Number.isNaN(time) ? 0 : time;
    };

    return [...leaves].sort((a, b) => getTime(b.requestedAt) - getTime(a.requestedAt));
  }

  private updateLeaveInList(updated: LeaveRequestDto): void {
    if (this.pendingOnly() && this.leaveStatusLabel(updated.status) !== 'Pending') {
      const filtered = this.leaves().filter(item => item.id !== updated.id);
      this.leaves.set(filtered);
      this.syncSelection(filtered);
      return;
    }

    const list = this.leaves().map(item => (item.id === updated.id ? updated : item));
    const sorted = this.sortLeaves(list);
    this.leaves.set(sorted);
    this.syncSelection(sorted);
  }

  private addOrUpdateLeave(leave: LeaveRequestDto): void {
    if (this.pendingOnly() && this.leaveStatusLabel(leave.status) !== 'Pending') {
      return;
    }

    const existing = this.leaves().some(item => item.id === leave.id);
    const list = existing ? this.leaves().map(item => (item.id === leave.id ? leave : item)) : [...this.leaves(), leave];
    const sorted = this.sortLeaves(list);
    this.leaves.set(sorted);
    this.syncSelection(sorted);
  }

  private syncSelection(list: LeaveRequestDto[]): void {
    if (!list.length) {
      this.selectedLeave.set(null);
      return;
    }

    const current = this.selectedLeave();
    if (!current) {
      this.selectedLeave.set(list[0]);
      return;
    }

    const found = list.find(item => item.id === current.id);
    this.selectedLeave.set(found ?? list[0]);
  }

  private resolveEmployeeName(employeeId?: string | null): string | null {
    if (!employeeId) {
      return null;
    }

    const match = this.employees().find(employee => employee.id === employeeId);
    return match ? match.fullName : null;
  }

  private leaveTypeValueFromDto(value: string | number | undefined): LeaveTypeCode {
    if (typeof value === 'number') {
      const entry = Object.entries(this.leaveTypeEnumMap).find(([, enumValue]) => enumValue === value);
      return (entry?.[0] as LeaveTypeCode) ?? 'Vacation';
    }

    const normalized = value?.toString() ?? 'Vacation';
    const option = this.leaveTypeOptions.find(opt => opt.value === normalized);
    return option ? option.value : 'Vacation';
  }

  private leaveStatusValueFromDto(value: string | number | undefined): LeaveStatusCode {
    if (typeof value === 'number') {
      const entry = Object.entries(this.leaveStatusEnumMap).find(([, enumValue]) => enumValue === value);
      return (entry?.[0] as LeaveStatusCode) ?? 'Pending';
    }

    const normalized = value?.toString().trim().toLowerCase();
    switch (normalized) {
      case 'approved':
        return 'Approved';
      case 'rejected':
        return 'Rejected';
      case 'cancelled':
      case 'canceled':
        return 'Cancelled';
      default:
        return 'Pending';
    }
  }

  private syncCurrentEmployee(employees: EmployeeDto[]): void {
    const session = this.auth.currentSession();
    if (!session?.userId) {
      this.currentEmployeeError.set('No authenticated user found.');
      return;
    }

    const match = employees.find(employee => employee.userId?.toLowerCase() === session.userId.toLowerCase());
    if (match) {
      this.currentEmployeeId.set(match.id);
      this.currentEmployeeName.set(match.fullName);
      this.currentEmployeeError.set(null);
      return;
    }

    this.currentEmployeeError.set('We could not find your employee profile. Please contact HR.');
  }

  private buildUpdatePayload(
    leave: LeaveRequestDto,
    overrides: Partial<Pick<UpdateLeaveRequestRequest, 'status' | 'approvedByEmployeeId' | 'reason'>>
  ): UpdateLeaveRequestRequest {
    const start = this.parseDate(leave.startDate);
    const end = this.parseDate(leave.endDate);
    const leaveTypeCode = this.leaveTypeValueFromDto(leave.leaveType);
    const statusCode = this.leaveStatusValueFromDto(leave.status);

    return {
      id: leave.id,
      employeeId: leave.employeeId,
      leaveType:
        typeof leave.leaveType === 'number'
          ? leave.leaveType
          : this.leaveTypeEnumMap[leaveTypeCode],
      startDate: start ? this.toIsoDate(start) : leave.startDate,
      endDate: end ? this.toIsoDate(end) : leave.endDate,
      reason: overrides.reason ?? leave.reason ?? null,
      status:
        overrides.status ??
        (typeof leave.status === 'number'
          ? leave.status
          : this.leaveStatusEnumMap[statusCode]),
      approvedByEmployeeId: overrides.approvedByEmployeeId ?? leave.approvedByEmployeeId ?? null
    };
  }

  private statusActionMessage(status: LeaveStatusCode): string {
    switch (status) {
      case 'Approved':
        return 'Leave request approved successfully.';
      case 'Rejected':
        return 'Leave request rejected successfully.';
      case 'Cancelled':
        return 'Leave request cancelled successfully.';
      default:
        return 'Leave status updated successfully.';
    }
  }

  private parseDate(value?: string | Date | null): Date | null {
    if (!value) {
      return null;
    }

    if (value instanceof Date) {
      return value;
    }

    const parsed = new Date(value);
    return Number.isNaN(parsed.getTime()) ? null : parsed;
  }

  private toIsoDate(date: Date): string {
    const copy = new Date(date);
    copy.setHours(0, 0, 0, 0);
    return copy.toISOString();
  }

  private toggleStatusInputs(disabled: boolean): void {
    const controls = this.leaveForm.controls;
    if (disabled) {
      controls.status.disable({ emitEvent: false });
      controls.approvedByEmployeeId.enable({ emitEvent: false });
    } else {
      controls.status.enable({ emitEvent: false });
      controls.approvedByEmployeeId.enable({ emitEvent: false });
    }
  }

  private hasAnyPermission(perms: string[]): boolean {
    return perms.some(permission => this.auth.hasPermission(permission));
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
