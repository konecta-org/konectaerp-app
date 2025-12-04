import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';

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

  // Summary
  getSummary(): Observable<FinanceSummaryDto> {
    return this.http.get<FinanceSummaryDto>(`${this.baseUrl}/FinanceSummary`);
  }

  // Budgets
  getBudgets(): Observable<BudgetDto[]> {
    return this.http.get<BudgetDto[]>(`${this.baseUrl}/Budgets`);
  }

  getBudget(id: number): Observable<BudgetDto> {
    return this.http.get<BudgetDto>(`${this.baseUrl}/Budgets/${id}`);
  }

  createBudget(budget: BudgetUpsertDto): Observable<BudgetDto> {
    return this.http.post<BudgetDto>(`${this.baseUrl}/Budgets`, budget);
  }

  updateBudget(id: number, budget: BudgetUpsertDto): Observable<BudgetDto> {
    return this.http.put<BudgetDto>(`${this.baseUrl}/Budgets/${id}`, budget);
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
    return this.http.get<InvoiceDto[]>(`${this.baseUrl}/Invoices`);
  }

  getInvoice(id: number): Observable<InvoiceDto> {
    return this.http.get<InvoiceDto>(`${this.baseUrl}/Invoices/${id}`);
  }

  createInvoice(invoice: InvoiceUpsertDto): Observable<InvoiceDto> {
    return this.http.post<InvoiceDto>(`${this.baseUrl}/Invoices`, invoice);
  }

  updateInvoice(id: number, invoice: InvoiceUpsertDto): Observable<InvoiceDto> {
    return this.http.put<InvoiceDto>(`${this.baseUrl}/Invoices/${id}`, invoice);
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
