import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, FormArray, ReactiveFormsModule, Validators } from '@angular/forms';
import { FinanceApiService, InvoiceDto, InvoiceUpsertDto, InvoiceLineUpsertDto } from '../core/services/finance-api.service';

type FormMode = 'create' | 'edit';

@Component({
  selector: 'app-finance-invoices',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './finance-invoices.component.html',
  styleUrls: ['./finance-invoices.component.scss']
})
export class FinanceInvoicesComponent implements OnInit {
  private readonly financeApi = inject(FinanceApiService);
  private readonly fb = inject(FormBuilder);

  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly invoices = signal<InvoiceDto[]>([]);
  readonly selectedInvoice = signal<InvoiceDto | null>(null);
  readonly feedback = signal<string | null>(null);

  // Dialog state
  readonly formVisible = signal(false);
  readonly formMode = signal<FormMode>('create');
  readonly formSubmitting = signal(false);
  readonly editingInvoice = signal<InvoiceDto | null>(null);

  // Delete confirmation
  readonly deleteDialogVisible = signal(false);
  readonly deleteSubmitting = signal(false);
  readonly invoiceToDelete = signal<InvoiceDto | null>(null);

  readonly statusOptions = ['Draft', 'Sent', 'Paid', 'Overdue', 'Cancelled'];
  readonly currencyOptions = ['USD', 'EUR', 'GBP', 'CAD', 'AUD'];

  readonly invoiceForm = this.fb.group({
    invoiceNumber: ['', [Validators.required, Validators.maxLength(64)]],
    customerName: ['', [Validators.required, Validators.maxLength(128)]],
    customerEmail: ['', Validators.email],
    customerContact: [''],
    issueDate: ['', Validators.required],
    dueDate: ['', Validators.required],
    status: ['Draft', Validators.required],
    currency: ['USD', Validators.required],
    notes: [''],
    lines: this.fb.array([])
  });

  get linesFormArray(): FormArray {
    return this.invoiceForm.get('lines') as FormArray;
  }

  ngOnInit(): void {
    this.loadInvoices();
  }

  reload(): void {
    this.loadInvoices();
  }

  trackById(_: number, invoice: InvoiceDto): number {
    return invoice.id;
  }

  formatCurrency(value: number, currency: string = 'USD'): string {
    return new Intl.NumberFormat('en-US', { style: 'currency', currency }).format(value);
  }

  getStatusClass(status: string): string {
    const normalized = status.toLowerCase();
    if (normalized === 'paid') return 'status--paid';
    if (normalized === 'sent' || normalized === 'pending') return 'status--pending';
    if (normalized === 'overdue') return 'status--overdue';
    if (normalized === 'draft') return 'status--draft';
    if (normalized === 'cancelled' || normalized === 'voided') return 'status--cancelled';
    return '';
  }

  isOverdue(invoice: InvoiceDto): boolean {
    const dueDate = new Date(invoice.dueDate);
    const today = new Date();
    return invoice.balanceDue > 0 && dueDate < today;
  }

  viewDetails(invoice: InvoiceDto): void {
    this.selectedInvoice.set(invoice);
  }

  closeDetails(): void {
    this.selectedInvoice.set(null);
  }

  // --- Form Actions ---
  openCreateDialog(): void {
    this.formMode.set('create');
    this.editingInvoice.set(null);
    const nextNumber = `INV-${new Date().getFullYear()}-${String(this.invoices().length + 1).padStart(4, '0')}`;
    const today = new Date();
    const dueDate = new Date(today);
    dueDate.setDate(dueDate.getDate() + 30);

    this.invoiceForm.reset({
      invoiceNumber: nextNumber,
      customerName: '',
      customerEmail: '',
      customerContact: '',
      issueDate: this.formatDateForInput(today),
      dueDate: this.formatDateForInput(dueDate),
      status: 'Draft',
      currency: 'USD',
      notes: ''
    });
    this.linesFormArray.clear();
    this.addLine();
    this.formVisible.set(true);
  }

  openEditDialog(invoice: InvoiceDto): void {
    this.formMode.set('edit');
    this.editingInvoice.set(invoice);
    this.invoiceForm.patchValue({
      invoiceNumber: invoice.invoiceNumber,
      customerName: invoice.customerName,
      customerEmail: invoice.customerEmail || '',
      customerContact: invoice.customerContact || '',
      issueDate: this.formatDateForInput(new Date(invoice.issueDate)),
      dueDate: this.formatDateForInput(new Date(invoice.dueDate)),
      status: invoice.status,
      currency: invoice.currency,
      notes: invoice.notes || ''
    });
    this.linesFormArray.clear();
    invoice.lines?.forEach(line => this.addLine(line));
    if (this.linesFormArray.length === 0) this.addLine();
    this.formVisible.set(true);
    this.closeDetails();
  }

  closeDialog(): void {
    this.formVisible.set(false);
    this.editingInvoice.set(null);
  }

  addLine(line?: { itemCode?: string; description: string; quantity: number; unitPrice: number }): void {
    const lineGroup = this.fb.group({
      itemCode: [line?.itemCode || ''],
      description: [line?.description || '', Validators.required],
      quantity: [line?.quantity || 1, [Validators.required, Validators.min(0.01)]],
      unitPrice: [line?.unitPrice || 0, [Validators.required, Validators.min(0)]]
    });
    this.linesFormArray.push(lineGroup);
  }

  removeLine(index: number): void {
    this.linesFormArray.removeAt(index);
  }

  calculateTotals(): { subtotal: number; tax: number; total: number } {
    let subtotal = 0;
    for (const line of this.linesFormArray.controls) {
      const qty = line.get('quantity')?.value || 0;
      const price = line.get('unitPrice')?.value || 0;
      subtotal += qty * price;
    }
    const tax = subtotal * 0.1; // 10% tax
    return { subtotal, tax, total: subtotal + tax };
  }

  submitForm(): void {
    if (this.invoiceForm.invalid) {
      this.invoiceForm.markAllAsTouched();
      return;
    }

    const formValue = this.invoiceForm.value;
    const totals = this.calculateTotals();
    const dto: InvoiceUpsertDto = {
      invoiceNumber: formValue.invoiceNumber!,
      customerName: formValue.customerName!,
      customerEmail: formValue.customerEmail || undefined,
      customerContact: formValue.customerContact || undefined,
      issueDate: formValue.issueDate!,
      dueDate: formValue.dueDate!,
      status: formValue.status!,
      subtotal: totals.subtotal,
      taxAmount: totals.tax,
      totalAmount: totals.total,
      paidAmount: this.editingInvoice()?.paidAmount || 0,
      currency: formValue.currency!,
      notes: formValue.notes || undefined,
      lines: (formValue.lines || []).map((line: any): InvoiceLineUpsertDto => ({
        itemCode: line.itemCode || undefined,
        description: line.description,
        quantity: line.quantity,
        unitPrice: line.unitPrice
      }))
    };

    this.formSubmitting.set(true);

    const request$ = this.formMode() === 'create'
      ? this.financeApi.createInvoice(dto)
      : this.financeApi.updateInvoice(this.editingInvoice()!.id, dto);

    request$.subscribe({
      next: () => {
        this.formSubmitting.set(false);
        this.formVisible.set(false);
        this.showFeedback(this.formMode() === 'create' ? 'Invoice created successfully.' : 'Invoice updated successfully.');
        this.loadInvoices();
      },
      error: () => {
        this.formSubmitting.set(false);
        this.showFeedback('Failed to save invoice. Please try again.');
      }
    });
  }

  // --- Quick Actions ---
  markAsPaid(invoice: InvoiceDto): void {
    const dto: InvoiceUpsertDto = {
      invoiceNumber: invoice.invoiceNumber,
      customerName: invoice.customerName,
      customerEmail: invoice.customerEmail,
      customerContact: invoice.customerContact,
      issueDate: invoice.issueDate,
      dueDate: invoice.dueDate,
      status: 'Paid',
      subtotal: invoice.subtotal,
      taxAmount: invoice.taxAmount,
      totalAmount: invoice.totalAmount,
      paidAmount: invoice.totalAmount,
      currency: invoice.currency,
      notes: invoice.notes,
      lines: invoice.lines?.map(l => ({
        itemCode: l.itemCode,
        description: l.description,
        quantity: l.quantity,
        unitPrice: l.unitPrice
      })) || []
    };

    this.financeApi.updateInvoice(invoice.id, dto).subscribe({
      next: () => {
        this.showFeedback('Invoice marked as paid.');
        this.loadInvoices();
        this.closeDetails();
      },
      error: () => {
        this.showFeedback('Failed to update invoice.');
      }
    });
  }

  // --- Delete Actions ---
  openDeleteDialog(invoice: InvoiceDto): void {
    this.invoiceToDelete.set(invoice);
    this.deleteDialogVisible.set(true);
    this.closeDetails();
  }

  closeDeleteDialog(): void {
    this.deleteDialogVisible.set(false);
    this.invoiceToDelete.set(null);
  }

  confirmDelete(): void {
    const invoice = this.invoiceToDelete();
    if (!invoice) return;

    this.deleteSubmitting.set(true);
    this.financeApi.deleteInvoice(invoice.id).subscribe({
      next: () => {
        this.deleteSubmitting.set(false);
        this.deleteDialogVisible.set(false);
        this.invoiceToDelete.set(null);
        this.showFeedback('Invoice deleted successfully.');
        this.loadInvoices();
      },
      error: () => {
        this.deleteSubmitting.set(false);
        this.showFeedback('Failed to delete invoice. Please try again.');
      }
    });
  }

  // --- Helpers ---
  private loadInvoices(): void {
    this.loading.set(true);
    this.error.set(null);

    this.financeApi.getInvoices().subscribe({
      next: data => {
        this.invoices.set(data ?? []);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Unable to load invoice data.');
        this.loading.set(false);
      }
    });
  }

  private formatDateForInput(date: Date): string {
    return date.toISOString().split('T')[0];
  }

  private showFeedback(message: string): void {
    this.feedback.set(message);
    setTimeout(() => this.feedback.set(null), 4000);
  }
}
