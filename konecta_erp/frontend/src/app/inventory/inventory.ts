import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { InventoryApiService, InventorySummaryDto } from '../core/services/inventory-api.service';

@Component({
  selector: 'app-inventory',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './inventory.html',
  styleUrl: './inventory.scss'
})
export class Inventory implements OnInit {
  private readonly inventoryApi = inject(InventoryApiService);
  private readonly router = inject(Router);

  readonly summary = signal<InventorySummaryDto | null>(null);
  readonly loadingSummary = signal(false);
  readonly loadError = signal<string | null>(null);

  readonly tiles = [
    {
      key: 'items',
      title: 'Items & stock',
      description: 'Browse catalog items, quantities on hand, and safety stock.',
      route: '/inventory/items',
      icon: 'IT',
      accent: 'linear-gradient(135deg, #22c55e, #16a34a)'
    },
    {
      key: 'warehouses',
      title: 'Warehouses',
      description: 'Review warehouse capacity, locations, and available inventory.',
      route: '/inventory/warehouses',
      icon: 'WH',
      accent: 'linear-gradient(135deg, #0ea5e9, #0369a1)'
    },
    {
      key: 'transactions',
      title: 'Stock movements',
      description: 'Track recent stock adjustments, receipts, and transfers.',
      route: '/inventory/transactions',
      icon: 'TX',
      accent: 'linear-gradient(135deg, #f97316, #ea580c)'
    }
  ];

  ngOnInit(): void {
    this.loadSummary();
  }

  navigate(tile: { route: string }): void {
    void this.router.navigate([tile.route]);
  }

  private loadSummary(): void {
    this.loadingSummary.set(true);
    this.loadError.set(null);

    this.inventoryApi.getSummary().subscribe({
      next: summary => {
        this.summary.set(summary);
        this.loadingSummary.set(false);
      },
      error: () => {
        this.loadingSummary.set(false);
        this.loadError.set('Unable to load inventory summary data.');
      }
    });
  }
}
