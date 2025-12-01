import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of, map, concatMap, delay } from 'rxjs';
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
    // Use sequential requests with delays to prevent database overload
    // Each request waits 300ms after the previous one completes
    return of(null).pipe(
      // First request: User Summary
      concatMap(() => this.getUserSummary()),
      concatMap((userSummary: UserSummary) => 
        // Wait 300ms, then fetch HR Summary
        of(userSummary).pipe(
          delay(300),
          concatMap((user: UserSummary) => 
            this.getHrSummary().pipe(
              map((hrSummary: HrSummary) => ({ userSummary: user, hrSummary }))
            )
          )
        )
      ),
      concatMap((data: { userSummary: UserSummary; hrSummary: HrSummary }) => 
        // Wait 300ms, then fetch Finance Summary
        of(data).pipe(
          delay(300),
          concatMap((prev: { userSummary: UserSummary; hrSummary: HrSummary }) => 
            this.getFinanceSummary().pipe(
              map((financeSummary: FinanceSummary) => ({ ...prev, financeSummary }))
            )
          )
        )
      ),
      concatMap((data: { userSummary: UserSummary; hrSummary: HrSummary; financeSummary: FinanceSummary }) => 
        // Wait 300ms, then fetch Inventory Summary
        of(data).pipe(
          delay(300),
          concatMap((prev: { userSummary: UserSummary; hrSummary: HrSummary; financeSummary: FinanceSummary }) => 
            this.getInventorySummary().pipe(
              map((inventorySummary: InventorySummary) => ({ ...prev, inventorySummary }))
            )
          )
        )
      )
    );
  }

  private getUserSummary(): Observable<UserSummary> {
    return this.http.get<UserSummary>(`${this.baseUrl}${environment.endpoints.users}/summary`);
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
