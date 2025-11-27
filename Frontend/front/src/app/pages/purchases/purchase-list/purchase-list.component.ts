import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Subject, takeUntil, debounceTime, distinctUntilChanged } from 'rxjs';
import { PurchaseStatusLabels, PurchaseStatusEnum } from '../../../core/models/purchase.models';
import { BadgeComponent } from '../../../shared/components/ui/badge/badge.component';
import { PageBreadcrumbComponent } from '../../../shared/components/common/page-breadcrumb/page-breadcrumb.component';
import { LabelComponent } from '../../../shared/components/form/label/label.component';
import { DateRangePickerComponent } from '../../../shared/components/date-range-picker/date-range-picker.component';
import { SelectComponent } from '../../../shared/components/form/select/select.component';
import { ModalComponent } from '../../../shared/components/ui/modal/modal.component';
import { ButtonComponent } from '../../../shared/components/ui/button/button.component';
import { 
  ApiPurchaseGetRequestParams, 
  ApiPurchaseIdDeleteRequestParams, 
  FacilityDto, 
  FacilityService, 
  ProductDto, 
  ProductService, 
  PurchaseDto, 
  PurchaseService, 
  SupplierDto, 
  SupplierService 
} from '../../../api';
import { 
  BaseFiltersComponent, 
  FilterField, 
  FilterValues,
  FilterOption 
} from '../../../shared/components/base-filters/base-filters.component';
import { BaseTableComponent, TableColumn, SortConfig } from '../../../shared/components/base-table/base-table.component';
import { SearchComponent } from '../../../shared/components/search/search.component';

@Component({
  selector: 'app-purchase-list',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    FormsModule,
    PageBreadcrumbComponent,
    BaseFiltersComponent,
    BaseTableComponent,
    SearchComponent,
    ModalComponent,
    BadgeComponent
  ],
  templateUrl: './purchase-list.component.html'
})
export class PurchaseListComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  // Table configuration
  tableColumns: TableColumn[] = [
    {
      key: 'supplierName',
      label: 'Поставщик',
      sortable: true
    },
    {
      key: 'productName',
      label: 'Продукт',
      sortable: true
    },
    {
      key: 'facilityName',
      label: 'Объект',
      sortable: true
    },
    {
      key: 'quantity',
      label: 'Количество',
      sortable: true,
      type: 'text'
    },
    {
      key: 'amount',
      label: 'Сумма',
      sortable: true,
      type: 'currency'
    },
    {
      key: 'purchaseStatus',
      label: 'Статус',
      sortable: true,
      type: 'badge',
      badgeColorMap: {
        [PurchaseStatusEnum.Pending]: 'warning',
        [PurchaseStatusEnum.Confirmed]: 'info',
        [PurchaseStatusEnum.Completed]: 'success',
        [PurchaseStatusEnum.Cancelled]: 'error'
      },
      format: (value: PurchaseStatusEnum) => PurchaseStatusLabels[value]
    },
    {
      key: 'dateCreated',
      label: 'Дата создания',
      sortable: true,
      type: 'date'
    },
    {
      key: 'actions',
      label: 'Действия',
      type: 'actions',
      width: '100px'
    }
  ];

  // Filter configuration
  filterFields: FilterField[] = [
    {
      key: 'dateRange',
      label: 'Диапазон дат',
      type: 'date-range',
      placeholder: 'Выберите период'
    },
    {
      key: 'supplierId',
      label: 'Поставщик',
      type: 'select',
      placeholder: 'Все поставщики'
    },
    {
      key: 'facilityId',
      label: 'Объект',
      type: 'select',
      placeholder: 'Все объекты'
    },
    {
      key: 'productId',
      label: 'Продукт',
      type: 'select',
      placeholder: 'Все продукты'
    },
    {
      key: 'purchaseStatus',
      label: 'Статус',
      type: 'select',
      placeholder: 'Все статусы'
    }
  ];

  // Data
  purchases: PurchaseDto[] = [];
  loading = false;
  totalItems = 0;
  currentPage = 1;
  pageSize = 10;
  totalPages = 0;

  // State
  sortConfig: SortConfig = { key: 'dateCreated', asc: false };
  filterValues: FilterValues = {};
  searchTerm = '';

  // Modal states
  isFilterModalOpen = false;
  isDeleteModalOpen = false;
  purchaseToDelete: PurchaseDto | null = null;
  resetFiltersTrigger = false;

  // Filter data
  suppliers: SupplierDto[] = [];
  facilities: FacilityDto[] = [];
  products: ProductDto[] = [];

  // Constants
  purchaseStatusLabels = PurchaseStatusLabels;
  purchaseStatusEnum = PurchaseStatusEnum;

  constructor(
    private purchaseService: PurchaseService,
    private supplierService: SupplierService,
    private facilityService: FacilityService,
    private productService: ProductService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.loadFilterData();
    this.loadPurchases();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /**
   * Загрузка данных для фильтров
   */
  loadFilterData(): void {
    // Загрузка поставщиков
    this.supplierService.apiSupplierListGet()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          this.suppliers = response.data || [];
          this.updateFilterOptions();
        },
        error: (error) => {
          console.error('Error loading suppliers:', error);
        }
      });

    // Загрузка объектов
    this.facilityService.apiFacilityListGet()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          this.facilities = response.data || [];
          this.updateFilterOptions();
        },
        error: (error) => {
          console.error('Error loading facilities:', error);
        }
      });

    // Загрузка продуктов
    this.productService.apiProductListGet()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          this.products = response.data || [];
          this.updateFilterOptions();
        },
        error: (error) => {
          console.error('Error loading products:', error);
        }
      });
  }

  /**
   * Обновление опций фильтров
   */
  updateFilterOptions(): void {
    this.filterFields = this.filterFields.map(field => {
      switch (field.key) {
        case 'supplierId':
          return { ...field, options: this.supplierOptions };
        case 'facilityId':
          return { ...field, options: this.facilityOptions };
        case 'productId':
          return { ...field, options: this.productOptions };
        case 'purchaseStatus':
          return { ...field, options: this.statusOptions };
        default:
          return field;
      }
    });
  }

  /**
   * Опции для фильтров
   */
  get statusOptions(): FilterOption[] {
    return [
      { value: '', label: 'Все статусы' },
      { value: PurchaseStatusEnum.Pending.toString(), label: PurchaseStatusLabels[PurchaseStatusEnum.Pending] },
      { value: PurchaseStatusEnum.Confirmed.toString(), label: PurchaseStatusLabels[PurchaseStatusEnum.Confirmed] },
      { value: PurchaseStatusEnum.Completed.toString(), label: PurchaseStatusLabels[PurchaseStatusEnum.Completed] },
      { value: PurchaseStatusEnum.Cancelled.toString(), label: PurchaseStatusLabels[PurchaseStatusEnum.Cancelled] }
    ];
  }

  get supplierOptions(): FilterOption[] {
    return [
      { value: '', label: 'Все поставщики' },
      ...this.suppliers.map(supplier => ({
        value: supplier.id?.toString() || '',
        label: supplier.name || 'Без названия'
      }))
    ];
  }

  get facilityOptions(): FilterOption[] {
    return [
      { value: '', label: 'Все объекты' },
      ...this.facilities.map(facility => ({
        value: facility.id?.toString() || '',
        label: facility.name || 'Без названия'
      }))
    ];
  }

  get productOptions(): FilterOption[] {
    return [
      { value: '', label: 'Все продукты' },
      ...this.products.map(product => ({
        value: product.id?.toString() || '',
        label: product.name || 'Без названия'
      }))
    ];
  }

  /**
   * Загрузка данных закупок
   */
  loadPurchases(): void {
    this.loading = true;

    const filterParams: ApiPurchaseGetRequestParams = {
      pageNumber: this.currentPage,
      pageSize: this.pageSize,
      sortBy: this.sortConfig.key,
      sortDescending: !this.sortConfig.asc,
      ...this.convertFiltersToApiParams(this.filterValues),
      searchTerm: this.searchTerm || undefined
    };

    this.purchaseService.apiPurchaseGet(filterParams)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          const data = response.data || {};
          this.purchases = data.items || [];
          this.totalItems = data.totalCount ?? 0;
          this.totalPages = data.totalPages ?? 0;
          this.loading = false;
        },
        error: (error) => {
          console.error('Error loading purchases:', error);
          this.loading = false;
        }
      });
  }

  /**
   * Преобразование фильтров в параметры API
   */
  private convertFiltersToApiParams(filters: FilterValues): Partial<ApiPurchaseGetRequestParams> {
    const params: Partial<ApiPurchaseGetRequestParams> = {};

    if (filters['supplierId']) {
      params.supplierId = Number(filters['supplierId']);
    }

    if (filters['facilityId']) {
      params.facilityId = Number(filters['facilityId']);
    }

    if (filters['productId']) {
      params.productId = Number(filters['productId']);
    }

    if (filters['purchaseStatus']) {
      params.purchaseStatus = Number(filters['purchaseStatus']) as PurchaseStatusEnum;
    }

    if (filters['dateRange'] && typeof filters['dateRange'] === 'object') {
      const dateRange = filters['dateRange'] as { startDate?: string; endDate?: string };
      if (dateRange.startDate) {
        params.startDate = dateRange.startDate;
      }
      if (dateRange.endDate) {
        params.endDate = dateRange.endDate;
      }
    }

    return params;
  }

  /**
   * Обработчики событий таблицы
   */
  onSortChange(sort: SortConfig): void {
    this.sortConfig = sort;
    this.currentPage = 1;
    this.loadPurchases();
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadPurchases();
  }

  onPageSizeChange(size: number): void {
    this.pageSize = size;
    this.currentPage = 1;
    this.loadPurchases();
  }

  /**
   * Обработчики поиска
   */
  onSearch(term: string): void {
    this.searchTerm = term;
    this.currentPage = 1;
    this.loadPurchases();
  }

  /**
   * Обработчики фильтров (новый API)
   */
  onFiltersChange(filters: FilterValues): void {
    this.filterValues = filters;
  }

  onFiltersApply(): void {
    this.currentPage = 1;
    this.loadPurchases();
  }

  onFiltersReset(): void {
    this.filterValues = {};
    this.searchTerm = '';
    this.currentPage = 1;
    
    // Триггерим сброс в дочерних компонентах
    this.resetFiltersTrigger = true;
    setTimeout(() => {
      this.resetFiltersTrigger = false;
    }, 0);

    this.loadPurchases();
  }

  onFilterModalToggle(isOpen: boolean): void {
    this.isFilterModalOpen = isOpen;
  }

  /**
   * Обработчики действий
   */
  onEditPurchase(purchase: PurchaseDto): void {
    this.router.navigate(['/purchases/edit', purchase.id]);
  }

  onDeletePurchase(purchase: PurchaseDto): void {
    this.openDeleteModal(purchase);
  }

  /**
   * Управление модальным окном удаления
   */
  openDeleteModal(purchase: PurchaseDto): void {
    this.purchaseToDelete = purchase;
    this.isDeleteModalOpen = true;
  }

  closeDeleteModal(): void {
    this.isDeleteModalOpen = false;
    this.purchaseToDelete = null;
  }

  confirmDelete(): void {
    if (this.purchaseToDelete?.id) {
      const request: ApiPurchaseIdDeleteRequestParams = {
        id: this.purchaseToDelete.id
      };

      this.purchaseService.apiPurchaseIdDelete(request)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: () => {
            this.loadPurchases();
            this.closeDeleteModal();
          },
          error: (error) => {
            console.error('Error deleting purchase:', error);
            this.closeDeleteModal();
          }
        });
    }
  }

  /**
   * Вспомогательные методы
   */
  get hasActiveFilters(): boolean {
    return Object.keys(this.filterValues).some(key => {
      const value = this.filterValues[key];
      if (value === null || value === undefined || value === '') {
        return false;
      }
      if (typeof value === 'object' && 'startDate' in value) {
        return !!(value.startDate || value.endDate);
      }
      return true;
    }) || !!this.searchTerm;
  }

  getBadgeColor(status: PurchaseStatusEnum): 'success' | 'error' | 'warning' | 'info' {
    switch (status) {
      case PurchaseStatusEnum.Pending:
        return 'warning';
      case PurchaseStatusEnum.Confirmed:
        return 'info';
      case PurchaseStatusEnum.Completed:
        return 'success';
      case PurchaseStatusEnum.Cancelled:
        return 'error';
      default:
        return 'warning';
    }
  }

  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('ru-RU', {
      style: 'currency',
      currency: 'RUB',
      minimumFractionDigits: 0
    }).format(amount);
  }

  // Геттеры для пагинации
  get startItem(): number {
    return this.totalItems === 0 ? 0 : (this.currentPage - 1) * this.pageSize + 1;
  }

  get endItem(): number {
    return Math.min(this.currentPage * this.pageSize, this.totalItems);
  }
}