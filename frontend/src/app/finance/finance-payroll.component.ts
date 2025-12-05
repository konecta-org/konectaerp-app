import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, FormArray, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { FinanceApiService, PayrollRunDto, PayrollRunUpsertDto, PayrollEntryUpsertDto } from '../core/services/finance-api.service';

@Component({
  selector: 'app-finance-payroll',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './finance-payroll.component.html',
  styleUrls: ['./finance-payroll.component.scss']
})
export class FinancePayrollComponent implements OnInit {
  private readonly financeApi = inject(FinanceApiService);
  private readonly fb = inject(FormBuilder);

  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly payrollRuns = signal<PayrollRunDto[]>([]);
  readonly selectedRun = signal<PayrollRunDto | null>(null);

  // Form state
  readonly formVisible = signal(false);
  readonly formMode = signal<'create' | 'edit'>('create');
  readonly editingId = signal<number | null>(null);
  readonly saving = signal(false);

  // Totals for display
  totalGross = 0;
  totalNet = 0;

  readonly payrollForm = this.fb.group({
    payrollNumber: ['', Validators.required],
    periodStart: ['', Validators.required],
    periodEnd: ['', Validators.required],
    paymentDate: ['', Validators.required],
    status: ['Draft', Validators.required],
    notes: [''],
    entries: this.fb.array([])
  });

  get entriesArray(): FormArray {
    return this.payrollForm.get('entries') as FormArray;
  }

  recalculateTotals(): void {
    let gross = 0;
    let net = 0;
    for (const entry of this.entriesArray.controls) {
      gross += +(entry.get('grossPay')?.value || 0);
      net += +(entry.get('netPay')?.value || 0);
    }
    this.totalGross = gross;
    this.totalNet = net;
  }

  ngOnInit(): void {
    this.loadPayrollRuns();
  }

  reload(): void {
    this.loadPayrollRuns();
  }

  trackById(_: number, run: PayrollRunDto): number {
    return run.id;
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(value);
  }

  getStatusClass(status: string): string {
    const normalized = status.toLowerCase();
    if (normalized === 'completed' || normalized === 'paid') return 'status--completed';
    if (normalized === 'processing') return 'status--processing';
    if (normalized === 'draft') return 'status--draft';
    if (normalized === 'cancelled') return 'status--cancelled';
    return '';
  }

  formatDateForInput(dateStr: string): string {
    if (!dateStr) return '';
    return dateStr.split('T')[0];
  }

  viewDetails(run: PayrollRunDto): void {
    this.selectedRun.set(run);
  }

  closeDetails(): void {
    this.selectedRun.set(null);
  }

  // CRUD Operations
  openCreateForm(): void {
    this.formMode.set('create');
    this.editingId.set(null);
    this.payrollForm.reset({ status: 'Draft' });
    this.entriesArray.clear();
    this.addEntry();
    this.totalGross = 0;
    this.totalNet = 0;
    this.formVisible.set(true);
  }

  openEditForm(run: PayrollRunDto): void {
    // Fetch the full payroll run with entries
    this.loading.set(true);
    this.financeApi.getPayrollRun(run.id).subscribe({
      next: (fullRun) => {
        this.loading.set(false);
        this.formMode.set('edit');
        this.editingId.set(fullRun.id);
        
        this.payrollForm.patchValue({
          payrollNumber: fullRun.payrollNumber,
          periodStart: this.formatDateForInput(fullRun.periodStart),
          periodEnd: this.formatDateForInput(fullRun.periodEnd),
          paymentDate: this.formatDateForInput(fullRun.paymentDate),
          status: fullRun.status,
          notes: fullRun.notes || ''
        });

        this.entriesArray.clear();
        if (fullRun.entries && fullRun.entries.length > 0) {
          for (const entry of fullRun.entries) {
            this.addEntry(entry);
          }
        } else {
          this.addEntry();
        }

        this.recalculateTotals();
        this.formVisible.set(true);
      },
      error: () => {
        this.loading.set(false);
        this.error.set('Failed to load payroll details.');
      }
    });
  }

  closeForm(): void {
    this.formVisible.set(false);
  }

  calculateEntryNetPay(index: number): void {
    const entry = this.entriesArray.at(index);
    const grossPay = +(entry.get('grossPay')?.value || 0);
    const deductions = +(entry.get('deductions')?.value || 0);
    const taxes = +(entry.get('taxes')?.value || 0);
    const netPay = grossPay - deductions - taxes;
    entry.get('netPay')?.setValue(netPay >= 0 ? netPay : 0);
    this.recalculateTotals();
  }

  addEntry(entry?: any): void {
    const entryGroup = this.fb.group({
      employeeId: [entry?.employeeId || '', Validators.required],
      employeeName: [entry?.employeeName || '', Validators.required],
      grossPay: [entry?.grossPay || 0, [Validators.required, Validators.min(0)]],
      deductions: [entry?.deductions || 0, [Validators.required, Validators.min(0)]],
      taxes: [entry?.taxes || 0, [Validators.required, Validators.min(0)]],
      netPay: [entry?.netPay || 0, [Validators.required, Validators.min(0)]],
      notes: [entry?.notes || '']
    });
    this.entriesArray.push(entryGroup);
    this.recalculateTotals();
  }

  removeEntry(index: number): void {
    if (this.entriesArray.length > 1) {
      this.entriesArray.removeAt(index);
      this.recalculateTotals();
    }
  }

  saveForm(): void {
    if (this.payrollForm.invalid) {
      this.payrollForm.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.error.set(null);
    const formValue = this.payrollForm.value;
    
    // Calculate totals from entries
    let totalGross = 0;
    let totalNet = 0;
    const entries: PayrollEntryUpsertDto[] = (formValue.entries || []).map((e: any) => {
      totalGross += +(e.grossPay || 0);
      totalNet += +(e.netPay || 0);
      const entry: PayrollEntryUpsertDto = {
        employeeId: e.employeeId || '',
        employeeName: e.employeeName || '',
        grossPay: +(e.grossPay || 0),
        deductions: +(e.deductions || 0),
        taxes: +(e.taxes || 0),
        netPay: +(e.netPay || 0)
      };
      if (e.notes) {
        entry.notes = e.notes;
      }
      return entry;
    });

    const payload: PayrollRunUpsertDto = {
      payrollNumber: formValue.payrollNumber || '',
      periodStart: formValue.periodStart || '',
      periodEnd: formValue.periodEnd || '',
      paymentDate: formValue.paymentDate || '',
      status: formValue.status || 'Draft',
      totalGrossPay: totalGross,
      totalNetPay: totalNet,
      entries
    };
    
    if (formValue.notes) {
      payload.notes = formValue.notes;
    }

    console.log('Saving payroll payload:', JSON.stringify(payload, null, 2));

    const request = this.formMode() === 'create'
      ? this.financeApi.createPayrollRun(payload)
      : this.financeApi.updatePayrollRun(this.editingId()!, payload);

    request.subscribe({
      next: () => {
        this.saving.set(false);
        this.formVisible.set(false);
        this.loadPayrollRuns();
      },
      error: (err) => {
        console.error('Payroll save error:', err);
        console.error('Validation errors:', err.error?.errors);
        this.saving.set(false);
        
        let errorMsg = 'Failed to save payroll run.';
        if (err.error?.errors) {
          const errors = err.error.errors;
          const messages = Object.entries(errors).map(([key, val]) => `${key}: ${(val as string[]).join(', ')}`);
          errorMsg = messages.join('; ');
        } else if (err.error?.title) {
          errorMsg = err.error.title;
        }
        this.error.set(errorMsg);
      }
    });
  }

  confirmDelete(run: PayrollRunDto): void {
    if (!confirm(`Delete payroll run "${run.payrollNumber}"? This action cannot be undone.`)) {
      return;
    }

    this.financeApi.deletePayrollRun(run.id).subscribe({
      next: () => this.loadPayrollRuns(),
      error: () => this.error.set('Failed to delete payroll run.')
    });
  }

  private loadPayrollRuns(): void {
    this.loading.set(true);
    this.error.set(null);

    this.financeApi.getPayrollRuns().subscribe({
      next: data => {
        this.payrollRuns.set(data ?? []);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Unable to load payroll data.');
        this.loading.set(false);
      }
    });
  }
}
