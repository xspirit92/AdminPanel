import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SortConfig } from '../../services/base-data.service';
import { BadgeComponent } from '../ui/badge/badge.component';
import { TableDropdownComponent } from '../common/table-dropdown/table-dropdown.component';
import { SelectComponent } from '../form/select/select.component';

export interface TableColumn {
  key: string;
  label: string;
  sortable?: boolean;
  width?: string;
  type?: 'text' | 'badge' | 'currency' | 'date' | 'actions';
  badgeColorMap?: { [key: string]: 'success' | 'error' | 'warning' | 'info' };
  format?: (value: any) => string;
}

@Component({
  selector: 'app-base-table',
  standalone: true,
  imports: [
    CommonModule, 
    FormsModule,
    BadgeComponent,
    TableDropdownComponent,
    SelectComponent
  ],
  templateUrl: './base-table.component.html'
})
export class BaseTableComponent implements OnInit {
  @Input() columns: TableColumn[] = [];
  @Input() data: any[] = [];
  @Input() loading = false;
  @Input() totalItems = 0;
  @Input() currentPage = 1;
  @Input() pageSize = 10;
  @Input() totalPages = 0;
  @Input() sortConfig: SortConfig = { key: '', asc: false };
  
  @Output() sortChange = new EventEmitter<SortConfig>();
  @Output() pageChange = new EventEmitter<number>();
  @Output() pageSizeChange = new EventEmitter<number>();
  @Output() editItem = new EventEmitter<any>();
  @Output() deleteItem = new EventEmitter<any>();

  pageSizeOptions = [
    { value: '10', label: '10' },
    { value: '25', label: '25' },
    { value: '50', label: '50' },
    { value: '100', label: '100' }
  ];

  selectedPageSize = '10';

  ngOnInit() {
    this.selectedPageSize = this.pageSize.toString();
  }

  onSort(column: TableColumn): void {
    if (!column.sortable) return;

    const newSort: SortConfig = {
      key: column.key,
      asc: this.sortConfig.key === column.key ? !this.sortConfig.asc : false
    };

    this.sortChange.emit(newSort);
  }

  onPageChange(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.pageChange.emit(page);
    }
  }

  onPageSizeChange(size: string): void {
    const pageSize = parseInt(size, 10);
    this.selectedPageSize = size; // Обновляем выбранное значение
    this.pageSizeChange.emit(pageSize);
  }

  onEdit(item: any): void {
    this.editItem.emit(item);
  }

  onDelete(item: any): void {
    this.deleteItem.emit(item);
  }

  getSortIconClass(columnKey: string): string {
    if (this.sortConfig.key !== columnKey) {
      return 'text-gray-300 dark:text-gray-400/50';
    }
    return this.sortConfig.asc ? 'text-gray-500 dark:text-gray-400' : 'text-gray-500 dark:text-gray-400';
  }

  getBadgeColor(column: TableColumn, value: any): 'success' | 'error' | 'warning' | 'info' {
    return column.badgeColorMap?.[value] || 'error';
  }

  formatValue(column: TableColumn, value: any): string {
    if (column.format) {
      return column.format(value);
    }

    switch (column.type) {
      case 'currency':
        return new Intl.NumberFormat('ru-RU', {
          style: 'currency',
          currency: 'RUB',
          minimumFractionDigits: 0
        }).format(value || 0);
      case 'date':
        return value ? new Date(value).toLocaleDateString('ru-RU') : '';
      default:
        return value?.toString() || '';
    }
  }

  get startItem(): number {
    return this.totalItems === 0 ? 0 : (this.currentPage - 1) * this.pageSize + 1;
  }

  get endItem(): number {
    return Math.min(this.currentPage * this.pageSize, this.totalItems);
  }
}