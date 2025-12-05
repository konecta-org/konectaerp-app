import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import {
  InventoryApiService,
  InventoryItemDto,
  StockAdjustmentRequestDto,
  StockTransactionDto,
  StockTransferRequestDto,
  WarehouseDto
} from '../core/services/inventory-api.service';

@Component({
  selector: 'app-inventory-transactions',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './inventory-transactions.component.html',
  styleUrls: ['./inventory-transactions.component.scss']
})
export class InventoryTransactionsComponent implements OnInit {
  private readonly inventoryApi = inject(InventoryApiService);
  private readonly fb = inject(FormBuilder);

  readonly transactions = signal<StockTransactionDto[]>([]);
  readonly loading = signal(false);
  readonly items = signal<InventoryItemDto[]>([]);
  readonly warehouses = signal<WarehouseDto[]>([]);
  readonly transactionTypes = signal<string[]>([]);
  readonly submittingAdjust = signal(false);
  readonly submittingTransfer = signal(false);
  readonly error = signal<string | null>(null);

  readonly adjustForm = this.fb.nonNullable.group({
    inventoryItemId: [0, Validators.required],
    warehouseId: [0, Validators.required],
    transactionType: ['', Validators.required],
    increase: [true],
    quantity: [0, [Validators.required, Validators.min(0.01)]],
    referenceNumber: [''],
    performedBy: [''],
    notes: ['']
  });

  readonly transferForm = this.fb.nonNullable.group({
    inventoryItemId: [0, Validators.required],
    fromWarehouseId: [0, Validators.required],
    toWarehouseId: [0, Validators.required],
    quantity: [0, [Validators.required, Validators.min(0.01)]],
    referenceNumber: [''],
    performedBy: [''],
    notes: ['']
  });

  ngOnInit(): void {
    this.loadTransactions();
    this.loadSupportData();
  }

  reload(): void {
    this.loadTransactions();
  }

  submitAdjust(): void {
    if (this.adjustForm.invalid) {
      this.adjustForm.markAllAsTouched();
      return;
    }

    const raw = this.adjustForm.getRawValue();
    const payload: StockAdjustmentRequestDto = {
      inventoryItemId: raw.inventoryItemId,
      warehouseId: raw.warehouseId,
      quantity: raw.quantity,
      transactionType: raw.transactionType,
      increase: raw.increase,
      referenceNumber: raw.referenceNumber || null,
      performedBy: raw.performedBy || null,
      notes: raw.notes || null
    };

    this.submittingAdjust.set(true);
    this.error.set(null);

    this.inventoryApi.adjustStock(payload).subscribe({
      next: () => {
        this.submittingAdjust.set(false);
        this.adjustForm.patchValue({ quantity: 0, referenceNumber: '', notes: '' });
        this.loadTransactions();
      },
      error: () => {
        this.submittingAdjust.set(false);
        this.error.set('Unable to record stock adjustment.');
      }
    });
  }

  submitTransfer(): void {
    if (this.transferForm.invalid) {
      this.transferForm.markAllAsTouched();
      return;
    }

    const raw = this.transferForm.getRawValue();
    const payload: StockTransferRequestDto = {
      inventoryItemId: raw.inventoryItemId,
      fromWarehouseId: raw.fromWarehouseId,
      toWarehouseId: raw.toWarehouseId,
      quantity: raw.quantity,
      referenceNumber: raw.referenceNumber || null,
      performedBy: raw.performedBy || null,
      notes: raw.notes || null
    };

    this.submittingTransfer.set(true);
    this.error.set(null);

    this.inventoryApi.transferStock(payload).subscribe({
      next: () => {
        this.submittingTransfer.set(false);
        this.transferForm.patchValue({ quantity: 0, referenceNumber: '', notes: '' });
        this.loadTransactions();
      },
      error: () => {
        this.submittingTransfer.set(false);
        this.error.set('Unable to record stock transfer.');
      }
    });
  }

  private loadTransactions(): void {
    this.loading.set(true);

    this.inventoryApi.getRecentTransactions({ page: 1, pageSize: 25 }).subscribe({
      next: transactions => {
        this.transactions.set(transactions);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
      }
    });
  }

  private loadSupportData(): void {
    this.inventoryApi.getItems().subscribe({
      next: items => this.items.set(items),
      error: () => undefined
    });

    this.inventoryApi.getWarehouses().subscribe({
      next: warehouses => this.warehouses.set(warehouses),
      error: () => undefined
    });

    this.inventoryApi.getTransactionTypes().subscribe({
      next: types => this.transactionTypes.set(types),
      error: () => undefined
    });
  }
}
