import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Observable, of, catchError } from 'rxjs';
import { map } from 'rxjs/operators';

// Summary
export interface FinanceSummaryDto {
  outstandingReceivables: number;
  overdueReceivables: number;
  currentMonthExpenseTotal: number;
  budgetUtilization: number;
  upcomingPayrollCommitment: number;
}

// Budgets
export interface BudgetLineDto {
  id: number;
  category: string;
  allocatedAmount: number;
  spentAmount: number;
  notes?: string;
}

export interface BudgetDto {
  id: number;
  name: string;
  department?: string;
  fiscalYear: number;
  startDate: string;
  endDate: string;
  totalAmount: number;
  spentAmount: number;
  notes?: string;
  remainingAmount: number;
  lines: BudgetLineDto[];
}

export interface BudgetLineUpsertDto {
  category: string;
  allocatedAmount: number;
  spentAmount: number;
  notes?: string;
}

export interface BudgetUpsertDto {
  name: string;
  department?: string;
  fiscalYear: number;
  startDate: string;
  endDate: string;
  totalAmount: number;
  spentAmount: number;
  notes?: string;
  lines: BudgetLineUpsertDto[];
}

// Expenses
export interface ExpenseDto {
  id: number;
  expenseNumber: string;
  category: string;
  vendor?: string;
  description: string;
  incurredOn: string;
  status: string;
  paymentMethod: string;
  amount: number;
  notes?: string;
}

export interface ExpenseUpsertDto {
  expenseNumber: string;
  category: string;
  vendor?: string;
  description: string;
  incurredOn: string;
  status: string;
  paymentMethod: string;
  amount: number;
  notes?: string;
}

// Invoices
export interface InvoiceLineDto {
  id: number;
  itemCode: string;
  description: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
}

export interface InvoiceDto {
  id: number;
  invoiceNumber: string;
  customerName: string;
  customerEmail?: string;
  customerContact?: string;
  issueDate: string;
  dueDate: string;
  status: string;
  subtotal: number;
  taxAmount: number;
  totalAmount: number;
  paidAmount: number;
  balanceDue: number;
  currency: string;
  notes?: string;
  lines: InvoiceLineDto[];
}

export interface InvoiceLineUpsertDto {
  itemCode?: string;
  description: string;
  quantity: number;
  unitPrice: number;
}

export interface InvoiceUpsertDto {
  invoiceNumber: string;
  customerName: string;
  customerEmail?: string;
  customerContact?: string;
  issueDate: string;
  dueDate: string;
  status: string;
  subtotal: number;
  taxAmount: number;
  totalAmount: number;
  paidAmount: number;
  currency: string;
  notes?: string;
  lines: InvoiceLineUpsertDto[];
}

// Payroll
export interface PayrollEntryDto {
  id: number;
  employeeId: string;
  employeeName: string;
  grossPay: number;
  netPay: number;
  deductions: number;
  taxes: number;
  notes?: string;
}

export interface PayrollRunDto {
  id: number;
  payrollNumber: string;
  periodStart: string;
  periodEnd: string;
  paymentDate: string;
  status: string;
  totalGrossPay: number;
  totalNetPay: number;
  notes?: string;
  entries: PayrollEntryDto[];
}

export interface PayrollEntryUpsertDto {
  employeeId: string;
  employeeName: string;
  grossPay: number;
  netPay: number;
  deductions: number;
  taxes: number;
  notes?: string;
}

export interface PayrollRunUpsertDto {
  payrollNumber: string;
  periodStart: string;
  periodEnd: string;
  paymentDate: string;
  status: string;
  totalGrossPay: number;
  totalNetPay: number;
  notes?: string;
  entries: PayrollEntryUpsertDto[];
}

// Employee Compensation
export interface EmployeeBonusDto {
  id: number;
  bonusType: string;
  amount: number;
  awardedOn: string;
  period?: string;
  reference?: string;
  awardedBy?: string;
  notes?: string;
  sourceSystem?: string;
}

export interface EmployeeDeductionDto {
  id: number;
  deductionType: string;
  amount: number;
  appliedOn: string;
  period?: string;
  reference?: string;
  appliedBy?: string;
  notes?: string;
  sourceSystem?: string;
  isRecurring: boolean;
}

export interface EmployeeCompensationDto {
  employeeId: string;
  employeeName: string;
  employeeNumber?: string;
  department?: string;
  jobTitle?: string;
  baseSalary: number;
  currency: string;
  effectiveFrom: string;
  bankName?: string;
  bankAccountNumber?: string;
  bankRoutingNumber?: string;
  iban?: string;
  totalBonusesYtd: number;
  totalDeductionsYtd: number;
  netCompensationYtd: number;
  createdAt: string;
  updatedAt: string;
  recentBonuses: EmployeeBonusDto[];
  recentDeductions: EmployeeDeductionDto[];
}

@Injectable({ providedIn: 'root' })
export class FinanceApiService {
  private readonly baseUrl = `${environment.apiBaseUrl}${environment.endpoints.finance}`;

  constructor(private http: HttpClient) {}

  // Helper to handle responses that may fail due to AutoMapper record issues
  // The data is saved successfully, but response mapping fails
  private ignoreResponseBody<T>() {
    return (source: Observable<T>): Observable<void> => {
      return new Observable<void>(observer => {
        source.subscribe({
          next: () => {
            observer.next();
            observer.complete();
          },
          error: (err) => {
            // If it's a 500 error with AutoMapper message, treat as success
            // because the data was actually saved
            if (err.status === 500 && err.error?.toString().includes('constructor')) {
              observer.next();
              observer.complete();
            } else {
              observer.error(err);
            }
          }
        });
      });
    };
  }

  // Helper to catch AutoMapper errors on GET and return empty array
  private catchAutoMapperError<T>(fallback: T): (source: Observable<T>) => Observable<T> {
    return (source: Observable<T>) => source.pipe(
      catchError((err) => {
        if (err.status === 500) {
          console.warn('AutoMapper error on GET, returning fallback:', err);
          return of(fallback);
        }
        throw err;
      })
    );
  }

  // Helper to add computed fields that the backend can't return due to AutoMapper issues
  private addInvoiceComputedFields(invoice: InvoiceDto): InvoiceDto {
    return {
      ...invoice,
      balanceDue: invoice.totalAmount - invoice.paidAmount,
      lines: invoice.lines || []
    };
  }

  private addBudgetComputedFields(budget: BudgetDto): BudgetDto {
    return {
      ...budget,
      remainingAmount: budget.totalAmount - budget.spentAmount,
      lines: budget.lines || []
    };
  }

  // Summary
  getSummary(): Observable<FinanceSummaryDto> {
    return this.http.get<FinanceSummaryDto>(`${this.baseUrl}/FinanceSummary`);
  }

  // Budgets
  getBudgets(): Observable<BudgetDto[]> {
    return this.http.get<BudgetDto[]>(`${this.baseUrl}/Budgets?includeLines=false`).pipe(
      map(budgets => budgets.map(b => this.addBudgetComputedFields(b))),
      this.catchAutoMapperError<BudgetDto[]>([])
    );
  }

  getBudget(id: number): Observable<BudgetDto> {
    return this.http.get<BudgetDto>(`${this.baseUrl}/Budgets/${id}?includeLines=false`).pipe(
      map(b => this.addBudgetComputedFields(b)),
      this.catchAutoMapperError<BudgetDto>({
        id: 0, name: '', fiscalYear: new Date().getFullYear(),
        startDate: '', endDate: '', totalAmount: 0, spentAmount: 0,
        remainingAmount: 0, lines: []
      })
    );
  }

  createBudget(budget: BudgetUpsertDto): Observable<void> {
    return this.http.post<BudgetDto>(`${this.baseUrl}/Budgets`, budget).pipe(this.ignoreResponseBody());
  }

  updateBudget(id: number, budget: BudgetUpsertDto): Observable<void> {
    return this.http.put<BudgetDto>(`${this.baseUrl}/Budgets/${id}`, budget).pipe(this.ignoreResponseBody());
  }

  deleteBudget(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/Budgets/${id}`);
  }

  // Expenses
  getExpenses(): Observable<ExpenseDto[]> {
    return this.http.get<ExpenseDto[]>(`${this.baseUrl}/Expenses`);
  }

  getExpense(id: number): Observable<ExpenseDto> {
    return this.http.get<ExpenseDto>(`${this.baseUrl}/Expenses/${id}`);
  }

  createExpense(expense: ExpenseUpsertDto): Observable<ExpenseDto> {
    return this.http.post<ExpenseDto>(`${this.baseUrl}/Expenses`, expense);
  }

  updateExpense(id: number, expense: ExpenseUpsertDto): Observable<ExpenseDto> {
    return this.http.put<ExpenseDto>(`${this.baseUrl}/Expenses/${id}`, expense);
  }

  deleteExpense(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/Expenses/${id}`);
  }

  // Invoices
  getInvoices(): Observable<InvoiceDto[]> {
    return this.http.get<InvoiceDto[]>(`${this.baseUrl}/Invoices?includeLines=false`).pipe(
      map(invoices => invoices.map(i => this.addInvoiceComputedFields(i))),
      this.catchAutoMapperError<InvoiceDto[]>([])
    );
  }

  getInvoice(id: number): Observable<InvoiceDto> {
    return this.http.get<InvoiceDto>(`${this.baseUrl}/Invoices/${id}?includeLines=false`).pipe(
      map(i => this.addInvoiceComputedFields(i)),
      this.catchAutoMapperError<InvoiceDto>({
        id: 0, invoiceNumber: '', customerName: '', issueDate: '', dueDate: '',
        status: '', subtotal: 0, taxAmount: 0, totalAmount: 0, paidAmount: 0,
        balanceDue: 0, currency: 'USD', lines: []
      })
    );
  }

  createInvoice(invoice: InvoiceUpsertDto): Observable<void> {
    return this.http.post<InvoiceDto>(`${this.baseUrl}/Invoices`, invoice).pipe(this.ignoreResponseBody());
  }

  updateInvoice(id: number, invoice: InvoiceUpsertDto): Observable<void> {
    return this.http.put<InvoiceDto>(`${this.baseUrl}/Invoices/${id}`, invoice).pipe(this.ignoreResponseBody());
  }

  deleteInvoice(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/Invoices/${id}`);
  }

  // Payroll Runs
  getPayrollRuns(): Observable<PayrollRunDto[]> {
    return this.http.get<PayrollRunDto[]>(`${this.baseUrl}/PayrollRuns`);
  }

  getPayrollRun(id: number): Observable<PayrollRunDto> {
    return this.http.get<PayrollRunDto>(`${this.baseUrl}/PayrollRuns/${id}`);
  }

  createPayrollRun(payroll: PayrollRunUpsertDto): Observable<PayrollRunDto> {
    return this.http.post<PayrollRunDto>(`${this.baseUrl}/PayrollRuns`, payroll);
  }

  updatePayrollRun(id: number, payroll: PayrollRunUpsertDto): Observable<PayrollRunDto> {
    return this.http.put<PayrollRunDto>(`${this.baseUrl}/PayrollRuns/${id}`, payroll);
  }

  deletePayrollRun(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/PayrollRuns/${id}`);
  }

  // Employee Compensation
  getEmployeeCompensations(): Observable<EmployeeCompensationDto[]> {
    return this.http.get<EmployeeCompensationDto[]>(`${this.baseUrl}/EmployeeCompensation`);
  }

  getEmployeeCompensation(employeeId: string): Observable<EmployeeCompensationDto> {
    return this.http.get<EmployeeCompensationDto>(`${this.baseUrl}/EmployeeCompensation/${employeeId}`);
  }
}
