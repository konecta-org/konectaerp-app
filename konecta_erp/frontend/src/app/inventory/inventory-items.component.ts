import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { InventoryApiService, InventoryItemDto } from '../core/services/inventory-api.service';

@Component({
  selector: 'app-inventory-items',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './inventory-items.component.html',
  styleUrls: ['./inventory-items.component.scss']
})
export class InventoryItemsComponent implements OnInit {
  private readonly inventoryApi = inject(InventoryApiService);

  readonly items = signal<InventoryItemDto[]>([]);
  readonly loading = signal(false);

  ngOnInit(): void {
    this.loadItems();
  }

  private loadItems(): void {
    this.loading.set(true);

    this.inventoryApi.getItems().subscribe({
      next: items => {
        this.items.set(items);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
      }
    });
  }
}

