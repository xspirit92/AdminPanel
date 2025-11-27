import { Component, Input, Output, EventEmitter, OnInit, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { BadgeComponent } from '../ui/badge/badge.component';
import { TableDropdownComponent } from '../common/table-dropdown/table-dropdown.component';
import { SelectComponent } from '../form/select/select.component';

// Интерфейсы для типизации
export interface SortConfig {
  key: string;
  asc: boolean;
}

export type BadgeColor = 'success' | 'error' | 'warning' | 'info';
export type ColumnType = 'text' | 'badge' | 'currency' | 'date' | 'actions';

export interface TableColumn<T = any> {
  key: keyof T | string;
  label: string;
  sortable?: boolean;
  width?: string;
  type?: ColumnType;
  badgeColorMap?: { [key: string]: BadgeColor };
  format?: (value: any, item?: T) => string;
  cellClass?: string | ((item: T) => string);
}

export interface PageSizeOption {
  value: string;
  label: string;
}

export interface TableActionEvent<T = any> {
  item: T;
  action: 'edit' | 'delete';
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
export class BaseTableComponent<T = any> implements OnInit, OnChanges {
  // Inputs
  @Input() columns: TableColumn<T>[] = [];
  @Input() data: T[] = [];
  @Input() loading = false;
  @Input() totalItems = 0;
  @Input() currentPage = 1;
  @Input() pageSize = 10;
  @Input() totalPages = 0;
  @Input() sortConfig: SortConfig = { key: '', asc: false };
  @Input() emptyStateMessage = 'Нет данных';
  @Input() loadingMessage = 'Загрузка...';
  @Input() showPagination = true;
  @Input() rowClass: string | ((item: T, index: number) => string) = '';

  // Outputs
  @Output() sortChange = new EventEmitter<SortConfig>();
  @Output() pageChange = new EventEmitter<number>();
  @Output() pageSizeChange = new EventEmitter<number>();
  @Output() editItem = new EventEmitter<T>();
  @Output() deleteItem = new EventEmitter<T>();

  // Конфигурация
  readonly pageSizeOptions: PageSizeOption[] = [
    { value: '10', label: '10' },
    { value: '25', label: '25' },
    { value: '50', label: '50' },
    { value: '100', label: '100' }
  ];

  // Состояние
  selectedPageSize = '10';
  private readonly defaultCurrencyConfig = {
    style: 'currency',
    currency: 'RUB',
    minimumFractionDigits: 0
  } as const;

  ngOnInit(): void {
    this.initializePageSize();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['pageSize']) {
      this.initializePageSize();
    }
  }

  /**
   * Публичные методы - обработчики событий
   */
  onSort(column: TableColumn<T>): void {
    if (!column.sortable) return;

    const newSort: SortConfig = {
      key: column.key as string,
      asc: this.sortConfig.key === column.key ? !this.sortConfig.asc : false
    };

    this.sortChange.emit(newSort);
  }

  onPageChange(page: number): void {
    if (this.isValidPage(page)) {
      this.pageChange.emit(page);
    }
  }

  onPageSizeChange(size: string): void {
    const pageSize = parseInt(size, 10);
    if (!isNaN(pageSize) && pageSize > 0) {
      this.selectedPageSize = size;
      this.pageSizeChange.emit(pageSize);
    }
  }

  onEdit(item: T): void {
    this.editItem.emit(item);
  }

  onDelete(item: T): void {
    this.deleteItem.emit(item);
  }

  /**
   * Вспомогательные методы для шаблона
   */
  getSortIconClass(columnKey: string): string {
    if (this.sortConfig.key !== columnKey) {
      return 'text-gray-300 dark:text-gray-400/50';
    }
    return this.sortConfig.asc ? 'text-gray-500 dark:text-gray-400' : 'text-gray-500 dark:text-gray-400';
  }

  getAriaSort(column: TableColumn<T>): 'ascending' | 'descending' | 'none' | null {
    if (!column.sortable) return null;

    if (this.sortConfig.key === column.key) {
      return this.sortConfig.asc ? 'ascending' : 'descending';
    }

    return 'none';
  }

  getSortAriaLabel(column: TableColumn<T>): string {
    if (!column.sortable) return '';

    if (this.sortConfig.key === column.key) {
      return this.sortConfig.asc ? 'Сортировка по возрастанию' : 'Сортировка по убыванию';
    }

    return 'Сортировать';
  }

  getBadgeColor(column: TableColumn<T>, value: any): BadgeColor {
    return column.badgeColorMap?.[value] || 'error';
  }

  formatValue(column: TableColumn<T>, item: T): string {
    const value = this.getNestedValue(item, column.key as string);

    try {
      if (column.format) {
        return column.format(value, item);
      }

      return this.formatByType(column.type, value);
    } catch (error) {
      console.warn(`Error formatting value for column ${column.key.toString()}:`, error);
      return '—';
    }
  }

  getCellClass(column: TableColumn<T>, item: T): string {
    if (typeof column.cellClass === 'function') {
      return column.cellClass(item);
    }
    return column.cellClass || '';
  }

  getRowClass(item: T, index: number): string {
    if (typeof this.rowClass === 'function') {
      return this.rowClass(item, index);
    }
    return this.rowClass;
  }

  getNestedValue(obj: any, path: string): any {
    if (!obj || !path) return undefined;

    return path.split('.').reduce((current, key) => {
      return current && current[key] !== undefined ? current[key] : undefined;
    }, obj);
  }

  /**
   * Геттеры для пагинации
   */
  get startItem(): number {
    return this.totalItems === 0 ? 0 : (this.currentPage - 1) * this.pageSize + 1;
  }

  get endItem(): number {
    return Math.min(this.currentPage * this.pageSize, this.totalItems);
  }

  get visiblePages(): number[] {
    if (this.totalPages <= 1) return [1];

    const pages: number[] = [];
    const maxVisiblePages = 5;
    let startPage = Math.max(1, this.currentPage - Math.floor(maxVisiblePages / 2));
    let endPage = Math.min(this.totalPages, startPage + maxVisiblePages - 1);

    // Adjust if we're at the beginning
    if (endPage - startPage + 1 < maxVisiblePages) {
      startPage = Math.max(1, endPage - maxVisiblePages + 1);
    }

    for (let i = startPage; i <= endPage; i++) {
      pages.push(i);
    }

    return pages;
  }

  get hasPreviousPage(): boolean {
    return this.currentPage > 1;
  }

  get hasNextPage(): boolean {
    return this.currentPage < this.totalPages;
  }

  /**
   * Приватные методы
   */
  private initializePageSize(): void {
    this.selectedPageSize = this.pageSize.toString();
  }

  private isValidPage(page: number): boolean {
    return page >= 1 && page <= this.totalPages;
  }

  private formatByType(type: ColumnType | undefined, value: any): string {
    if (value === null || value === undefined) return '—';

    switch (type) {
      case 'currency':
        return this.formatCurrency(value);
      case 'date':
        return this.formatDate(value);
      case 'badge':
      case 'text':
      default:
        return this.formatText(value);
    }
  }

  private formatCurrency(value: number): string {
    try {
      return new Intl.NumberFormat('ru-RU', this.defaultCurrencyConfig).format(value || 0);
    } catch {
      return `${value}`;
    }
  }

  private formatDate(value: string | Date): string {
    try {
      const date = typeof value === 'string' ? new Date(value) : value;
      return date.toLocaleDateString('ru-RU');
    } catch {
      return 'Неверная дата';
    }
  }

  private formatText(value: any): string {
    if (value === null || value === undefined) return '—';

    const stringValue = value.toString();
    return stringValue || '—';
  }

  /**
   * Методы для отслеживания в *ngFor (улучшение производительности)
   */
  trackByColumn(index: number, column: TableColumn<T>): string {
    return `${column.key.toString()}-${index}`;
  }

  trackByItem(index: number, item: T): any {
    // Если у элемента есть id, используем его для лучшей производительности
    const id = (item as any).id;
    return id !== undefined ? id : index;
  }

  /**
   * Вспомогательные методы для доступности (a11y)
   */
  getPaginationAriaLabel(page: number): string {
    return `Перейти на страницу ${page}`;
  }
}