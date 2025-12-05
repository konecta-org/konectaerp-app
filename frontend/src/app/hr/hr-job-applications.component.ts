import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { RouterModule } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import {
  CreateJobApplicationRequest,
  HrApiService,
  InterviewDto,
  JobApplicationDto,
  JobOpeningDto,
  UpdateJobApplicationRequest
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

type JobApplicationStatusCode = 'Submitted' | 'UnderReview' | 'Interviewing' | 'Offered' | 'Rejected' | 'Hired' | 'Withdrawn';
type ApplicationFormMode = 'create' | 'edit';
type ApplicationAction = 'manage' | 'delete';

type JobOpeningOption = {
  label: string;
  value: string;
  description?: string;
};

@Component({
  selector: 'app-hr-job-applications',
  standalone: true,
  imports: [
    CommonModule,
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
    RouterModule
  ],
  templateUrl: './hr-job-applications.component.html',
  styleUrls: ['./hr-job-applications.component.scss']
})
export class HrJobApplicationsComponent implements OnInit {
  private readonly hrApi = inject(HrApiService);
  private readonly auth = inject(AuthService);
  private readonly fb = inject(FormBuilder);

  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly feedback = signal<string | null>(null);
  readonly applications = signal<JobApplicationDto[]>([]);
  readonly jobOpenings = signal<JobOpeningDto[]>([]);
  readonly selectedApplication = signal<JobApplicationDto | null>(null);
  readonly applicationInterviews = signal<Record<string, InterviewDto[]>>({});

  readonly formVisible = signal(false);
  readonly formMode = signal<ApplicationFormMode>('create');
  readonly formSubmitting = signal(false);

  readonly deleteVisible = signal(false);
  readonly deleteSubmitting = signal(false);

  readonly hasApplications = computed(() => this.applications().length > 0);
  readonly detailVisible = computed(() => this.selectedApplication() !== null);

  private readonly managePermissions = ['hr.job-applications.manage'];
  private readonly deletePermissions = ['hr.job-applications.manage'];

  readonly canManageApplications = computed(() => this.hasAnyPermission(this.managePermissions));
  readonly canDeleteApplications = computed(() => this.hasAnyPermission(this.deletePermissions));

  readonly managePermissionLabel = this.formatPermissionList(this.managePermissions);
  readonly deletePermissionLabel = this.formatPermissionList(this.deletePermissions);

  readonly jobOpeningOptions = computed<JobOpeningOption[]>(() =>
    this.jobOpenings().map(opening => ({
      label: opening.title,
      value: opening.id,
      description: opening.departmentName || undefined
    }))
  );

  readonly statusOptions: { label: string; value: JobApplicationStatusCode }[] = [
    { label: 'Submitted', value: 'Submitted' },
    { label: 'Under Review', value: 'UnderReview' },
    { label: 'Interviewing', value: 'Interviewing' },
    { label: 'Offered', value: 'Offered' },
    { label: 'Hired', value: 'Hired' },
    { label: 'Rejected', value: 'Rejected' },
    { label: 'Withdrawn', value: 'Withdrawn' }
  ];

  readonly applicationForm = this.fb.group({
    jobOpeningId: this.fb.control<string | null>(null, Validators.required),
    candidateName: this.fb.nonNullable.control('', [Validators.required, Validators.maxLength(150)]),
    candidateEmail: this.fb.nonNullable.control('', [Validators.required, Validators.email, Validators.maxLength(200)]),
    candidatePhone: this.fb.control<string | null>(null, [Validators.maxLength(30)]),
    resumeUrl: this.fb.control<string | null>(null, [Validators.maxLength(2048)]),
    coverLetter: this.fb.control<string | null>(null, [Validators.maxLength(4000)]),
    status: this.fb.control<JobApplicationStatusCode>('Submitted', { nonNullable: true })
  });

  private readonly statusEnumMap: Record<JobApplicationStatusCode, number> = {
    Submitted: 0,
    UnderReview: 1,
    Interviewing: 2,
    Offered: 3,
    Rejected: 4,
    Hired: 5,
    Withdrawn: 6
  };

  private readonly statusLabelMap: Record<JobApplicationStatusCode, string> = {
    Submitted: 'Submitted',
    UnderReview: 'Under Review',
    Interviewing: 'Interviewing',
    Offered: 'Offered',
    Rejected: 'Rejected',
    Hired: 'Hired',
    Withdrawn: 'Withdrawn'
  };

  private readonly statusReverseMap = new Map<number, JobApplicationStatusCode>(
    Object.entries(this.statusEnumMap).map(([key, value]) => [value, key as JobApplicationStatusCode])
  );

  ngOnInit(): void {
    this.loadJobOpenings();
    this.loadApplications();
  }

  reload(): void {
    this.loadApplications();
  }

  trackByApplication(_: number, application: JobApplicationDto): string {
    return application.id;
  }

  viewApplication(application: JobApplicationDto): void {
    this.selectedApplication.set(application);
    this.loadInterviewsForApplication(application.id);
  }

  closeDetails(): void {
    this.selectedApplication.set(null);
  }

  openCreateDialog(): void {
    if (!this.canManageApplications()) {
      return;
    }

    this.formMode.set('create');
    this.applicationForm.reset({
      jobOpeningId: null,
      candidateName: '',
      candidateEmail: '',
      candidatePhone: null,
      resumeUrl: null,
      coverLetter: null,
      status: 'Submitted'
    });
    this.toggleStatusControl(true);
    this.formVisible.set(true);
  }

  openEditDialog(application: JobApplicationDto, event?: Event): void {
    event?.stopPropagation();

    if (!this.canManageApplications()) {
      return;
    }

    this.selectedApplication.set(application);
    this.formMode.set('edit');
    this.toggleStatusControl(false);

    this.applicationForm.reset({
      jobOpeningId: application.jobOpeningId,
      candidateName: application.candidateName,
      candidateEmail: application.candidateEmail,
      candidatePhone: application.candidatePhone ?? null,
      resumeUrl: application.resumeUrl ?? null,
      coverLetter: application.coverLetter ?? null,
      status: this.statusValueFromDto(application.status)
    });

    this.formVisible.set(true);
  }

  closeForm(): void {
    if (this.formSubmitting()) {
      return;
    }

    this.toggleStatusControl(false);
    this.formVisible.set(false);
  }

  submitForm(): void {
    if (this.applicationForm.invalid) {
      this.applicationForm.markAllAsTouched();
      return;
    }

    const formValue = this.applicationForm.getRawValue();
    const jobOpeningId = formValue.jobOpeningId;

    if (!jobOpeningId) {
      return;
    }

    const candidateName = formValue.candidateName.trim();
    const candidateEmail = formValue.candidateEmail.trim();
    const candidatePhone = formValue.candidatePhone?.trim();
    const resumeUrl = formValue.resumeUrl?.trim();
    const coverLetter = formValue.coverLetter?.trim();

    if (!candidateName || !candidateEmail) {
      this.applicationForm.controls.candidateName.markAsTouched();
      this.applicationForm.controls.candidateEmail.markAsTouched();
      return;
    }

    const createPayload: CreateJobApplicationRequest = {
      jobOpeningId,
      candidateName,
      candidateEmail,
      candidatePhone: candidatePhone && candidatePhone.length ? candidatePhone : undefined,
      resumeUrl: resumeUrl && resumeUrl.length ? resumeUrl : undefined,
      coverLetter: coverLetter && coverLetter.length ? coverLetter : undefined
    };

    this.formSubmitting.set(true);

    if (this.formMode() === 'create') {
      this.hrApi.createJobApplication(createPayload).subscribe({
        next: application => {
          const normalized = this.normalizeApplication(application);
          const sorted = this.sortApplications([...this.applications(), normalized]);
          this.applications.set(sorted);
          this.selectedApplication.set(normalized);
          this.syncSelection(sorted);
          this.feedback.set('Application submitted successfully.');
          this.formSubmitting.set(false);
          this.toggleStatusControl(false);
          this.formVisible.set(false);
        },
        error: () => {
          this.formSubmitting.set(false);
          this.error.set('Unable to submit application. Please try again.');
        }
      });
      return;
    }

    const current = this.selectedApplication();
    if (!current) {
      this.formSubmitting.set(false);
      return;
    }

    const statusCode = formValue.status ?? 'Submitted';
    const updatePayload: UpdateJobApplicationRequest = {
      id: current.id,
      jobOpeningId,
      candidateName,
      candidateEmail,
      candidatePhone: candidatePhone && candidatePhone.length ? candidatePhone : null,
      resumeUrl: resumeUrl && resumeUrl.length ? resumeUrl : null,
      coverLetter: coverLetter && coverLetter.length ? coverLetter : null,
      status: this.statusEnumMap[statusCode]
    };

    this.hrApi.updateJobApplication(current.id, updatePayload).subscribe({
      next: () => {
        const updated: JobApplicationDto = this.normalizeApplication({
          ...current,
          jobOpeningId: updatePayload.jobOpeningId,
          candidateName: updatePayload.candidateName,
          candidateEmail: updatePayload.candidateEmail,
          candidatePhone: updatePayload.candidatePhone ?? undefined,
          resumeUrl: updatePayload.resumeUrl ?? undefined,
          coverLetter: updatePayload.coverLetter ?? undefined,
          status: statusCode,
          updatedAt: new Date().toISOString()
        });

        this.updateApplicationInList(updated);
        this.selectedApplication.set(updated);
        this.feedback.set('Application updated successfully.');
        this.formSubmitting.set(false);
        this.toggleStatusControl(false);
        this.formVisible.set(false);
      },
      error: () => {
        this.formSubmitting.set(false);
        this.error.set('Unable to update application. Please try again.');
      }
    });
  }

  openDeleteDialog(application: JobApplicationDto, event?: Event): void {
    event?.stopPropagation();

    if (!this.canDeleteApplications()) {
      return;
    }

    this.selectedApplication.set(application);
    this.deleteVisible.set(true);
  }

  closeDeleteDialog(): void {
    if (this.deleteSubmitting()) {
      return;
    }

    this.deleteVisible.set(false);
  }

  confirmDelete(): void {
    const application = this.selectedApplication();
    if (!application) {
      return;
    }

    this.deleteSubmitting.set(true);

    this.hrApi.deleteJobApplication(application.id).subscribe({
      next: () => {
        const filtered = this.applications().filter(item => item.id !== application.id);
        this.applications.set(filtered);
        this.syncSelection(filtered);
        this.feedback.set('Application deleted successfully.');
        this.deleteSubmitting.set(false);
        this.deleteVisible.set(false);
      },
      error: () => {
        this.deleteSubmitting.set(false);
        this.error.set('Unable to delete application. Please try again.');
      }
    });
  }

  applicationStatusLabel(value?: string | number | null): string {
    if (value === null || value === undefined) {
      return 'Submitted';
    }

    if (typeof value === 'number') {
      const code = this.statusReverseMap.get(value);
      return code ? this.statusLabelMap[code] : 'Submitted';
    }

    const normalized = value.toString().replace(/\s+/g, '').toLowerCase();
    const match = Object.keys(this.statusEnumMap).find(
      key => key.toLowerCase() === normalized
    ) as JobApplicationStatusCode | undefined;

    if (match) {
      return this.statusLabelMap[match];
    }

    switch (normalized) {
      case 'underreview':
        return this.statusLabelMap.UnderReview;
      case 'interviewing':
        return this.statusLabelMap.Interviewing;
      case 'offered':
        return this.statusLabelMap.Offered;
      case 'rejected':
        return this.statusLabelMap.Rejected;
      case 'hired':
        return this.statusLabelMap.Hired;
      case 'withdrawn':
        return this.statusLabelMap.Withdrawn;
      default:
        return value.toString();
    }
  }

  applicationStatusSeverity(value?: string | number | null): 'success' | 'info' | 'warning' | 'danger' {
    const label = this.applicationStatusLabel(value).toLowerCase();
    switch (label) {
      case 'submitted':
        return 'info';
      case 'under review':
        return 'info';
      case 'interviewing':
        return 'warning';
      case 'offered':
      case 'hired':
        return 'success';
      case 'rejected':
        return 'danger';
      case 'withdrawn':
        return 'warning';
      default:
        return 'info';
    }
  }

  jobOpeningLabel(application: JobApplicationDto): string {
    const opening = this.jobOpenings().find(item => item.id === application.jobOpeningId);
    return opening?.title ?? application.jobTitle ?? 'Job application';
  }

  jobOpeningDepartment(application: JobApplicationDto): string | undefined {
    const opening = this.jobOpenings().find(item => item.id === application.jobOpeningId);
    return opening?.departmentName ?? undefined;
  }

  currentInterviews(): InterviewDto[] {
    const current = this.selectedApplication();
    if (!current) {
      return [];
    }
    return this.applicationInterviews()[current.id] ?? [];
  }

  interviewStatusLabel(value?: string | number | null): string {
    if (value === null || value === undefined) {
      return 'Scheduled';
    }

    if (typeof value === 'number') {
      const map: Record<number, string> = {
        0: 'Scheduled',
        1: 'Completed',
        2: 'Cancelled',
        3: 'Rescheduled',
        4: 'No-Show'
      };
      return map[value] ?? 'Scheduled';
    }

    const normalized = value.toString().toLowerCase();
    switch (normalized) {
      case 'scheduled':
        return 'Scheduled';
      case 'completed':
        return 'Completed';
      case 'cancelled':
        return 'Cancelled';
      case 'rescheduled':
        return 'Rescheduled';
      case 'no-show':
      case 'noshow':
        return 'No-Show';
      default:
        return value.toString();
    }
  }

  interviewStatusSeverity(value?: string | number | null): 'info' | 'success' | 'danger' | 'warning' {
    const label = this.interviewStatusLabel(value).toLowerCase();
    switch (label) {
      case 'completed':
        return 'success';
      case 'cancelled':
        return 'danger';
      case 'no-show':
      case 'rescheduled':
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
      const map: Record<number, string> = {
        0: 'In Person',
        1: 'Virtual',
        2: 'Phone'
      };
      return map[value] ?? 'In Person';
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

  controlInvalid(controlName: keyof typeof this.applicationForm.controls): boolean {
    const control = this.applicationForm.controls[controlName];
    return control.invalid && control.touched;
  }

  tooltipFor(action: ApplicationAction, locked: boolean): string {
    if (!locked) {
      if (action === 'delete') {
        return 'Delete application';
      }

      return this.formMode() === 'create' ? 'Add application' : 'Edit application';
    }

    const map: Record<ApplicationAction, string> = {
      manage: this.managePermissionLabel,
      delete: this.deletePermissionLabel
    };

    return `Requires ${map[action]}`;
  }

  actionLockClass(locked: boolean): Record<'is-locked', boolean> {
    return { 'is-locked': locked };
  }

  private loadApplications(): void {
    this.loading.set(true);
    this.error.set(null);

    this.hrApi.getJobApplications().subscribe({
      next: data => {
        const normalized = (data ?? []).map(application => this.normalizeApplication(application));
        const sorted = this.sortApplications(normalized);
        this.applications.set(sorted);
        this.syncSelection(sorted);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Unable to load job applications.');
        this.loading.set(false);
      }
    });
  }

  private loadJobOpenings(): void {
    this.hrApi.getJobOpenings().subscribe({
      next: openings => {
        this.jobOpenings.set(openings ?? []);
        this.refreshNormalizedApplications();
      },
      error: () => {
        // silent failure
      }
    });
  }

  private loadInterviewsForApplication(applicationId: string): void {
    if (this.applicationInterviews()[applicationId]) {
      return;
    }

    this.hrApi.getInterviews({ jobApplicationId: applicationId }).subscribe({
      next: data => {
        const map = { ...this.applicationInterviews() };
        map[applicationId] = (data ?? []).map(interview => this.normalizeInterview(interview));
        this.applicationInterviews.set(map);
      },
      error: () => {
        // silent failure
      }
    });
  }

  private normalizeApplication(application: JobApplicationDto): JobApplicationDto {
    const opening = this.jobOpenings().find(item => item.id === application.jobOpeningId);
    return {
      ...application,
      jobTitle: application.jobTitle ?? opening?.title ?? application.jobTitle
    };
  }

  private normalizeInterview(interview: InterviewDto): InterviewDto {
    const application = this.applications().find(app => app.id === interview.jobApplicationId);
    return {
      ...interview,
      candidateName: application?.candidateName ?? interview.candidateName,
      jobApplicationTitle: application?.jobTitle ?? interview.jobApplicationTitle,
      status: this.interviewStatusLabel(interview.status),
      mode: this.interviewModeLabel(interview.mode)
    };
  }

  private refreshNormalizedApplications(): void {
    if (!this.applications().length) {
      return;
    }

    const normalized = this.sortApplications(this.applications().map(application => this.normalizeApplication(application)));
    this.applications.set(normalized);
    this.syncSelection(normalized);
  }

  private sortApplications(applications: JobApplicationDto[]): JobApplicationDto[] {
    return [...applications].sort(
      (a, b) => new Date(b.appliedAt).getTime() - new Date(a.appliedAt).getTime()
    );
  }

  private updateApplicationInList(updated: JobApplicationDto): void {
    const list = this.sortApplications(
      this.applications().map(application => (application.id === updated.id ? { ...application, ...updated } : application))
    );
    this.applications.set(list);
    this.syncSelection(list);
  }

  private syncSelection(list: JobApplicationDto[]): void {
    const current = this.selectedApplication();

    if (!list.length) {
      this.selectedApplication.set(null);
      return;
    }

    if (!current) {
      this.selectedApplication.set(list[0]);
      return;
    }

    const found = list.find(application => application.id === current.id);
    this.selectedApplication.set(found ?? list[0]);
  }

  private statusValueFromDto(value?: string | number | null): JobApplicationStatusCode {
    if (typeof value === 'number') {
      const code = this.statusReverseMap.get(value);
      return code ?? 'Submitted';
    }

    if (!value) {
      return 'Submitted';
    }

    const normalized = value.toString().replace(/\s+/g, '').toLowerCase();
    const match = Object.keys(this.statusEnumMap).find(
      key => key.toLowerCase() === normalized
    ) as JobApplicationStatusCode | undefined;

    if (match) {
      return match;
    }

    switch (normalized) {
      case 'underreview':
        return 'UnderReview';
      case 'interviewing':
        return 'Interviewing';
      case 'offered':
        return 'Offered';
      case 'rejected':
        return 'Rejected';
      case 'hired':
        return 'Hired';
      case 'withdrawn':
        return 'Withdrawn';
      default:
        return 'Submitted';
    }
  }

  private toggleStatusControl(disabled: boolean): void {
    const control = this.applicationForm.controls.status;
    if (disabled) {
      control.disable({ emitEvent: false });
    } else {
      control.enable({ emitEvent: false });
    }
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
