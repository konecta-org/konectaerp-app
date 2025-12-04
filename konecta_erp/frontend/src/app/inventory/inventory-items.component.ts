import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { InventoryApiService, InventoryItemDto, InventoryItemUpsertDto } from '../core/services/inventory-api.service';

@Component({
  selector: 'app-inventory-items',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './inventory-items.component.html',
  styleUrls: ['./inventory-items.component.scss']
})
export class InventoryItemsComponent implements OnInit {
  private readonly inventoryApi = inject(InventoryApiService);
  private readonly fb = inject(FormBuilder);

  readonly items = signal<InventoryItemDto[]>([]);
  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly error = signal<string | null>(null);
  readonly formVisible = signal(false);
  readonly formMode = signal<'create' | 'edit'>('create');
  readonly selectedItem = signal<InventoryItemDto | null>(null);

  readonly itemForm = this.fb.nonNullable.group({
    sku: ['', Validators.required],
    name: ['', Validators.required],
    description: [''],
    category: [''],
    status: ['Active', Validators.required],
    unitOfMeasure: ['Each', Validators.required],
    safetyStockLevel: [0, [Validators.min(0)]],
    reorderPoint: [0, [Validators.min(0)]],
    standardCost: [0, [Validators.min(0)]],
    unitPrice: [0, [Validators.min(0)]]
  });

  ngOnInit(): void {
    this.loadItems();
  }

  reload(): void {
    this.loadItems();
  }

  openCreate(): void {
    this.formMode.set('create');
    this.selectedItem.set(null);
    this.error.set(null);
    this.itemForm.reset({
      sku: '',
      name: '',
      description: '',
      category: '',
      status: 'Active',
      unitOfMeasure: 'Each',
      safetyStockLevel: 0,
      reorderPoint: 0,
      standardCost: 0,
      unitPrice: 0
    });
    this.formVisible.set(true);
  }

  openEdit(item: InventoryItemDto): void {
    this.formMode.set('edit');
    this.selectedItem.set(item);
    this.error.set(null);
    this.itemForm.reset({
      sku: item.sku,
      name: item.name,
      description: item.description ?? '',
      category: item.category ?? '',
      status: item.status,
      unitOfMeasure: item.unitOfMeasure,
      safetyStockLevel: item.safetyStockLevel,
      reorderPoint: item.reorderPoint,
      standardCost: item.standardCost,
      unitPrice: item.unitPrice
    });
    this.formVisible.set(true);
  }

  closeForm(): void {
    this.formVisible.set(false);
    this.selectedItem.set(null);
    this.error.set(null);
  }

  submitForm(): void {
    if (this.itemForm.invalid) {
      this.itemForm.markAllAsTouched();
      return;
    }

    const raw = this.itemForm.getRawValue();
    const payload: InventoryItemUpsertDto = {
      sku: raw.sku,
      name: raw.name,
      description: raw.description || null,
      category: raw.category || null,
      status: raw.status,
      unitOfMeasure: raw.unitOfMeasure,
      safetyStockLevel: raw.safetyStockLevel ?? 0,
      reorderPoint: raw.reorderPoint ?? 0,
      standardCost: raw.standardCost ?? 0,
      unitPrice: raw.unitPrice ?? 0,
      stockLevels: []
    };

    this.saving.set(true);
    this.error.set(null);

    const mode = this.formMode();
    const selected = this.selectedItem();

    if (mode === 'create' || !selected) {
      this.inventoryApi.createItem(payload).subscribe({
        next: () => {
          this.saving.set(false);
          this.formVisible.set(false);
          this.selectedItem.set(null);
          this.loadItems();
        },
        error: () => {
          this.saving.set(false);
          this.error.set('Unable to save inventory item.');
        }
      });
    } else {
      this.inventoryApi.updateItem(selected.id, payload).subscribe({
        next: () => {
          this.saving.set(false);
          this.formVisible.set(false);
          this.selectedItem.set(null);
          this.loadItems();
        },
        error: () => {
          this.saving.set(false);
          this.error.set('Unable to save inventory item.');
        }
      });
    }
  }

  deleteItem(item: InventoryItemDto): void {
    if (!confirm(`Delete inventory item "${item.sku} - ${item.name}"?`)) {
      return;
    }

    this.loading.set(true);
    this.inventoryApi.deleteItem(item.id).subscribe({
      next: () => {
        this.loadItems();
      },
      error: () => {
        this.loading.set(false);
        this.error.set('Unable to delete inventory item.');
      }
    });
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
