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

export interface InventoryStockLevelUpsertDto {
  warehouseId: number;
  quantityOnHand: number;
  quantityReserved: number;
  reorderQuantity: number;
}

export interface InventoryItemUpsertDto {
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
  stockLevels: InventoryStockLevelUpsertDto[];
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

export interface WarehouseUpsertDto {
  code: string;
  name: string;
  location?: string | null;
  contactEmail?: string | null;
  status: string;
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

export interface StockAdjustmentRequestDto {
  inventoryItemId: number;
  warehouseId: number;
  quantity: number;
  transactionType: string;
  increase: boolean;
  referenceNumber?: string | null;
  performedBy?: string | null;
  notes?: string | null;
}

export interface StockTransferRequestDto {
  inventoryItemId: number;
  fromWarehouseId: number;
  toWarehouseId: number;
  quantity: number;
  referenceNumber?: string | null;
  performedBy?: string | null;
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

  getItem(id: number): Observable<InventoryItemDto> {
    return this.http.get<InventoryItemDto>(`${this.baseUrl}/InventoryItems/${id}`);
  }

  createItem(request: InventoryItemUpsertDto): Observable<InventoryItemDto> {
    return this.http.post<InventoryItemDto>(`${this.baseUrl}/InventoryItems`, request);
  }

  updateItem(id: number, request: InventoryItemUpsertDto): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/InventoryItems/${id}`, request);
  }

  deleteItem(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/InventoryItems/${id}`);
  }

  getWarehouses(): Observable<WarehouseDto[]> {
    return this.http.get<WarehouseDto[]>(`${this.baseUrl}/Warehouses`);
  }

  getWarehouse(id: number): Observable<WarehouseDto> {
    return this.http.get<WarehouseDto>(`${this.baseUrl}/Warehouses/${id}`);
  }

  createWarehouse(request: WarehouseUpsertDto): Observable<WarehouseDto> {
    return this.http.post<WarehouseDto>(`${this.baseUrl}/Warehouses`, request);
  }

  updateWarehouse(id: number, request: WarehouseUpsertDto): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/Warehouses/${id}`, request);
  }

  deleteWarehouse(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/Warehouses/${id}`);
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

  getTransactionTypes(): Observable<string[]> {
    return this.http.get<string[]>(`${this.baseUrl}/stock-operations/transaction-types`);
  }

  adjustStock(request: StockAdjustmentRequestDto): Observable<StockTransactionDto> {
    return this.http.post<StockTransactionDto>(`${this.baseUrl}/stock-operations/adjust`, request);
  }

  transferStock(request: StockTransferRequestDto): Observable<StockTransactionDto[]> {
    return this.http.post<StockTransactionDto[]>(`${this.baseUrl}/stock-operations/transfer`, request);
  }
}
