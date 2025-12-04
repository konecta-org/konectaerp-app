import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { InventoryApiService, StockTransactionDto } from '../core/services/inventory-api.service';

@Component({
  selector: 'app-inventory-transactions',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './inventory-transactions.component.html',
  styleUrls: ['./inventory-transactions.component.scss']
})
export class InventoryTransactionsComponent implements OnInit {
  private readonly inventoryApi = inject(InventoryApiService);

  readonly transactions = signal<StockTransactionDto[]>([]);
  readonly loading = signal(false);

  ngOnInit(): void {
    this.loadTransactions();
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
}

