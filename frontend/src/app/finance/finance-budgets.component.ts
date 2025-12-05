import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, FormArray, ReactiveFormsModule, Validators } from '@angular/forms';
import { FinanceApiService, BudgetDto, BudgetUpsertDto, BudgetLineUpsertDto } from '../core/services/finance-api.service';

type FormMode = 'create' | 'edit';

@Component({
  selector: 'app-finance-budgets',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './finance-budgets.component.html',
  styleUrls: ['./finance-budgets.component.scss']
})
export class FinanceBudgetsComponent implements OnInit {
  private readonly financeApi = inject(FinanceApiService);
  private readonly fb = inject(FormBuilder);

  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly budgets = signal<BudgetDto[]>([]);
  readonly feedback = signal<string | null>(null);

  // Dialog state
  readonly formVisible = signal(false);
  readonly formMode = signal<FormMode>('create');
  readonly formSubmitting = signal(false);
  readonly selectedBudget = signal<BudgetDto | null>(null);

  // Delete confirmation
  readonly deleteDialogVisible = signal(false);
  readonly deleteSubmitting = signal(false);
  readonly budgetToDelete = signal<BudgetDto | null>(null);

  readonly budgetForm = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(128)]],
    department: [''],
    fiscalYear: [new Date().getFullYear(), [Validators.required, Validators.min(2000), Validators.max(3000)]],
    startDate: ['', Validators.required],
    endDate: ['', Validators.required],
    totalAmount: [0, [Validators.required, Validators.min(0)]],
    spentAmount: [0, [Validators.min(0)]],
    notes: [''],
    lines: this.fb.array([])
  });

  get linesFormArray(): FormArray {
    return this.budgetForm.get('lines') as FormArray;
  }

  ngOnInit(): void {
    this.loadBudgets();
  }

  reload(): void {
    this.loadBudgets();
  }

  trackById(_: number, budget: BudgetDto): number {
    return budget.id;
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(value);
  }

  getUtilization(budget: BudgetDto): number {
    if (budget.totalAmount === 0) return 0;
    return (budget.spentAmount / budget.totalAmount) * 100;
  }

  getUtilizationClass(budget: BudgetDto): string {
    const pct = this.getUtilization(budget);
    if (pct >= 90) return 'utilization--critical';
    if (pct >= 75) return 'utilization--warning';
    return 'utilization--healthy';
  }

  // --- Form Actions ---
  openCreateDialog(): void {
    this.formMode.set('create');
    this.selectedBudget.set(null);
    this.budgetForm.reset({
      name: '',
      department: '',
      fiscalYear: new Date().getFullYear(),
      startDate: this.formatDateForInput(new Date(new Date().getFullYear(), 0, 1)),
      endDate: this.formatDateForInput(new Date(new Date().getFullYear(), 11, 31)),
      totalAmount: 0,
      spentAmount: 0,
      notes: ''
    });
    this.linesFormArray.clear();
    this.formVisible.set(true);
  }

  openEditDialog(budget: BudgetDto): void {
    this.formMode.set('edit');
    this.selectedBudget.set(budget);
    this.budgetForm.patchValue({
      name: budget.name,
      department: budget.department || '',
      fiscalYear: budget.fiscalYear,
      startDate: this.formatDateForInput(new Date(budget.startDate)),
      endDate: this.formatDateForInput(new Date(budget.endDate)),
      totalAmount: budget.totalAmount,
      spentAmount: budget.spentAmount,
      notes: budget.notes || ''
    });
    this.linesFormArray.clear();
    budget.lines?.forEach(line => this.addLine(line));
    this.formVisible.set(true);
  }

  closeDialog(): void {
    this.formVisible.set(false);
    this.selectedBudget.set(null);
  }

  addLine(line?: { category: string; allocatedAmount: number; spentAmount: number; notes?: string }): void {
    const lineGroup = this.fb.group({
      category: [line?.category || '', Validators.required],
      allocatedAmount: [line?.allocatedAmount || 0, [Validators.required, Validators.min(0)]],
      spentAmount: [line?.spentAmount || 0, Validators.min(0)],
      notes: [line?.notes || '']
    });
    this.linesFormArray.push(lineGroup);
  }

  removeLine(index: number): void {
    this.linesFormArray.removeAt(index);
  }

  submitForm(): void {
    if (this.budgetForm.invalid) {
      this.budgetForm.markAllAsTouched();
      return;
    }

    const formValue = this.budgetForm.value;
    const dto: BudgetUpsertDto = {
      name: formValue.name!,
      department: formValue.department || undefined,
      fiscalYear: formValue.fiscalYear!,
      startDate: formValue.startDate!,
      endDate: formValue.endDate!,
      totalAmount: formValue.totalAmount!,
      spentAmount: formValue.spentAmount || 0,
      notes: formValue.notes || undefined,
      lines: (formValue.lines || []).map((line: any): BudgetLineUpsertDto => ({
        category: line.category,
        allocatedAmount: line.allocatedAmount,
        spentAmount: line.spentAmount || 0,
        notes: line.notes || undefined
      }))
    };

    this.formSubmitting.set(true);

    const request$ = this.formMode() === 'create'
      ? this.financeApi.createBudget(dto)
      : this.financeApi.updateBudget(this.selectedBudget()!.id, dto);

    request$.subscribe({
      next: () => {
        this.formSubmitting.set(false);
        this.formVisible.set(false);
        this.showFeedback(this.formMode() === 'create' ? 'Budget created successfully.' : 'Budget updated successfully.');
        this.loadBudgets();
      },
      error: () => {
        this.formSubmitting.set(false);
        this.showFeedback('Failed to save budget. Please try again.');
      }
    });
  }

  // --- Delete Actions ---
  openDeleteDialog(budget: BudgetDto): void {
    this.budgetToDelete.set(budget);
    this.deleteDialogVisible.set(true);
  }

  closeDeleteDialog(): void {
    this.deleteDialogVisible.set(false);
    this.budgetToDelete.set(null);
  }

  confirmDelete(): void {
    const budget = this.budgetToDelete();
    if (!budget) return;

    this.deleteSubmitting.set(true);
    this.financeApi.deleteBudget(budget.id).subscribe({
      next: () => {
        this.deleteSubmitting.set(false);
        this.deleteDialogVisible.set(false);
        this.budgetToDelete.set(null);
        this.showFeedback('Budget deleted successfully.');
        this.loadBudgets();
      },
      error: () => {
        this.deleteSubmitting.set(false);
        this.showFeedback('Failed to delete budget. Please try again.');
      }
    });
  }

  // --- Helpers ---
  private loadBudgets(): void {
    this.loading.set(true);
    this.error.set(null);

    this.financeApi.getBudgets().subscribe({
      next: data => {
        this.budgets.set(data ?? []);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Unable to load budget data.');
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
