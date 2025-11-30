import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, forkJoin, map } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface DashboardSummary {
  userSummary: UserSummary;
  hrSummary: HrSummary;
  financeSummary: FinanceSummary;
  inventorySummary: InventorySummary;
}

export interface UserSummary {
  totalUsers: number;
  activeUsers: number;
  newUsersThisMonth: number;
  inactiveUsers: number;
}

export interface HrSummary {
  totalEmployees: number;
  activeEmployees: number;
  departmentCount: number;
  pendingLeaveRequests: number;
  pendingResignations: number;
}

export interface FinanceSummary {
  totalRevenue: number;
  totalExpenses: number;
  pendingInvoices: number;
  budgetUtilization: number;
}

export interface InventorySummary {
  totalItems: number;
  lowStockItems: number;
  totalValue: number;
  recentTransactions: number;
}

@Injectable({
  providedIn: 'root'
})
export class DashboardApiService {
  private readonly baseUrl = environment.apiBaseUrl;

  constructor(private http: HttpClient) {}

  getDashboardSummary(): Observable<DashboardSummary> {
    return forkJoin({
      userSummary: this.getUserSummary(),
      hrSummary: this.getHrSummary(),
      financeSummary: this.getFinanceSummary(),
      inventorySummary: this.getInventorySummary()
    });
  }

  private getUserSummary(): Observable<UserSummary> {
    return this.http.get<UserSummary>(`${this.baseUrl}${environment.endpoints.users}/users/summary`);
  }

  private getHrSummary(): Observable<HrSummary> {
    return this.http.get<HrSummary>(`${this.baseUrl}${environment.endpoints.hr}/HrSummary`);
  }

  private getFinanceSummary(): Observable<FinanceSummary> {
    return this.http.get<FinanceSummary>(`${this.baseUrl}${environment.endpoints.finance}/FinanceSummary`);
  }

  private getInventorySummary(): Observable<InventorySummary> {
    return this.http.get<InventorySummary>(`${this.baseUrl}${environment.endpoints.inventory}/inventory-summary`);
  }
}
