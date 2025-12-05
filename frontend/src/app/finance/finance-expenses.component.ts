import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { FinanceApiService, ExpenseDto, ExpenseUpsertDto } from '../core/services/finance-api.service';

type FormMode = 'create' | 'edit';

@Component({
  selector: 'app-finance-expenses',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './finance-expenses.component.html',
  styleUrls: ['./finance-expenses.component.scss']
})
export class FinanceExpensesComponent implements OnInit {
  private readonly financeApi = inject(FinanceApiService);
  private readonly fb = inject(FormBuilder);

  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly expenses = signal<ExpenseDto[]>([]);
  readonly feedback = signal<string | null>(null);

  // Dialog state
  readonly formVisible = signal(false);
  readonly formMode = signal<FormMode>('create');
  readonly formSubmitting = signal(false);
  readonly selectedExpense = signal<ExpenseDto | null>(null);

  // Delete confirmation
  readonly deleteDialogVisible = signal(false);
  readonly deleteSubmitting = signal(false);
  readonly expenseToDelete = signal<ExpenseDto | null>(null);

  readonly statusOptions = ['Draft', 'Pending', 'Approved', 'Rejected', 'Paid'];
  readonly paymentMethodOptions = ['BankTransfer', 'CreditCard', 'Cash', 'Check', 'PurchaseOrder'];
  readonly categoryOptions = ['Technology', 'Marketing', 'Travel', 'Office Supplies', 'Software', 'Consulting', 
    'Utilities', 'Training', 'Equipment', 'Maintenance', 'Legal', 'Cloud Services', 'Recruitment', 
    'Insurance', 'Events', 'Furniture', 'Other'];

  readonly expenseForm = this.fb.group({
    expenseNumber: ['', [Validators.required, Validators.maxLength(64)]],
    category: ['', Validators.required],
    vendor: [''],
    description: ['', [Validators.required, Validators.maxLength(512)]],
    incurredOn: ['', Validators.required],
    status: ['Pending', Validators.required],
    paymentMethod: ['BankTransfer', Validators.required],
    amount: [0, [Validators.required, Validators.min(0.01)]],
    notes: ['']
  });

  ngOnInit(): void {
    this.loadExpenses();
  }

  reload(): void {
    this.loadExpenses();
  }

  trackById(_: number, expense: ExpenseDto): number {
    return expense.id;
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(value);
  }

  getStatusClass(status: string): string {
    const normalized = status.toLowerCase();
    if (normalized === 'approved') return 'status--approved';
    if (normalized === 'pending') return 'status--pending';
    if (normalized === 'rejected') return 'status--rejected';
    if (normalized === 'paid') return 'status--paid';
    if (normalized === 'draft') return 'status--draft';
    return '';
  }

  // --- Form Actions ---
  openCreateDialog(): void {
    this.formMode.set('create');
    this.selectedExpense.set(null);
    const nextNumber = `EXP-${new Date().getFullYear()}-${String(this.expenses().length + 1).padStart(4, '0')}`;
    this.expenseForm.reset({
      expenseNumber: nextNumber,
      category: '',
      vendor: '',
      description: '',
      incurredOn: this.formatDateForInput(new Date()),
      status: 'Pending',
      paymentMethod: 'BankTransfer',
      amount: 0,
      notes: ''
    });
    this.formVisible.set(true);
  }

  openEditDialog(expense: ExpenseDto): void {
    this.formMode.set('edit');
    this.selectedExpense.set(expense);
    this.expenseForm.patchValue({
      expenseNumber: expense.expenseNumber,
      category: expense.category,
      vendor: expense.vendor || '',
      description: expense.description,
      incurredOn: this.formatDateForInput(new Date(expense.incurredOn)),
      status: expense.status,
      paymentMethod: expense.paymentMethod,
      amount: expense.amount,
      notes: expense.notes || ''
    });
    this.formVisible.set(true);
  }

  closeDialog(): void {
    this.formVisible.set(false);
    this.selectedExpense.set(null);
  }

  submitForm(): void {
    if (this.expenseForm.invalid) {
      this.expenseForm.markAllAsTouched();
      return;
    }

    const formValue = this.expenseForm.value;
    const dto: ExpenseUpsertDto = {
      expenseNumber: formValue.expenseNumber!,
      category: formValue.category!,
      vendor: formValue.vendor || undefined,
      description: formValue.description!,
      incurredOn: formValue.incurredOn!,
      status: formValue.status!,
      paymentMethod: formValue.paymentMethod!,
      amount: formValue.amount!,
      notes: formValue.notes || undefined
    };

    this.formSubmitting.set(true);

    const request$ = this.formMode() === 'create'
      ? this.financeApi.createExpense(dto)
      : this.financeApi.updateExpense(this.selectedExpense()!.id, dto);

    request$.subscribe({
      next: () => {
        this.formSubmitting.set(false);
        this.formVisible.set(false);
        this.showFeedback(this.formMode() === 'create' ? 'Expense created successfully.' : 'Expense updated successfully.');
        this.loadExpenses();
      },
      error: () => {
        this.formSubmitting.set(false);
        this.showFeedback('Failed to save expense. Please try again.');
      }
    });
  }

  // --- Quick Status Actions ---
  approveExpense(expense: ExpenseDto): void {
    this.updateExpenseStatus(expense, 'Approved');
  }

  rejectExpense(expense: ExpenseDto): void {
    this.updateExpenseStatus(expense, 'Rejected');
  }

  private updateExpenseStatus(expense: ExpenseDto, newStatus: string): void {
    const dto: ExpenseUpsertDto = {
      expenseNumber: expense.expenseNumber,
      category: expense.category,
      vendor: expense.vendor,
      description: expense.description,
      incurredOn: expense.incurredOn,
      status: newStatus,
      paymentMethod: expense.paymentMethod,
      amount: expense.amount,
      notes: expense.notes
    };

    this.financeApi.updateExpense(expense.id, dto).subscribe({
      next: () => {
        this.showFeedback(`Expense ${newStatus.toLowerCase()}.`);
        this.loadExpenses();
      },
      error: () => {
        this.showFeedback('Failed to update expense status.');
      }
    });
  }

  // --- Delete Actions ---
  openDeleteDialog(expense: ExpenseDto): void {
    this.expenseToDelete.set(expense);
    this.deleteDialogVisible.set(true);
  }

  closeDeleteDialog(): void {
    this.deleteDialogVisible.set(false);
    this.expenseToDelete.set(null);
  }

  confirmDelete(): void {
    const expense = this.expenseToDelete();
    if (!expense) return;

    this.deleteSubmitting.set(true);
    this.financeApi.deleteExpense(expense.id).subscribe({
      next: () => {
        this.deleteSubmitting.set(false);
        this.deleteDialogVisible.set(false);
        this.expenseToDelete.set(null);
        this.showFeedback('Expense deleted successfully.');
        this.loadExpenses();
      },
      error: () => {
        this.deleteSubmitting.set(false);
        this.showFeedback('Failed to delete expense. Please try again.');
      }
    });
  }

  // --- Helpers ---
  private loadExpenses(): void {
    this.loading.set(true);
    this.error.set(null);

    this.financeApi.getExpenses().subscribe({
      next: data => {
        this.expenses.set(data ?? []);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Unable to load expense data.');
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
