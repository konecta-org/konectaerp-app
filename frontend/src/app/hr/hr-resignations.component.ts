import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import {
  EmployeeDto,
  HrApiService,
  ResignationRequestDto,
  ReviewResignationRequest,
  SubmitResignationRequest,
  UpdateResignationRequest
} from '../core/services/hr-api.service';
import { AuthService } from '../core/services/auth.service';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { TooltipModule } from 'primeng/tooltip';
import { TagModule } from 'primeng/tag';
import { CardModule } from 'primeng/card';
import { DialogModule } from 'primeng/dialog';
import { SelectModule } from 'primeng/select';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { MessageModule } from 'primeng/message';
import { InputSwitchModule } from 'primeng/inputswitch';
import { DatePickerModule } from 'primeng/datepicker';

type ResignationAction = 'approve' | 'reject';
type ResignationFormMode = 'create' | 'edit';

@Component({
  selector: 'app-hr-resignations',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    TableModule,
    ButtonModule,
    TooltipModule,
    TagModule,
    CardModule,
    DialogModule,
    SelectModule,
    InputTextModule,
    TextareaModule,
    MessageModule,
    InputSwitchModule,
    DatePickerModule
  ],
  templateUrl: './hr-resignations.component.html',
  styleUrls: ['./hr-resignations.component.scss']
})
export class HrResignationsComponent implements OnInit {
  private readonly hrApi = inject(HrApiService);
  private readonly auth = inject(AuthService);
  private readonly fb = inject(FormBuilder);

  private readonly managePermissions = ['hr.resignations.manage'];

  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly feedback = signal<string | null>(null);
  readonly resignations = signal<ResignationRequestDto[]>([]);
  readonly statusFilter = signal<string>('');
  readonly selectedResignation = signal<ResignationRequestDto | null>(null);

  readonly formVisible = signal(false);
  readonly formMode = signal<ResignationFormMode>('create');
  readonly formSubmitting = signal(false);

  readonly decisionVisible = signal(false);
  readonly decisionSubmitting = signal(false);
  readonly decisionMode = signal<ResignationAction>('approve');

  readonly currentEmployeeId = signal<string | null>(null);
  readonly currentEmployeeName = signal<string | null>(null);
  readonly currentEmployeeLoading = signal(false);
  readonly currentEmployeeError = signal<string | null>(null);

  readonly statusOptions: { label: string; value: string }[] = [
    { label: 'All statuses', value: '' },
    { label: 'Pending', value: 'Pending' },
    { label: 'Approved', value: 'Approved' },
    { label: 'Rejected', value: 'Rejected' }
  ];

  readonly hasResignations = computed(() => this.resignations().length > 0);
  readonly detailVisible = computed(() => this.selectedResignation() !== null);
  readonly canManageResignations = computed(() => this.hasAnyPermission(this.managePermissions));
  readonly managePermissionLabel = this.formatPermissionList(this.managePermissions);
  readonly canSubmitResignation = computed(() => !!this.currentEmployeeId());

  readonly decisionForm = this.fb.group({
    decisionNotes: this.fb.control<string | null>(null, [Validators.maxLength(2000)]),
    eligibleForRehire: this.fb.nonNullable.control(true)
  });

  readonly resignationForm = this.fb.group({
    effectiveDate: this.fb.control<Date | null>(null, Validators.required),
    reason: this.fb.control<string | null>(null, [Validators.maxLength(2000)])
  });

  readonly today = new Date();

  ngOnInit(): void {
    this.loadResignations();
    this.determineCurrentEmployee();
  }

  setStatusFilter(status: string): void {
    if (this.statusFilter() === status) {
      return;
    }

    this.statusFilter.set(status);
    this.loadResignations();
  }

  reload(): void {
    this.loadResignations();
  }

  clearFeedback(): void {
    this.feedback.set(null);
  }

  trackById(_: number, resignation: ResignationRequestDto): string {
    return resignation.id;
  }

  viewResignation(resignation: ResignationRequestDto): void {
    this.selectedResignation.set(resignation);
  }

  isPending(resignation: ResignationRequestDto): boolean {
    return this.statusLabel(resignation.status) === 'Pending';
  }

  decisionTooltip(resignation: ResignationRequestDto, action: ResignationAction): string {
    if (!this.canManageResignations()) {
      return `Requires ${this.managePermissionLabel}`;
    }

    if (!this.isPending(resignation)) {
      return 'Decision already recorded';
    }

    return action === 'approve' ? 'Approve resignation' : 'Reject resignation';
  }

  editTooltip(resignation: ResignationRequestDto): string {
    if (!this.canManageResignations()) {
      return `Requires ${this.managePermissionLabel}`;
    }

    if (!this.isPending(resignation)) {
      return 'Decision already recorded';
    }

    return 'Edit resignation request';
  }

  openDecisionDialog(mode: ResignationAction, event?: Event, resignation?: ResignationRequestDto): void {
    event?.stopPropagation();

    if (!this.canManageResignations()) {
      return;
    }

    if (resignation) {
      this.selectedResignation.set(resignation);
    }

    const current = this.selectedResignation();
    if (!current || !this.isPending(current)) {
      return;
    }

    this.decisionMode.set(mode);
    this.decisionForm.reset({
      decisionNotes: null,
      eligibleForRehire: mode === 'approve'
    });
    this.toggleEligibleControl(mode === 'reject');
    this.decisionVisible.set(true);
  }

  closeDecisionDialog(): void {
    if (this.decisionSubmitting()) {
      return;
    }

    this.decisionVisible.set(false);
    this.decisionForm.reset({ decisionNotes: null, eligibleForRehire: true });
    this.toggleEligibleControl(false);
  }

  submitDecision(): void {
    const current = this.selectedResignation();
    if (!current) {
      return;
    }

    if (this.decisionForm.invalid) {
      this.decisionForm.markAllAsTouched();
      return;
    }

    const formValue = this.decisionForm.getRawValue();
    const notes = formValue.decisionNotes?.trim();
    const decisionValue = this.decisionMode() === 'approve' ? 1 : 2;

    const payload: ReviewResignationRequest = {
      id: current.id,
      decision: decisionValue,
      decisionNotes: notes && notes.length ? notes : null,
      approvedByEmployeeId: null,
      eligibleForRehire: decisionValue === 1 ? formValue.eligibleForRehire : undefined
    };

    this.decisionSubmitting.set(true);

    this.hrApi.decideResignation(current.id, payload).subscribe({
      next: response => {
        const normalized = this.normalizeResignation(response);
        this.updateResignationInList(normalized);
        this.selectedResignation.set(normalized);
        this.feedback.set(
          decisionValue === 1 ? 'Resignation approved successfully.' : 'Resignation rejected successfully.'
        );
        this.decisionSubmitting.set(false);
        this.decisionVisible.set(false);
        this.toggleEligibleControl(false);
      },
      error: () => {
        this.decisionSubmitting.set(false);
        this.error.set('Unable to submit decision. Please try again.');
      }
    });
  }

  statusLabel(value?: string | number | null): string {
    if (value === null || value === undefined) {
      return 'Pending';
    }

    if (typeof value === 'number') {
      switch (value) {
        case 1:
          return 'Approved';
        case 2:
          return 'Rejected';
        default:
          return 'Pending';
      }
    }

    const normalized = value.toString().trim().toLowerCase();
    switch (normalized) {
      case 'approved':
        return 'Approved';
      case 'rejected':
        return 'Rejected';
      case 'pending':
        return 'Pending';
      default:
        return value.toString();
    }
  }

  statusSeverity(value?: string | number | null): 'info' | 'success' | 'warning' | 'danger' {
    const label = this.statusLabel(value).toLowerCase();
    switch (label) {
      case 'approved':
        return 'success';
      case 'rejected':
        return 'danger';
      case 'pending':
        return 'warning';
      default:
        return 'info';
    }
  }

  rehireSeverity(value?: boolean | null): 'info' | 'success' | 'danger' {
    if (value === null || value === undefined) {
      return 'info';
    }

    return value ? 'success' : 'danger';
  }

  rehireLabel(value?: boolean | null): string {
    if (value === null || value === undefined) {
      return 'Not evaluated';
    }

    return value ? 'Eligible for rehire' : 'Not eligible for rehire';
  }

  controlInvalid(controlName: keyof typeof this.decisionForm.controls): boolean {
    const control = this.decisionForm.controls[controlName];
    return control.invalid && control.touched;
  }

  actionLockClass(locked: boolean): Record<'is-locked', boolean> {
    return { 'is-locked': locked };
  }

  openCreateDialog(): void {
    const employeeId = this.currentEmployeeId();
    if (!employeeId) {
      this.currentEmployeeError.set('Unable to determine your employee profile. Please contact HR.');
      return;
    }

    this.selectedResignation.set(null);
    this.formMode.set('create');
    this.resignationForm.reset({
      effectiveDate: this.defaultEffectiveDate(),
      reason: null
    });
    this.formVisible.set(true);
  }

  openEditDialog(resignation: ResignationRequestDto, event?: Event): void {
    event?.stopPropagation();

    if (!this.canManageResignations() || !this.isPending(resignation)) {
      return;
    }

    this.selectedResignation.set(resignation);
    this.formMode.set('edit');

    const effectiveDate = resignation.effectiveDate ? new Date(resignation.effectiveDate) : this.defaultEffectiveDate();

    this.resignationForm.reset({
      effectiveDate,
      reason: resignation.reason ?? null
    });
    this.formVisible.set(true);
  }

  closeFormDialog(): void {
    if (this.formSubmitting()) {
      return;
    }

    this.formVisible.set(false);
    this.resignationForm.reset({ effectiveDate: null, reason: null });
  }

  submitResignationForm(): void {
    if (this.resignationForm.invalid) {
      this.resignationForm.markAllAsTouched();
      return;
    }

    const formValue = this.resignationForm.getRawValue();
    const reason = formValue.reason?.trim();
    const effectiveDate = formValue.effectiveDate ? this.toIsoDate(formValue.effectiveDate) : null;

    if (!effectiveDate) {
      this.resignationForm.controls.effectiveDate.setErrors({ invalid: true });
      return;
    }

    this.formSubmitting.set(true);

    if (this.formMode() === 'create') {
      const employeeId = this.currentEmployeeId();
      if (!employeeId) {
        this.formSubmitting.set(false);
        this.currentEmployeeError.set('Unable to determine your employee profile. Please contact HR.');
        return;
      }

      const payload: SubmitResignationRequest = {
        employeeId,
        effectiveDate,
        reason: reason && reason.length ? reason : null
      };

      this.hrApi.submitResignation(payload).subscribe({
        next: response => {
          const normalized = this.normalizeResignation(response);
          this.addOrUpdateResignation(normalized);
          this.selectedResignation.set(normalized);
          this.feedback.set('Resignation request submitted successfully.');
          this.formSubmitting.set(false);
          this.formVisible.set(false);
        },
        error: () => {
          this.formSubmitting.set(false);
          this.error.set('Unable to submit resignation request. Please try again.');
        }
      });
    } else {
      const current = this.selectedResignation();
      if (!current) {
        this.formSubmitting.set(false);
        return;
      }

      const payload: UpdateResignationRequest = {
        id: current.id,
        effectiveDate,
        reason: reason && reason.length ? reason : null
      };

      this.hrApi.updateResignation(current.id, payload).subscribe({
        next: response => {
          const normalized = this.normalizeResignation(response);
          this.addOrUpdateResignation(normalized);
          this.selectedResignation.set(normalized);
          this.feedback.set('Resignation request updated successfully.');
          this.formSubmitting.set(false);
          this.formVisible.set(false);
        },
        error: () => {
          this.formSubmitting.set(false);
          this.error.set('Unable to update resignation request. Please try again.');
        }
      });
    }
  }

  formControlInvalid(controlName: 'effectiveDate' | 'reason'): boolean {
    const control = this.resignationForm.controls[controlName];
    return control.invalid && control.touched;
  }

  private loadResignations(): void {
    this.loading.set(true);
    this.error.set(null);
    this.feedback.set(null);

    const status = this.statusFilter();
    this.hrApi.getResignations({ status: status || undefined }).subscribe({
      next: data => {
        const normalized = (data ?? []).map(item => this.normalizeResignation(item));
        const sorted = this.sortResignations(normalized);
        this.resignations.set(sorted);
        this.syncSelection(sorted);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Unable to load resignation requests.');
        this.loading.set(false);
      }
    });
  }

  private determineCurrentEmployee(): void {
    const session = this.auth.currentSession();
    if (!session?.userId) {
      this.currentEmployeeError.set('No authenticated user found.');
      return;
    }

    this.currentEmployeeLoading.set(true);
    this.hrApi.getEmployees().subscribe({
      next: employees => {
        const match = (employees ?? []).find(employee => employee.userId?.toLowerCase() === session.userId.toLowerCase());
        if (match) {
          this.currentEmployeeId.set(match.id);
          this.currentEmployeeName.set(match.fullName);
          this.currentEmployeeError.set(null);
        } else {
          this.currentEmployeeError.set('We could not find your employee profile. Please contact HR.');
        }
        this.currentEmployeeLoading.set(false);
      },
      error: () => {
        this.currentEmployeeLoading.set(false);
        this.currentEmployeeError.set('Unable to load employee information. Please try again.');
      }
    });
  }

  private normalizeResignation(resignation: ResignationRequestDto): ResignationRequestDto {
    const source = resignation as ResignationRequestDto & {
      submittedAt?: string;
      lastWorkingDay?: string;
      requestedAt?: string;
      effectiveDate?: string;
    };

    const requestedAt = source.requestedAt ?? source.submittedAt ?? resignation.requestedAt ?? new Date().toISOString();
    const effectiveDate = source.effectiveDate ?? source.lastWorkingDay ?? resignation.effectiveDate ?? requestedAt;

    return {
      ...resignation,
      status: this.statusLabel(resignation.status),
      requestedAt,
      effectiveDate,
      reason: resignation.reason ?? null,
      decisionNotes: resignation.decisionNotes ?? null,
      approvedByEmployeeId: resignation.approvedByEmployeeId ?? null,
      approvedByName: resignation.approvedByName ?? null,
      eligibleForRehire: resignation.eligibleForRehire ?? null
    };
  }

  private sortResignations(resignations: ResignationRequestDto[]): ResignationRequestDto[] {
    const getTime = (value?: string | null) => {
      if (!value) {
        return 0;
      }

      const time = new Date(value).getTime();
      return Number.isNaN(time) ? 0 : time;
    };

    return [...resignations].sort((a, b) => getTime(b.requestedAt) - getTime(a.requestedAt));
  }

  private updateResignationInList(updated: ResignationRequestDto): void {
    this.addOrUpdateResignation(updated);
  }

  private addOrUpdateResignation(resignation: ResignationRequestDto): void {
    const existing = this.resignations().find(item => item.id === resignation.id);
    const list = existing
      ? this.resignations().map(item => (item.id === resignation.id ? { ...item, ...resignation } : item))
      : [...this.resignations(), resignation];

    const sorted = this.sortResignations(list);
    this.resignations.set(sorted);
    this.syncSelection(sorted);
  }

  private defaultEffectiveDate(): Date {
    const date = new Date();
    date.setDate(date.getDate() + 14);
    date.setHours(0, 0, 0, 0);
    return date;
  }

  private toIsoDate(date: Date): string {
    const copy = new Date(date);
    copy.setHours(0, 0, 0, 0);
    return copy.toISOString();
  }

  private syncSelection(list: ResignationRequestDto[]): void {
    if (!list.length) {
      this.selectedResignation.set(null);
      return;
    }

    const current = this.selectedResignation();
    if (!current) {
      this.selectedResignation.set(list[0]);
      return;
    }

    const found = list.find(item => item.id === current.id);
    this.selectedResignation.set(found ?? list[0]);
  }

  private toggleEligibleControl(disabled: boolean): void {
    const control = this.decisionForm.controls.eligibleForRehire;
    if (disabled) {
      control.disable({ emitEvent: false });
    } else {
      control.enable({ emitEvent: false });
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
