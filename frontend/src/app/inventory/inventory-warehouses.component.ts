import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { InventoryApiService, WarehouseDto, WarehouseUpsertDto } from '../core/services/inventory-api.service';

@Component({
  selector: 'app-inventory-warehouses',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './inventory-warehouses.component.html',
  styleUrls: ['./inventory-warehouses.component.scss']
})
export class InventoryWarehousesComponent implements OnInit {
  private readonly inventoryApi = inject(InventoryApiService);
  private readonly fb = inject(FormBuilder);

  readonly warehouses = signal<WarehouseDto[]>([]);
  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly error = signal<string | null>(null);
  readonly formVisible = signal(false);
  readonly formMode = signal<'create' | 'edit'>('create');
  readonly selectedWarehouse = signal<WarehouseDto | null>(null);

  readonly warehouseForm = this.fb.nonNullable.group({
    code: ['', Validators.required],
    name: ['', Validators.required],
    location: [''],
    contactEmail: [''],
    status: ['Active', Validators.required]
  });

  ngOnInit(): void {
    this.loadWarehouses();
  }

  reload(): void {
    this.loadWarehouses();
  }

  openCreate(): void {
    this.formMode.set('create');
    this.selectedWarehouse.set(null);
    this.error.set(null);
    this.warehouseForm.reset({
      code: '',
      name: '',
      location: '',
      contactEmail: '',
      status: 'Active'
    });
    this.formVisible.set(true);
  }

  openEdit(warehouse: WarehouseDto): void {
    this.formMode.set('edit');
    this.selectedWarehouse.set(warehouse);
    this.error.set(null);
    this.warehouseForm.reset({
      code: warehouse.code,
      name: warehouse.name,
      location: warehouse.location ?? '',
      contactEmail: warehouse.contactEmail ?? '',
      status: warehouse.status
    });
    this.formVisible.set(true);
  }

  closeForm(): void {
    this.formVisible.set(false);
    this.selectedWarehouse.set(null);
    this.error.set(null);
  }

  submitForm(): void {
    if (this.warehouseForm.invalid) {
      this.warehouseForm.markAllAsTouched();
      return;
    }

    const raw = this.warehouseForm.getRawValue();
    const payload: WarehouseUpsertDto = {
      code: raw.code,
      name: raw.name,
      location: raw.location || null,
      contactEmail: raw.contactEmail || null,
      status: raw.status
    };

    this.saving.set(true);
    this.error.set(null);

    const mode = this.formMode();
    const selected = this.selectedWarehouse();

    if (mode === 'create' || !selected) {
      this.inventoryApi.createWarehouse(payload).subscribe({
        next: () => {
          this.saving.set(false);
          this.formVisible.set(false);
          this.selectedWarehouse.set(null);
          this.loadWarehouses();
        },
        error: () => {
          this.saving.set(false);
          this.error.set('Unable to save warehouse.');
        }
      });
    } else {
      this.inventoryApi.updateWarehouse(selected.id, payload).subscribe({
        next: () => {
          this.saving.set(false);
          this.formVisible.set(false);
          this.selectedWarehouse.set(null);
          this.loadWarehouses();
        },
        error: () => {
          this.saving.set(false);
          this.error.set('Unable to save warehouse.');
        }
      });
    }
  }

  deleteWarehouse(warehouse: WarehouseDto): void {
    if (!confirm(`Delete warehouse "${warehouse.code} - ${warehouse.name}"?`)) {
      return;
    }

    this.loading.set(true);
    this.inventoryApi.deleteWarehouse(warehouse.id).subscribe({
      next: () => {
        this.loadWarehouses();
      },
      error: () => {
        this.loading.set(false);
        this.error.set('Unable to delete warehouse.');
      }
    });
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
