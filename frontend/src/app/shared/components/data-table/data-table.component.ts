import { Component, Input, Output, EventEmitter, TemplateRef } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface TableColumn {
  key: string;
  label: string;
  sortable?: boolean;
  template?: TemplateRef<any>;
}

export interface SortEvent {
  column: string;
  direction: 'asc' | 'desc';
}

@Component({
  selector: 'app-data-table',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './data-table.component.html',
  styleUrl: './data-table.component.scss'
})
export class DataTableComponent {
  @Input() columns: TableColumn[] = [];
  @Input() data: any[] = [];
  @Input() loading: boolean = false;
  @Input() totalItems: number = 0;
  @Input() pageSize: number = 20;
  @Input() currentPage: number = 1;
  @Input() showPagination: boolean = true;
  @Input() emptyMessage: string = 'No data available';
  
  @Output() pageChange = new EventEmitter<number>();
  @Output() sortChange = new EventEmitter<SortEvent>();
  @Output() rowClick = new EventEmitter<any>();

  currentSort: { column: string; direction: 'asc' | 'desc' } | null = null;

  get totalPages(): number {
    return Math.ceil(this.totalItems / this.pageSize);
  }

  get pages(): number[] {
    const pages: number[] = [];
    const maxPages = 5;
    let startPage = Math.max(1, this.currentPage - Math.floor(maxPages / 2));
    let endPage = Math.min(this.totalPages, startPage + maxPages - 1);

    if (endPage - startPage < maxPages - 1) {
      startPage = Math.max(1, endPage - maxPages + 1);
    }

    for (let i = startPage; i <= endPage; i++) {
      pages.push(i);
    }
    return pages;
  }

  onSort(column: TableColumn): void {
    if (!column.sortable) return;

    if (this.currentSort?.column === column.key) {
      this.currentSort.direction = this.currentSort.direction === 'asc' ? 'desc' : 'asc';
    } else {
      this.currentSort = { column: column.key, direction: 'asc' };
    }

    this.sortChange.emit(this.currentSort);
  }

  onPageChange(page: number): void {
    if (page < 1 || page > this.totalPages || page === this.currentPage) return;
    this.pageChange.emit(page);
  }

  onRowClick(row: any): void {
    this.rowClick.emit(row);
  }

  getCellValue(row: any, column: TableColumn): any {
    return row[column.key];
  }
}
