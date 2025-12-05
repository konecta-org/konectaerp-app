import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, effect, inject, signal } from '@angular/core';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../core/services/auth.service';
import {
  AttendanceRecordDto,
  CreateAttendanceRecordRequest,
  EmployeeDto,
  HrApiService,
  UpdateAttendanceRecordRequest
} from '../core/services/hr-api.service';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { TooltipModule } from 'primeng/tooltip';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';
import { DialogModule } from 'primeng/dialog';
import { DatePickerModule } from 'primeng/datepicker';
import { SelectModule } from 'primeng/select';
import { MessageModule } from 'primeng/message';
import { TextareaModule } from 'primeng/textarea';

type AttendanceStatusCode = 'Present' | 'Absent' | 'Remote' | 'OnLeave' | 'Holiday';

const startOfTodayLocal = (): Date => {
  const date = new Date();
  date.setHours(0, 0, 0, 0);
  return date;
};

@Component({
  selector: 'app-hr-attendance',
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
    MessageModule,
    TextareaModule
  ],
  templateUrl: './hr-attendance.component.html',
  styleUrls: ['./hr-attendance.component.scss']
})
export class HrAttendanceComponent implements OnInit {
  private readonly hrApi = inject(HrApiService);
  private readonly auth = inject(AuthService);
  private readonly fb = inject(FormBuilder);

  private readonly managePermissions = ['hr.attendance.manage'];

  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly feedback = signal<string | null>(null);
  readonly records = signal<AttendanceRecordDto[]>([]);
  readonly employees = signal<EmployeeDto[]>([]);
  readonly selectedRecord = signal<AttendanceRecordDto | null>(null);

  readonly currentEmployeeId = signal<string | null>(null);
  readonly currentEmployeeName = signal<string | null>(null);
  readonly currentEmployeeLoading = signal(false);
  readonly currentEmployeeError = signal<string | null>(null);

  readonly formVisible = signal(false);
  readonly formSubmitting = signal(false);
  readonly checkoutDialogVisible = signal(false);
  readonly checkoutSubmitting = signal(false);
  readonly checkoutSubmittingId = signal<string | null>(null);
  readonly checkoutRecord = signal<AttendanceRecordDto | null>(null);

  readonly canManageAttendance = computed(() => this.hasAnyPermission(this.managePermissions));
  readonly managePermissionLabel = this.formatPermissionList(this.managePermissions);

  readonly hasRecords = computed(() => this.records().length > 0);
  readonly detailVisible = computed(() => this.selectedRecord() !== null);

  readonly employeeOptions = computed(() =>
    this.employees().map(employee => ({
      label: employee.fullName,
      value: employee.id,
      description: employee.position
    }))
  );

  readonly formEmployeeName = computed(() => this.currentEmployeeName());

  readonly employeeSelectVisible = computed(
    () => !this.currentEmployeeId() || !!this.currentEmployeeError() || !this.formEmployeeName()
  );

  readonly attendanceStatusOptions: { label: string; value: AttendanceStatusCode }[] = [
    { label: 'Present', value: 'Present' },
    { label: 'Absent', value: 'Absent' },
    { label: 'Remote', value: 'Remote' },
    { label: 'On leave', value: 'OnLeave' },
    { label: 'Holiday', value: 'Holiday' }
  ];

  private readonly attendanceStatusEnumMap: Record<AttendanceStatusCode, number> = {
    Present: 0,
    Absent: 1,
    Remote: 2,
    OnLeave: 3,
    Holiday: 4
  };

  readonly attendanceForm = this.fb.group({
    employeeId: this.fb.control<string | null>(null, Validators.required),
    workDate: this.fb.control<Date | null>(startOfTodayLocal(), Validators.required),
    status: this.fb.control<AttendanceStatusCode>('Present', { nonNullable: true }),
    notes: this.fb.control<string | null>(null, [Validators.maxLength(1000)])
  });

  readonly checkoutForm = this.fb.group({
    checkOutTime: this.fb.control<Date | null>(null, Validators.required)
  });

  private readonly syncEmployeeToFormEffect = effect(() => {
    if (!this.formVisible() || this.employeeSelectVisible()) {
      return;
    }

    const employeeId = this.currentEmployeeId();
    if (!employeeId) {
      return;
    }

    const control = this.attendanceForm.controls.employeeId;
    if (control.value !== employeeId) {
      control.setValue(employeeId, { emitEvent: false });
      control.markAsPristine();
      control.markAsUntouched();
    }

    if (this.currentEmployeeError()) {
      this.currentEmployeeError.set(null);
    }
  });

  readonly today = startOfTodayLocal();

  ngOnInit(): void {
    this.loadEmployees();
    this.determineCurrentEmployee();
    this.loadAttendance();
  }

  reload(): void {
    this.loadAttendance();
  }

  openAttendDialog(): void {
    const employeeId = this.currentEmployeeId();
    if (!employeeId) {
      this.currentEmployeeError.set('Select your employee record from the list below.');
    }

    this.attendanceForm.reset({
      employeeId: employeeId ?? null,
      workDate: startOfTodayLocal(),
      status: 'Present',
      notes: null
    });

    this.formVisible.set(true);
  }

  closeFormDialog(): void {
    if (this.formSubmitting()) {
      return;
    }

    this.formVisible.set(false);
    this.attendanceForm.reset({
      employeeId: this.currentEmployeeId(),
      workDate: startOfTodayLocal(),
      status: 'Present',
      notes: null
    });
  }

  controlInvalid(controlName: keyof typeof this.attendanceForm.controls): boolean {
    const control = this.attendanceForm.controls[controlName];
    return control.invalid && control.touched;
  }

  checkoutDisabled(record: AttendanceRecordDto): boolean {
    const submitting = this.checkoutSubmittingId();

    if (this.currentEmployeeLoading()) {
      return true;
    }

    if (submitting && submitting !== record.id) {
      return true;
    }

    return this.checkoutSubmitting();
  }

  checkoutTooltip(record: AttendanceRecordDto): string {
    if (this.checkoutSubmittingId() === record.id || this.checkoutSubmitting()) {
      return 'Saving check-out…';
    }

    if (this.currentEmployeeLoading()) {
      return 'Loading data…';
    }

    return record.checkOutTime ? 'Edit check-out time' : 'Set check-out time';
  }

  submitAttendance(): void {
    if (this.attendanceForm.invalid) {
      this.attendanceForm.markAllAsTouched();
      return;
    }

    const employeeId = this.attendanceForm.controls.employeeId.value;
    if (!employeeId) {
      this.currentEmployeeError.set('Select your employee record from the list below.');
      return;
    }

    this.formSubmitting.set(true);

    const workDate = this.attendanceForm.controls.workDate.value ?? new Date();
    const isoDate = new Date(workDate).toISOString().split('T')[0];

    const request: CreateAttendanceRecordRequest = {
      employeeId,
      workDate: isoDate,
      checkInTime: new Date().toISOString(),
      status: this.attendanceStatusEnumMap[this.attendanceForm.controls.status.value],
      notes: this.attendanceForm.controls.notes.value?.trim() || null
    };

    this.hrApi.createAttendanceRecord(request).subscribe({
      next: record => {
        const normalized = this.normalizeRecord(record);
        this.records.set(this.sortRecords([normalized, ...this.records()]));
        this.selectedRecord.set(normalized);
        this.feedback.set('Attendance recorded successfully.');
        this.formSubmitting.set(false);
        this.formVisible.set(false);
      },
      error: error => {
        const message = error?.error?.message || 'Unable to record attendance. Please try again.';
        this.error.set(message);
        this.formSubmitting.set(false);
      }
    });
  }

  openCheckoutDialog(record: AttendanceRecordDto, event?: Event): void {
    event?.stopPropagation();

    if (this.checkoutDisabled(record)) {
      return;
    }

    this.checkoutRecord.set(record);
    const initial = record.checkOutTime ? new Date(record.checkOutTime) : new Date();
    this.checkoutForm.reset({ checkOutTime: initial });
    this.checkoutDialogVisible.set(true);
  }

  closeCheckoutDialog(): void {
    if (this.checkoutSubmitting()) {
      return;
    }

    this.checkoutDialogVisible.set(false);
    this.checkoutRecord.set(null);
    this.checkoutForm.reset({ checkOutTime: null });
  }

  submitCheckoutForm(): void {
    const record = this.checkoutRecord();
    if (!record) {
      return;
    }

    if (this.checkoutForm.invalid) {
      this.checkoutForm.markAllAsTouched();
      return;
    }

    const checkOutValue = this.checkoutForm.controls.checkOutTime.value;
    if (!checkOutValue) {
      return;
    }

    const checkoutIso = new Date(checkOutValue).toISOString();

    this.checkoutSubmitting.set(true);
    this.checkoutSubmittingId.set(record.id);

    const request: UpdateAttendanceRecordRequest = {
      id: record.id,
      checkInTime: record.checkInTime ?? null,
      checkOutTime: checkoutIso,
      status: this.attendanceStatusEnumMap[this.attendanceStatusValue(record.status)],
      notes: record.notes ?? null
    };

    this.hrApi.updateAttendanceRecord(record.id, request).subscribe({
      next: () => {
        const updated = this.normalizeRecord({
          ...record,
          checkOutTime: checkoutIso
        });
        const updatedList = this.records().map(item => (item.id === updated.id ? updated : item));
        this.records.set(updatedList);

        const currentDetail = this.selectedRecord();
        if (currentDetail?.id === updated.id) {
          this.selectedRecord.set(updated);
        }

        this.feedback.set('Check-out updated successfully.');
        this.checkoutSubmitting.set(false);
        this.checkoutSubmittingId.set(null);
        this.checkoutDialogVisible.set(false);
        this.checkoutRecord.set(updated);
      },
      error: error => {
        const message = error?.error?.message || 'Unable to update check-out. Please try again.';
        this.error.set(message);
        this.checkoutSubmitting.set(false);
        this.checkoutSubmittingId.set(null);
      }
    });
  }

  viewRecord(record: AttendanceRecordDto): void {
    this.selectedRecord.set(record);
  }

  closeDetails(): void {
    this.selectedRecord.set(null);
  }

  trackById(_: number, record: AttendanceRecordDto): string {
    return record.id;
  }

  clearFeedback(): void {
    this.feedback.set(null);
  }

  private loadAttendance(): void {
    this.loading.set(true);
    this.error.set(null);

    this.hrApi.getAttendance().subscribe({
      next: data => {
        const normalized = (data ?? []).map(item => this.normalizeRecord(item));
        const sorted = this.sortRecords(normalized);
        this.records.set(sorted);
        this.syncSelection(sorted);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Unable to load attendance records.');
        this.loading.set(false);
      }
    });
  }

  private loadEmployees(): void {
    this.hrApi.getEmployees().subscribe({
      next: employees => this.employees.set(employees ?? []),
      error: () => this.employees.set([])
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
        if (!this.employees().length) {
          this.employees.set(list);
        }
        const match = list.find(employee => employee.userId?.toLowerCase() === session.userId.toLowerCase());
        if (match) {
          this.currentEmployeeId.set(match.id);
          this.currentEmployeeName.set(match.fullName);
          this.currentEmployeeError.set(null);
          this.attendanceForm.controls.employeeId.setValue(match.id, { emitEvent: false });
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

  private hasAnyPermission(perms: string[]): boolean {
    return perms.some(permission => this.auth.hasPermission(permission));
  }

  private formatPermissionList(permissions: string[]): string {
    if (permissions.length === 1) {
      return permissions[0];
    }

    return permissions.slice(0, -1).join(', ') + ' or ' + permissions.at(-1);
  }

  attendanceStatusLabel(value?: string | number | null): string {
    if (value === null || value === undefined) {
      return 'Present';
    }

    if (typeof value === 'number') {
      const entry = Object.entries(this.attendanceStatusEnumMap).find(([, enumValue]) => enumValue === value);
      return entry ? this.attendanceStatusLabel(entry[0] as AttendanceStatusCode) : 'Present';
    }

    const normalized = value.toString().trim().toLowerCase();
    switch (normalized) {
      case 'present':
        return 'Present';
      case 'absent':
        return 'Absent';
      case 'remote':
        return 'Remote';
      case 'onleave':
      case 'on leave':
        return 'On leave';
      case 'holiday':
        return 'Holiday';
      default:
        return value.toString();
    }
  }

  attendanceStatusSeverity(value?: string | number | null): 'success' | 'danger' | 'info' | 'warning' {
    const label = this.attendanceStatusLabel(value).toLowerCase();
    switch (label) {
      case 'present':
        return 'success';
      case 'absent':
        return 'danger';
      case 'remote':
        return 'info';
      case 'on leave':
        return 'warning';
      case 'holiday':
        return 'info';
      default:
        return 'info';
    }
  }

  private attendanceStatusValue(value?: string | number | null): AttendanceStatusCode {
    if (value === null || value === undefined) {
      return 'Present';
    }

    if (typeof value === 'number') {
      const entry = Object.entries(this.attendanceStatusEnumMap).find(([, enumValue]) => enumValue === value);
      return entry ? (entry[0] as AttendanceStatusCode) : 'Present';
    }

    const normalized = value.toString().trim().toLowerCase();
    switch (normalized) {
      case 'present':
        return 'Present';
      case 'absent':
        return 'Absent';
      case 'remote':
        return 'Remote';
      case 'onleave':
      case 'on leave':
        return 'OnLeave';
      case 'holiday':
        return 'Holiday';
      default:
        return 'Present';
    }
  }

  private normalizeRecord(record: AttendanceRecordDto): AttendanceRecordDto {
    return {
      ...record,
      status: this.attendanceStatusLabel(record.status),
      notes: record.notes ?? undefined,
      checkInTime: record.checkInTime ?? undefined,
      checkOutTime: record.checkOutTime ?? undefined
    };
  }

  private sortRecords(records: AttendanceRecordDto[]): AttendanceRecordDto[] {
    const toTime = (value?: string | null) => {
      if (!value) {
        return 0;
      }

      const time = new Date(value).getTime();
      return Number.isNaN(time) ? 0 : time;
    };

    return [...records].sort((a, b) => toTime(b.workDate) - toTime(a.workDate));
  }

  private syncSelection(records: AttendanceRecordDto[]): void {
    const current = this.selectedRecord();
    if (!current) {
      return;
    }

    const match = records.find(record => record.id === current.id);
    this.selectedRecord.set(match ?? null);
  }

}
