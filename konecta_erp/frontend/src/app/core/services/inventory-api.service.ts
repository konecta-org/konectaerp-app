import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface InventorySummaryDto {
  totalActiveItems: number;
  totalWarehouses: number;
  totalQuantityOnHand: number;
  totalQuantityReserved: number;
  itemsBelowSafetyStock: number;
}

export interface InventoryStockLevelDto {
  warehouseId: number;
  warehouseCode: string;
  warehouseName: string;
  quantityOnHand: number;
  quantityReserved: number;
  availableQuantity: number;
  reorderQuantity: number;
}

export interface InventoryItemDto {
  id: number;
  sku: string;
  name: string;
  description?: string | null;
  category?: string | null;
  status: string;
  unitOfMeasure: string;
  safetyStockLevel: number;
  reorderPoint: number;
  standardCost: number;
  unitPrice: number;
  totalOnHand: number;
  totalReserved: number;
  totalAvailable: number;
  stockLevels: InventoryStockLevelDto[];
}

export interface WarehouseDto {
  id: number;
  code: string;
  name: string;
  location?: string | null;
  contactEmail?: string | null;
  status: string;
  totalOnHand: number;
  totalReserved: number;
  totalAvailable: number;
}

export interface StockTransactionDto {
  id: number;
  inventoryItemId: number;
  itemSku: string;
  itemName: string;
  warehouseId: number;
  warehouseCode: string;
  warehouseName: string;
  quantity: number;
  transactionType: string;
  referenceNumber?: string | null;
  performedBy?: string | null;
  occurredAtUtc: string;
  notes?: string | null;
}

@Injectable({
  providedIn: 'root'
})
export class InventoryApiService {
  private readonly baseUrl = `${environment.apiBaseUrl}${environment.endpoints.inventory}`;

  constructor(private readonly http: HttpClient) {}

  getSummary(): Observable<InventorySummaryDto> {
    return this.http.get<InventorySummaryDto>(`${this.baseUrl}/inventory-summary`);
  }

  getItems(category?: string): Observable<InventoryItemDto[]> {
    const params = category ? new HttpParams().set('category', category) : undefined;
    return this.http.get<InventoryItemDto[]>(`${this.baseUrl}/InventoryItems`, { params });
  }

  getWarehouses(): Observable<WarehouseDto[]> {
    return this.http.get<WarehouseDto[]>(`${this.baseUrl}/Warehouses`);
  }

  getRecentTransactions(options?: { page?: number; pageSize?: number; itemId?: number; warehouseId?: number }): Observable<StockTransactionDto[]> {
    let params = new HttpParams();
    const page = options?.page ?? 1;
    const pageSize = options?.pageSize ?? 25;

    params = params.set('page', String(page)).set('pageSize', String(pageSize));

    if (options?.itemId != null) {
      params = params.set('itemId', String(options.itemId));
    }

    if (options?.warehouseId != null) {
      params = params.set('warehouseId', String(options.warehouseId));
    }

    return this.http.get<StockTransactionDto[]>(`${this.baseUrl}/StockTransactions`, { params });
  }
}

