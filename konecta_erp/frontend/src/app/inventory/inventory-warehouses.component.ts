import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { InventoryApiService, WarehouseDto } from '../core/services/inventory-api.service';

@Component({
  selector: 'app-inventory-warehouses',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './inventory-warehouses.component.html',
  styleUrls: ['./inventory-warehouses.component.scss']
})
export class InventoryWarehousesComponent implements OnInit {
  private readonly inventoryApi = inject(InventoryApiService);

  readonly warehouses = signal<WarehouseDto[]>([]);
  readonly loading = signal(false);

  ngOnInit(): void {
    this.loadWarehouses();
  }

  private loadWarehouses(): void {
    this.loading.set(true);

    this.inventoryApi.getWarehouses().subscribe({
      next: warehouses => {
        this.warehouses.set(warehouses);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
      }
    });
  }
}

