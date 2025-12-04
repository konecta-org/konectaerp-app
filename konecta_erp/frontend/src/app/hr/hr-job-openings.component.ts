import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../core/services/auth.service';
import {
  CreateJobOpeningRequest,
  HrApiService,
  JobOpeningDto,
  UpdateJobOpeningRequest
} from '../core/services/hr-api.service';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { TooltipModule } from 'primeng/tooltip';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';
import { DividerModule } from 'primeng/divider';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { InputTextarea } from 'primeng/inputtextarea';
import { DropdownModule } from 'primeng/dropdown';
import { DatePickerModule } from 'primeng/datepicker';
import { InputNumberModule } from 'primeng/inputnumber';
import { MessageModule } from 'primeng/message';
import { DatePipe } from '@angular/common';
import { DepartmentDto } from '../core/services/hr-api.service';

type JobOpeningFormMode = 'create' | 'edit';
type JobOpeningAction = 'manage' | 'delete';
type EmploymentTypeOption = { label: string; value: EmploymentTypeCode };
type JobStatusOption = { label: string; value: JobStatusCode };

type EmploymentTypeCode = 'FullTime' | 'PartTime' | 'Contract' | 'Internship' | 'Temporary';
type JobStatusCode = 'Draft' | 'Open' | 'Closed' | 'Filled' | 'Cancelled';

@Component({
  selector: 'app-hr-job-openings',
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
    DialogModule,
    InputTextModule,
    InputTextarea,
    DropdownModule,
    DatePickerModule,
    InputNumberModule,
    MessageModule,
    DatePipe
  ],
  templateUrl: './hr-job-openings.component.html',
  styleUrls: ['./hr-job-openings.component.scss']
})
export class HrJobOpeningsComponent implements OnInit {
  private readonly hrApi = inject(HrApiService);
  private readonly auth = inject(AuthService);
  private readonly fb = inject(FormBuilder);

  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly feedback = signal<string | null>(null);
  readonly openings = signal<JobOpeningDto[]>([]);
  readonly departments = signal<DepartmentDto[]>([]);
  readonly selectedOpening = signal<JobOpeningDto | null>(null);

  readonly departmentOptions = computed(() =>
    this.departments().map(department => ({ label: department.departmentName, value: department.departmentId }))
  );

  readonly formVisible = signal(false);
  readonly formMode = signal<JobOpeningFormMode>('create');
  readonly formSubmitting = signal(false);

  readonly deleteVisible = signal(false);
  readonly deleteSubmitting = signal(false);

  readonly hasOpenings = computed(() => this.openings().length > 0);
  readonly detailVisible = computed(() => this.selectedOpening() !== null);

  private readonly managePermissions = ['hr.job-openings.manage'];
  private readonly deletePermissions = ['hr.job-openings.manage'];

  readonly managePermissionLabel = this.formatPermissionList(this.managePermissions);
  readonly deletePermissionLabel = this.formatPermissionList(this.deletePermissions);

  readonly canManageOpenings = computed(() => this.hasAnyPermission(this.managePermissions));
  readonly canDeleteOpenings = computed(() => this.hasAnyPermission(this.deletePermissions));

  readonly employmentTypeOptions: EmploymentTypeOption[] = [
    { label: 'Full Time', value: 'FullTime' },
    { label: 'Part Time', value: 'PartTime' },
    { label: 'Contract', value: 'Contract' },
    { label: 'Internship', value: 'Internship' },
    { label: 'Temporary', value: 'Temporary' }
  ];

  readonly statusOptions: JobStatusOption[] = [
    { label: 'Draft', value: 'Draft' },
    { label: 'Open', value: 'Open' },
    { label: 'Closed', value: 'Closed' },
    { label: 'Filled', value: 'Filled' },
    { label: 'Cancelled', value: 'Cancelled' }
  ];

  private readonly employmentTypeEnumMap: Record<EmploymentTypeCode, number> = {
    FullTime: 0,
    PartTime: 1,
    Contract: 2,
    Internship: 3,
    Temporary: 4
  };

  private readonly statusEnumMap: Record<JobStatusCode, number> = {
    Draft: 0,
    Open: 1,
    Closed: 2,
    Filled: 3,
    Cancelled: 4
  };

  readonly jobForm = this.fb.group({
    title: this.fb.control('', { validators: [Validators.required, Validators.maxLength(150)], nonNullable: true }),
    departmentId: this.fb.control<string | null>(null),
    employmentType: this.fb.control<EmploymentTypeCode>(this.defaultEmploymentType(), { nonNullable: true }),
    status: this.fb.control<JobStatusCode>(this.defaultStatus(), { nonNullable: true }),
    location: this.fb.control<string | null>(null, [Validators.maxLength(150)]),
    salaryMin: this.fb.control<number | null>(null, [Validators.min(0)]),
    salaryMax: this.fb.control<number | null>(null, [Validators.min(0)]),
    closingDate: this.fb.control<Date | null>(null),
    description: this.fb.control<string | null>(null, [Validators.maxLength(4000)]),
    requirements: this.fb.control<string | null>(null, [Validators.maxLength(4000)])
  });

  ngOnInit(): void {
    this.loadDepartments();
    this.loadOpenings();
  }

  reload(): void {
    this.loadOpenings();
  }

  trackByOpening(_: number, opening: JobOpeningDto): string {
    return opening.id;
  }

  viewOpening(opening: JobOpeningDto): void {
    this.selectedOpening.set(opening);
  }

  closeDetails(): void {
    this.selectedOpening.set(null);
  }

  tooltipFor(action: JobOpeningAction, locked: boolean): string {
    if (!locked) {
      if (action === 'delete') {
        return 'Delete opening';
      }
      return 'Edit opening';
    }

    const map: Record<JobOpeningAction, string> = {
      manage: this.managePermissionLabel,
      delete: this.deletePermissionLabel
    };

    return `Requires ${map[action]}`;
  }

  actionLockClass(locked: boolean): Record<'is-locked', boolean> {
    return { 'is-locked': locked };
  }

  clearFeedback(): void {
    this.feedback.set(null);
  }

  jobControlInvalid(controlName: keyof typeof this.jobForm.controls): boolean {
    const control = this.jobForm.controls[controlName];
    return control.invalid && control.touched;
  }

  openCreateOpening(): void {
    if (!this.canManageOpenings()) {
      return;
    }

    this.formMode.set('create');
    this.jobForm.reset({
      title: '',
      departmentId: null,
      employmentType: this.defaultEmploymentType(),
      status: this.defaultStatus(),
      location: null,
      salaryMin: null,
      salaryMax: null,
      closingDate: null,
      description: null,
      requirements: null
    });
    this.formVisible.set(true);
  }

  openEditOpening(opening: JobOpeningDto, event?: Event): void {
    event?.stopPropagation();

    if (!this.canManageOpenings()) {
      return;
    }

    this.selectedOpening.set(opening);
    this.formMode.set('edit');
    this.jobForm.reset({
      title: opening.title,
      departmentId: opening.departmentId || null,
      employmentType: this.employmentTypeValueFromDto(opening.employmentType),
      status: this.statusValueFromDto(opening.status),
      location: opening.location ?? null,
      salaryMin: opening.salaryMin ?? null,
      salaryMax: opening.salaryMax ?? null,
      closingDate: opening.closingDate ? new Date(opening.closingDate) : null,
      description: opening.description ?? null,
      requirements: opening.requirements ?? null
    });
    this.formVisible.set(true);
  }

  closeJobForm(): void {
    if (this.formSubmitting()) {
      return;
    }
    this.formVisible.set(false);
  }

  submitJobForm(): void {
    if (this.jobForm.invalid) {
      this.jobForm.markAllAsTouched();
      return;
    }

    const formValue = this.jobForm.getRawValue();
    const salaryMin = formValue.salaryMin ?? undefined;
    const salaryMax = formValue.salaryMax ?? undefined;

    if (salaryMin !== undefined && salaryMax !== undefined && salaryMin > salaryMax) {
      this.jobForm.controls.salaryMin.setErrors({ salaryRange: true });
      this.jobForm.controls.salaryMax.setErrors({ salaryRange: true });
      return;
    }

    const employmentTypeValue = formValue.employmentType ?? this.defaultEmploymentType();
    const statusValue = formValue.status ?? this.defaultStatus();
    const location = formValue.location?.trim();
    const description = formValue.description?.trim();
    const requirements = formValue.requirements?.trim();

    const employmentTypeNumber = this.employmentTypeEnumMap[employmentTypeValue];
    const statusNumber = this.statusEnumMap[statusValue];

    const payloadBase: CreateJobOpeningRequest = {
      title: formValue.title.trim(),
      departmentId: formValue.departmentId ?? undefined,
      employmentType: employmentTypeNumber,
      status: statusNumber,
      location: location && location.length ? location : undefined,
      salaryMin: salaryMin,
      salaryMax: salaryMax,
      closingDate: formValue.closingDate ? formValue.closingDate.toISOString() : undefined,
      description: description && description.length ? description : undefined,
      requirements: requirements && requirements.length ? requirements : undefined
    };

    this.formSubmitting.set(true);

    if (this.formMode() === 'create') {
      this.hrApi.createJobOpening(payloadBase).subscribe({
        next: opening => {
          const normalized = this.normalizeOpening(opening);
          const updatedList = this.sortOpenings([...this.openings(), normalized]);
          this.openings.set(updatedList);
          this.selectedOpening.set(normalized);
          this.syncSelection(updatedList);
          this.feedback.set('Job opening created successfully.');
          this.formSubmitting.set(false);
          this.formVisible.set(false);
        },
        error: () => {
          this.formSubmitting.set(false);
          this.error.set('Unable to create job opening. Please try again.');
        }
      });
      return;
    }

    const current = this.selectedOpening();
    if (!current) {
      this.formSubmitting.set(false);
      return;
    }

    const payload: UpdateJobOpeningRequest = { ...payloadBase, id: current.id };

    this.hrApi.updateJobOpening(current.id, payload).subscribe({
      next: () => {
        const employmentTypeLabel = this.employmentTypeLabel(payload.employmentType);
        const statusLabel = this.statusLabel(payload.status);
        const updated: JobOpeningDto = {
          ...current,
          title: payload.title,
          departmentId: payload.departmentId ?? current.departmentId,
          departmentName:
            (payload.departmentId
              ? this.departments().find(dept => dept.departmentId === payload.departmentId)?.departmentName
              : current.departmentName) ?? current.departmentName,
          employmentType: employmentTypeLabel,
          status: statusLabel,
          location: payload.location ?? undefined,
          salaryMin: payload.salaryMin,
          salaryMax: payload.salaryMax,
          closingDate: payload.closingDate ?? undefined,
          description: payload.description ?? undefined,
          requirements: payload.requirements ?? undefined
        };
        this.updateOpeningInList(updated);
        this.selectedOpening.set(updated);
        this.feedback.set('Job opening updated successfully.');
        this.formSubmitting.set(false);
        this.formVisible.set(false);
      },
      error: () => {
        this.formSubmitting.set(false);
        this.error.set('Unable to update job opening. Please try again.');
      }
    });
  }

  openDeleteConfirmation(opening: JobOpeningDto, event?: Event): void {
    event?.stopPropagation();

    if (!this.canDeleteOpenings()) {
      return;
    }

    this.selectedOpening.set(opening);
    this.deleteVisible.set(true);
  }

  closeDeleteDialog(): void {
    if (this.deleteSubmitting()) {
      return;
    }
    this.deleteVisible.set(false);
  }

  confirmDelete(): void {
    const opening = this.selectedOpening();
    if (!opening) {
      return;
    }

    this.deleteSubmitting.set(true);

    this.hrApi.deleteJobOpening(opening.id).subscribe({
      next: () => {
        const filtered = this.openings().filter(item => item.id !== opening.id);
        this.openings.set(filtered);
        this.syncSelection(filtered);
        this.feedback.set('Job opening deleted successfully.');
        this.deleteSubmitting.set(false);
        this.deleteVisible.set(false);
      },
      error: () => {
        this.deleteSubmitting.set(false);
        this.error.set('Unable to delete job opening. Please try again.');
      }
    });
  }

  employmentTypeLabel(value?: string | number | null): string {
    if (value === null || value === undefined) {
      return 'Not specified';
    }

    if (typeof value === 'number') {
      const match = Object.entries(this.employmentTypeEnumMap).find(([, enumValue]) => enumValue === value);
      return match ? this.employmentTypeLabel(match[0]) : 'Not specified';
    }

    const normalized = value.toLowerCase();
    const found = this.employmentTypeOptions.find(option => option.value.toLowerCase() === normalized);
    if (found) {
      return found.label;
    }

    switch (normalized) {
      case 'fulltime':
        return 'Full Time';
      case 'parttime':
        return 'Part Time';
      case 'contract':
        return 'Contract';
      case 'internship':
        return 'Internship';
      case 'temporary':
        return 'Temporary';
      default:
        return value;
    }
  }

  statusLabel(value?: string | number | null): string {
    if (value === null || value === undefined) {
      return 'Draft';
    }

    if (typeof value === 'number') {
      const match = Object.entries(this.statusEnumMap).find(([, enumValue]) => enumValue === value);
      return match ? this.statusLabel(match[0]) : 'Draft';
    }

    const normalized = value.toLowerCase();
    const found = this.statusOptions.find(option => option.value.toLowerCase() === normalized);
    if (found) {
      return found.label;
    }

    return value;
  }

  statusSeverity(status?: string | null): 'success' | 'warning' | 'danger' | 'info' {
    const label = (status ? this.statusLabel(status) : this.statusLabel(undefined)).toLowerCase();
    switch (label) {
      case 'open':
        return 'success';
      case 'closed':
        return 'danger';
      case 'filled':
        return 'info';
      case 'cancelled':
        return 'warning';
      default:
        return 'info';
    }
  }

  private loadOpenings(): void {
    this.loading.set(true);
    this.error.set(null);

    this.hrApi.getJobOpenings({ includeApplications: true }).subscribe({
      next: data => {
        const normalized = (data ?? []).map(opening => this.normalizeOpening(opening));
        const sorted = this.sortOpenings(normalized);
        this.openings.set(sorted);
        this.syncSelection(sorted);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Unable to load job openings.');
        this.loading.set(false);
      }
    });
  }

  private loadDepartments(): void {
    this.hrApi.getDepartments().subscribe({
      next: data => this.departments.set(this.sortDepartments(data ?? [])),
      error: () => {
        // keep silent, as departments are optional for job openings
      }
    });
  }

  private sortOpenings(openings: JobOpeningDto[]): JobOpeningDto[] {
    return [...openings].sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());
  }

  private sortDepartments(departments: DepartmentDto[]): DepartmentDto[] {
    return [...departments].sort((a, b) => a.departmentName.localeCompare(b.departmentName));
  }

  private updateOpeningInList(updated: JobOpeningDto): void {
    const updatedList = this.sortOpenings(
      this.openings().map(opening => (opening.id === updated.id ? { ...opening, ...updated } : opening))
    );
    this.openings.set(updatedList);
    this.syncSelection(updatedList);
  }

  private normalizeOpening(opening: JobOpeningDto): JobOpeningDto {
    return {
      ...opening,
      employmentType: this.employmentTypeLabel(opening.employmentType),
      status: this.statusLabel(opening.status)
    };
  }

  private syncSelection(list: JobOpeningDto[]): void {
    const current = this.selectedOpening();

    if (!list.length) {
      this.selectedOpening.set(null);
      return;
    }

    if (!current) {
      this.selectedOpening.set(list[0]);
      return;
    }

    const next = list.find(opening => opening.id === current.id);
    this.selectedOpening.set(next ?? list[0]);
  }

  private defaultEmploymentType(): EmploymentTypeCode {
    return this.employmentTypeOptions[0]?.value ?? 'FullTime';
  }

  private defaultStatus(): JobStatusCode {
    const open = this.statusOptions.find(option => option.value === 'Open');
    return open?.value ?? this.statusOptions[0]?.value ?? 'Draft';
  }

  private employmentTypeValueFromDto(value?: string | number | null): EmploymentTypeCode {
    if (typeof value === 'number') {
      const entry = Object.entries(this.employmentTypeEnumMap).find(([, enumValue]) => enumValue === value);
      return (entry?.[0] as EmploymentTypeCode) ?? this.defaultEmploymentType();
    }

    if (!value) {
      return this.defaultEmploymentType();
    }

    const normalized = value.toLowerCase();
    const match = this.employmentTypeOptions.find(option => option.value.toLowerCase() === normalized);

    switch (normalized) {
      case 'fulltime':
        return 'FullTime';
      case 'full time':
        return 'FullTime';
      case 'parttime':
        return 'PartTime';
      case 'part time':
        return 'PartTime';
      case 'contract':
        return 'Contract';
      case 'internship':
        return 'Internship';
      case 'temporary':
        return 'Temporary';
      default:
        return match?.value ?? this.defaultEmploymentType();
    }
  }

  private statusValueFromDto(value?: string | number | null): JobStatusCode {
    if (typeof value === 'number') {
      const entry = Object.entries(this.statusEnumMap).find(([, enumValue]) => enumValue === value);
      return (entry?.[0] as JobStatusCode) ?? this.defaultStatus();
    }

    if (!value) {
      return this.defaultStatus();
    }

    const normalized = value.toLowerCase();
    const match = this.statusOptions.find(option => option.value.toLowerCase() === normalized);
    switch (normalized) {
      case 'draft':
        return 'Draft';
      case 'open':
        return 'Open';
      case 'closed':
        return 'Closed';
      case 'filled':
        return 'Filled';
      case 'cancelled':
        return 'Cancelled';
      default:
        return match?.value ?? this.defaultStatus();
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
