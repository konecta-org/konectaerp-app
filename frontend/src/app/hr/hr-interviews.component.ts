import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import {
  EmployeeDto,
  HrApiService,
  InterviewDto,
  JobApplicationDto,
  ScheduleInterviewRequest,
  UpdateInterviewRequest
} from '../core/services/hr-api.service';
import { AuthService } from '../core/services/auth.service';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { TooltipModule } from 'primeng/tooltip';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';
import { DialogModule } from 'primeng/dialog';
import { DatePickerModule } from 'primeng/datepicker';
import { InputTextModule } from 'primeng/inputtext';
import { MessageModule } from 'primeng/message';
import { SelectModule } from 'primeng/select';
import { TextareaModule } from 'primeng/textarea';

type InterviewModeCode = 'InPerson' | 'Virtual' | 'Phone';
type InterviewStatusCode = 'Scheduled' | 'Completed' | 'Cancelled' | 'NoShow';
type InterviewFormMode = 'create' | 'edit';
type InterviewAction = 'manage' | 'delete';

type ApplicationOption = {
  label: string;
  value: string | null;
  description?: string;
  disabled?: boolean;
};

@Component({
  selector: 'app-hr-interviews',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TableModule,
    ButtonModule,
    TooltipModule,
    CardModule,
    TagModule,
    DialogModule,
    DatePickerModule,
    InputTextModule,
    MessageModule,
    SelectModule,
    TextareaModule
  ],
  templateUrl: './hr-interviews.component.html',
  styleUrls: ['./hr-interviews.component.scss']
})
export class HrInterviewsComponent implements OnInit {
  private readonly hrApi = inject(HrApiService);
  private readonly auth = inject(AuthService);
  private readonly fb = inject(FormBuilder);

  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly feedback = signal<string | null>(null);
  readonly interviews = signal<InterviewDto[]>([]);
  readonly jobApplications = signal<JobApplicationDto[]>([]);
  readonly employees = signal<EmployeeDto[]>([]);
  readonly selectedInterview = signal<InterviewDto | null>(null);

  readonly formVisible = signal(false);
  readonly formMode = signal<InterviewFormMode>('create');
  readonly formSubmitting = signal(false);

  readonly deleteVisible = signal(false);
  readonly deleteSubmitting = signal(false);

  readonly hasInterviews = computed(() => this.interviews().length > 0);
  readonly detailVisible = computed(() => this.selectedInterview() !== null);

  private readonly managePermissions = ['hr.interviews.manage'];
  private readonly deletePermissions = ['hr.interviews.manage'];

  readonly canManageInterviews = computed(() => this.hasAnyPermission(this.managePermissions));
  readonly canDeleteInterviews = computed(() => this.hasAnyPermission(this.deletePermissions));

  readonly managePermissionLabel = this.formatPermissionList(this.managePermissions);
  readonly deletePermissionLabel = this.formatPermissionList(this.deletePermissions);

  readonly applicationOptions = computed<ApplicationOption[]>(() =>
    [...this.jobApplications()]
      .sort((a, b) => new Date(b.appliedAt).getTime() - new Date(a.appliedAt).getTime())
      .map(application => ({
        label: `${application.jobTitle ?? 'Job Application'} · ${application.candidateName}`,
        value: application.id,
        description: application.candidateEmail ?? undefined
      }))
  );

  readonly interviewerOptions = computed(() =>
    this.employees().map(employee => ({ label: employee.fullName, value: employee.id }))
  );

  readonly modeOptions: { label: string; value: InterviewModeCode }[] = [
    { label: 'In Person', value: 'InPerson' },
    { label: 'Virtual', value: 'Virtual' },
    { label: 'Phone', value: 'Phone' }
  ];

  readonly statusOptions: { label: string; value: InterviewStatusCode }[] = [
    { label: 'Scheduled', value: 'Scheduled' },
    { label: 'Completed', value: 'Completed' },
    { label: 'Cancelled', value: 'Cancelled' },
    { label: 'No-Show', value: 'NoShow' }
  ];

  private readonly modeEnumMap: Record<InterviewModeCode, number> = {
    InPerson: 0,
    Virtual: 1,
    Phone: 2
  };

  private readonly statusEnumMap: Record<InterviewStatusCode, number> = {
    Scheduled: 0,
    Completed: 1,
    Cancelled: 2,
    NoShow: 3
  };

  readonly interviewForm = this.fb.group({
    jobApplicationId: this.fb.control<string | null>(null, Validators.required),
    interviewerEmployeeId: this.fb.control<string | null>(null),
    scheduledAt: this.fb.control<Date | null>(null, Validators.required),
    mode: this.fb.control<InterviewModeCode>('InPerson', { nonNullable: true }),
    status: this.fb.control<InterviewStatusCode>('Scheduled', { nonNullable: true }),
    location: this.fb.control<string | null>(null, [Validators.maxLength(250)]),
    notes: this.fb.control<string | null>(null, [Validators.maxLength(4000)])
  });

  ngOnInit(): void {
    this.loadJobApplications();
    this.loadInterviews();
    this.loadEmployees();
  }

  reload(): void {
    this.loadInterviews();
  }

  trackByInterview(_: number, interview: InterviewDto): string {
    return interview.id;
  }

  viewInterview(interview: InterviewDto): void {
    this.selectedInterview.set(interview);
  }

  closeDetails(): void {
    this.selectedInterview.set(null);
  }

  openScheduleDialog(): void {
    if (!this.canManageInterviews()) {
      return;
    }

    this.toggleJobApplicationControl(false);
    this.formMode.set('create');
    this.interviewForm.reset({
      jobApplicationId: null,
      interviewerEmployeeId: null,
      scheduledAt: null,
      mode: 'InPerson',
      status: 'Scheduled',
      location: null,
      notes: null
    });
    this.formVisible.set(true);
  }

  openEditDialog(interview: InterviewDto, event?: Event): void {
    event?.stopPropagation();

    if (!this.canManageInterviews()) {
      return;
    }

    this.selectedInterview.set(interview);
    this.formMode.set('edit');
    this.toggleJobApplicationControl(true);
    this.interviewForm.reset({
      jobApplicationId: interview.jobApplicationId,
      interviewerEmployeeId: interview.interviewerEmployeeId ?? null,
      scheduledAt: interview.scheduledAt ? new Date(interview.scheduledAt) : null,
      mode: this.modeValueFromDto(interview.mode),
      status: this.statusValueFromDto(interview.status),
      location: interview.location ?? null,
      notes: interview.notes ?? null
    });
    this.formVisible.set(true);
  }

  closeForm(): void {
    if (this.formSubmitting()) {
      return;
    }

    this.toggleJobApplicationControl(false);
    this.formVisible.set(false);
  }

  submitForm(): void {
    if (this.interviewForm.invalid) {
      this.interviewForm.markAllAsTouched();
      return;
    }

    const formValue = this.interviewForm.getRawValue();
    const scheduledAt = formValue.scheduledAt;
    const location = formValue.location?.trim();
    const notes = formValue.notes?.trim();

    if (!scheduledAt) {
      return;
    }

    const payloadBase: ScheduleInterviewRequest = {
      jobApplicationId: formValue.jobApplicationId!,
      interviewerEmployeeId: formValue.interviewerEmployeeId ?? undefined,
      scheduledAt: scheduledAt.toISOString(),
      mode: this.modeEnumMap[formValue.mode ?? 'InPerson'],
      location: location && location.length ? location : undefined,
      notes: notes && notes.length ? notes : undefined
    };

    this.formSubmitting.set(true);

    if (this.formMode() === 'create') {
      this.hrApi.scheduleInterview(payloadBase).subscribe({
        next: interview => {
          const normalized = this.normalizeInterview(interview);
          const updated = this.sortInterviews([...this.interviews(), normalized]);
          this.interviews.set(updated);
          this.selectedInterview.set(normalized);
          this.syncSelection(updated);
          this.feedback.set('Interview scheduled successfully.');
          this.formSubmitting.set(false);
          this.toggleJobApplicationControl(false);
          this.formVisible.set(false);
        },
        error: () => {
          this.formSubmitting.set(false);
          this.error.set('Unable to schedule interview. Please try again.');
        }
      });
      return;
    }

    const current = this.selectedInterview();
    if (!current) {
      this.formSubmitting.set(false);
      return;
    }

    const payload: UpdateInterviewRequest = {
      id: current.id,
      interviewerEmployeeId: payloadBase.interviewerEmployeeId ?? null,
      scheduledAt: payloadBase.scheduledAt,
      mode: payloadBase.mode,
      location: payloadBase.location ?? null,
      status: this.statusEnumMap[formValue.status ?? 'Scheduled'],
      notes: payloadBase.notes ?? null
    };

    this.hrApi.updateInterview(current.id, payload).subscribe({
      next: () => {
        const updated: InterviewDto = {
          ...current,
          interviewerEmployeeId: payload.interviewerEmployeeId ?? null,
          interviewerName:
            payload.interviewerEmployeeId
              ? this.employees().find(emp => emp.id === payload.interviewerEmployeeId)?.fullName ?? current.interviewerName
              : null,
          scheduledAt: payloadBase.scheduledAt,
          mode: this.interviewModeLabel(payload.mode ?? current.mode),
          status: this.interviewStatusLabel(payload.status ?? current.status),
          location: payload.location ?? null,
          notes: payload.notes ?? null
        };
        const normalized = this.normalizeInterview(updated);
        this.updateInterviewInList(normalized);
        this.selectedInterview.set(normalized);
        this.feedback.set('Interview updated successfully.');
        this.formSubmitting.set(false);
        this.toggleJobApplicationControl(false);
        this.formVisible.set(false);
      },
      error: () => {
        this.formSubmitting.set(false);
        this.error.set('Unable to update interview. Please try again.');
      }
    });
  }

  openDeleteDialog(interview: InterviewDto, event?: Event): void {
    event?.stopPropagation();

    if (!this.canDeleteInterviews()) {
      return;
    }

    this.selectedInterview.set(interview);
    this.deleteVisible.set(true);
  }

  closeDeleteDialog(): void {
    if (this.deleteSubmitting()) {
      return;
    }

    this.deleteVisible.set(false);
  }

  confirmDelete(): void {
    const interview = this.selectedInterview();
    if (!interview) {
      return;
    }

    this.deleteSubmitting.set(true);

    this.hrApi.deleteInterview(interview.id).subscribe({
      next: () => {
        const filtered = this.interviews().filter(item => item.id !== interview.id);
        this.interviews.set(filtered);
        this.syncSelection(filtered);
        this.feedback.set('Interview deleted successfully.');
        this.deleteSubmitting.set(false);
        this.deleteVisible.set(false);
      },
      error: () => {
        this.deleteSubmitting.set(false);
        this.error.set('Unable to delete interview. Please try again.');
      }
    });
  }

  interviewStatusLabel(value?: string | number | null): string {
    if (value === null || value === undefined) {
      return 'Scheduled';
    }

    if (typeof value === 'number') {
      const match = Object.entries(this.statusEnumMap).find(([, enumValue]) => enumValue === value);
      return match ? this.interviewStatusLabel(match[0]) : 'Scheduled';
    }

    const normalized = value.toString().toLowerCase();
    const found = this.statusOptions.find(option => option.value.toLowerCase() === normalized);
    if (found) {
      return found.label;
    }

    switch (normalized) {
      case 'noshow':
        return 'No-Show';
      default:
        return value.toString();
    }
  }

  interviewStatusSeverity(value?: string | number | null): 'success' | 'warning' | 'danger' | 'info' {
    const label = this.interviewStatusLabel(value).toLowerCase();
    switch (label) {
      case 'scheduled':
        return 'info';
      case 'completed':
        return 'success';
      case 'cancelled':
        return 'danger';
      case 'no-show':
        return 'warning';
      default:
        return 'info';
    }
  }

  interviewModeLabel(value?: string | number | null): string {
    if (value === null || value === undefined) {
      return 'In Person';
    }

    if (typeof value === 'number') {
      const match = Object.entries(this.modeEnumMap).find(([, enumValue]) => enumValue === value);
      return match ? this.interviewModeLabel(match[0]) : 'In Person';
    }

    const normalized = value.toString().toLowerCase();
    switch (normalized) {
      case 'inperson':
      case 'in person':
        return 'In Person';
      case 'virtual':
        return 'Virtual';
      case 'phone':
        return 'Phone';
      default:
        return value.toString();
    }
  }

  clearFeedback(): void {
    this.feedback.set(null);
  }

  controlInvalid(controlName: keyof typeof this.interviewForm.controls): boolean {
    const control = this.interviewForm.controls[controlName];
    return control.invalid && control.touched;
  }

  tooltipFor(action: InterviewAction, locked: boolean): string {
    if (!locked) {
      if (action === 'delete') {
        return 'Delete interview';
      }

      return this.formMode() === 'create' ? 'Schedule interview' : 'Edit interview';
    }

    const map: Record<InterviewAction, string> = {
      manage: this.managePermissionLabel,
      delete: this.deletePermissionLabel
    };

    return `Requires ${map[action]}`;
  }

  actionLockClass(locked: boolean): Record<'is-locked', boolean> {
    return { 'is-locked': locked };
  }

  private loadInterviews(): void {
    this.loading.set(true);
    this.error.set(null);

    this.hrApi.getInterviews().subscribe({
      next: data => {
        const normalized = (data ?? []).map(interview => this.normalizeInterview(interview));
        const sorted = this.sortInterviews(normalized);
        this.interviews.set(sorted);
        this.syncSelection(sorted);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Unable to load interviews.');
        this.loading.set(false);
      }
    });
  }

  private loadJobApplications(): void {
    this.hrApi.getJobApplications().subscribe({
      next: applications => {
        this.jobApplications.set(applications ?? []);
        this.refreshNormalizedInterviews();
      },
      error: () => {
        // silent failure
      }
    });
  }

  private loadEmployees(): void {
    this.hrApi.getEmployees().subscribe({
      next: employees => {
        this.employees.set(employees ?? []);
        this.refreshNormalizedInterviews();
      },
      error: () => {
        // silent failure
      }
    });
  }

  private normalizeInterview(interview: InterviewDto): InterviewDto {
    const jobApplication = this.jobApplications().find(app => app.id === interview.jobApplicationId);
    return {
      ...interview,
      jobApplicationTitle: jobApplication?.jobTitle ?? interview.jobApplicationTitle,
      candidateName: jobApplication?.candidateName ?? interview.candidateName,
      interviewerName:
        interview.interviewerEmployeeId
          ? this.employees().find(emp => emp.id === interview.interviewerEmployeeId)?.fullName ?? interview.interviewerName
          : interview.interviewerName,
      status: this.interviewStatusLabel(interview.status),
      mode: this.interviewModeLabel(interview.mode)
    };
  }

  private refreshNormalizedInterviews(): void {
    if (!this.interviews().length) {
      return;
    }

    const normalized = this.sortInterviews(this.interviews().map(interview => this.normalizeInterview(interview)));
    this.interviews.set(normalized);
    this.syncSelection(normalized);
  }

  private toggleJobApplicationControl(disabled: boolean): void {
    const control = this.interviewForm.controls.jobApplicationId;
    if (disabled) {
      control.disable({ emitEvent: false });
    } else {
      control.enable({ emitEvent: false });
    }
  }

  private modeValueFromDto(value?: string | number | null): InterviewModeCode {
    if (typeof value === 'number') {
      const entry = Object.entries(this.modeEnumMap).find(([, enumValue]) => enumValue === value);
      return (entry?.[0] as InterviewModeCode) ?? 'InPerson';
    }

    if (!value) {
      return 'InPerson';
    }

    const normalized = value.toString().toLowerCase();
    switch (normalized) {
      case 'virtual':
        return 'Virtual';
      case 'phone':
        return 'Phone';
      default:
        return 'InPerson';
    }
  }

  private statusValueFromDto(value?: string | number | null): InterviewStatusCode {
    if (typeof value === 'number') {
      const entry = Object.entries(this.statusEnumMap).find(([, enumValue]) => enumValue === value);
      return (entry?.[0] as InterviewStatusCode) ?? 'Scheduled';
    }

    if (!value) {
      return 'Scheduled';
    }

    const normalized = value.toString().toLowerCase();
    switch (normalized) {
      case 'completed':
        return 'Completed';
      case 'cancelled':
        return 'Cancelled';
      case 'no-show':
      case 'noshow':
        return 'NoShow';
      default:
        return 'Scheduled';
    }
  }

  private sortInterviews(interviews: InterviewDto[]): InterviewDto[] {
    return [...interviews].sort((a, b) => new Date(b.scheduledAt).getTime() - new Date(a.scheduledAt).getTime());
  }

  private updateInterviewInList(updated: InterviewDto): void {
    const updatedList = this.sortInterviews(
      this.interviews().map(interview => (interview.id === updated.id ? { ...interview, ...updated } : interview))
    );
    this.interviews.set(updatedList);
    this.syncSelection(updatedList);
  }

  private syncSelection(list: InterviewDto[]): void {
    const current = this.selectedInterview();

    if (!list.length) {
      this.selectedInterview.set(null);
      return;
    }

    if (!current) {
      this.selectedInterview.set(list[0]);
      return;
    }

    const next = list.find(interview => interview.id === current.id);
    this.selectedInterview.set(next ?? list[0]);
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
