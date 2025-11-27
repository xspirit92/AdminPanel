import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { PurchaseStatusLabels, PurchaseStatusEnum } from '../../../core/models/purchase.models';
import { BadgeComponent } from '../../../shared/components/ui/badge/badge.component';
import { PageBreadcrumbComponent } from '../../../shared/components/common/page-breadcrumb/page-breadcrumb.component';
import { LabelComponent } from '../../../shared/components/form/label/label.component';
import { DateRangePickerComponent } from '../../../shared/components/date-range-picker/date-range-picker.component';
import { SelectComponent } from '../../../shared/components/form/select/select.component';
import { ModalComponent } from '../../../shared/components/ui/modal/modal.component';
import { ButtonComponent } from '../../../shared/components/ui/button/button.component';
import { ApiPurchaseGetRequestParams, ApiPurchaseIdDeleteRequestParams, FacilityDto, FacilityService, ProductDto, ProductService, PurchaseDto, PurchaseService, SupplierDto, SupplierService } from '../../../api';
import { BaseFiltersComponent, FilterField } from '../../../shared/components/base-filters/base-filters.component';
import { BaseTableComponent, TableColumn } from '../../../shared/components/base-table/base-table.component';
import { SearchComponent } from '../../../shared/components/search/search.component';

interface SortConfig {
  key: string;
  asc: boolean;
}

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
    LabelComponent,
    SelectComponent,
    DateRangePickerComponent,
    ButtonComponent,
    BadgeComponent
  ],
  templateUrl: './purchase-list.component.html'
})
export class PurchaseListComponent implements OnInit {
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

  // Filter configuration - инициализируем пустыми опциями
  filterFields: FilterField[] = [
    {
      key: 'supplierId',
      label: 'Поставщик',
      type: 'select',
      placeholder: 'Все поставщики',
      options: []
    },
    {
      key: 'facilityId',
      label: 'Объект',
      type: 'select',
      placeholder: 'Все объекты',
      options: []
    },
    {
      key: 'productId',
      label: 'Продукт',
      type: 'select',
      placeholder: 'Все продукты',
      options: []
    },
    {
      key: 'purchaseStatus',
      label: 'Статус',
      type: 'select',
      placeholder: 'Все статусы',
      options: []
    },
    {
      key: 'dateRange',
      label: 'Диапазон дат',
      type: 'date-range',
      placeholder: 'Выберите период'
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
  sortConfig = { key: 'dateCreated', asc: false };
  filterValues: any = {};
  searchTerm = '';

  // Модальное окно фильтров
  isFilterModalOpen = false;

  // Модальное окно подтверждения удаления
  isDeleteModalOpen = false;
  purchaseToDelete: PurchaseDto | null = null;

  // Данные для фильтров
  suppliers: SupplierDto[] = [];
  facilities: FacilityDto[] = [];
  products: ProductDto[] = [];

  // Фильтры из Swagger
  filterSettings = {
    supplierId: undefined as number | undefined,
    facilityId: undefined as number | undefined,
    productId: undefined as number | undefined,
    purchaseStatus: undefined as PurchaseStatusEnum | undefined,
    startDate: undefined as string | undefined,
    endDate: undefined as string | undefined,
    searchTerm: ''
  };
  resetFiltersTrigger = false;

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

  // Загружаем данные для фильтров
  loadFilterData(): void {
    this.supplierService.apiSupplierListGet().subscribe({
      next: (response) => {
        this.suppliers = response.data || [];
        this.updateFilterOptions();
      },
      error: (error) => {
        console.error('Error loading suppliers:', error);
      }
    });

    this.facilityService.apiFacilityListGet().subscribe({
      next: (response) => {
        this.facilities = response.data || [];
        this.updateFilterOptions();
      },
      error: (error) => {
        console.error('Error loading facilities:', error);
      }
    });

    this.productService.apiProductListGet().subscribe({
      next: (response) => {
        this.products = response.data || [];
        this.updateFilterOptions();
      },
      error: (error) => {
        console.error('Error loading products:', error);
      }
    });
  }

  // Опции для селекта статуса
  get statusOptions() {
    return [
      { value: '', label: 'Все статусы' },
      { value: PurchaseStatusEnum.Pending.toString(), label: PurchaseStatusLabels[PurchaseStatusEnum.Pending] },
      { value: PurchaseStatusEnum.Confirmed.toString(), label: PurchaseStatusLabels[PurchaseStatusEnum.Confirmed] },
      { value: PurchaseStatusEnum.Completed.toString(), label: PurchaseStatusLabels[PurchaseStatusEnum.Completed] },
      { value: PurchaseStatusEnum.Cancelled.toString(), label: PurchaseStatusLabels[PurchaseStatusEnum.Cancelled] }
    ];
  }

  // Опции для поставщиков
  get supplierOptions() {
    return [
      { value: '', label: 'Все поставщики' },
      ...this.suppliers.map(supplier => ({
        value: supplier.id?.toString() || '',
        label: supplier.name || ''
      }))
    ];
  }

  // Опции для объектов
  get facilityOptions() {
    return [
      { value: '', label: 'Все объекты' },
      ...this.facilities.map(facility => ({
        value: facility.id?.toString() || '',
        label: facility.name || ''
      }))
    ];
  }

  // Опции для продуктов
  get productOptions() {
    return [
      { value: '', label: 'Все продукты' },
      ...this.products.map(product => ({
        value: product.id?.toString() || '',
        label: product.name || ''
      }))
    ];
  }

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

  loadPurchases(): void {
    this.loading = true;

    // Применяем фильтры и сортировку
    const filterParams: ApiPurchaseGetRequestParams = {
      pageNumber: this.currentPage,
      pageSize: this.pageSize,
      sortBy: this.sortConfig.key,
      sortDescending: !this.sortConfig.asc,
      supplierId: this.filterSettings.supplierId,
      facilityId: this.filterSettings.facilityId,
      productId: this.filterSettings.productId,
      purchaseStatus: this.filterSettings.purchaseStatus,
      startDate: this.filterSettings.startDate,
      endDate: this.filterSettings.endDate,
      searchTerm: this.filterSettings.searchTerm
    };

    this.purchaseService.apiPurchaseGet(filterParams).subscribe({
      next: (response) => {
        let data = response.data || {};
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

  onSortChange(sort: SortConfig): void {
    this.sortConfig = sort;
    this.currentPage = 1; // Сбрасываем на первую страницу при сортировке
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

  onSearch(term: string): void {
    this.filterSettings.searchTerm = term;
    this.currentPage = 1;
    this.loadPurchases();
  }

  applyFilters(): void {
    this.currentPage = 1;
    this.loadPurchases();
    this.closeFilterModal();
  }

  clearFilters(): void {
    this.resetFiltersTrigger = true;

    this.filterValues = {};
    this.filterSettings = {
      supplierId: undefined,
      facilityId: undefined,
      productId: undefined,
      purchaseStatus: undefined,
      startDate: undefined,
      endDate: undefined,
      searchTerm: this.filterSettings.searchTerm
    };

    setTimeout(() => {
      this.resetFiltersTrigger = false;
    }, 0);

    this.applyFilters();
  }

  onEditPurchase(purchase: PurchaseDto): void {
    this.router.navigate(['/purchases/edit', purchase.id]);
  }

  onDeletePurchase(purchase: PurchaseDto): void {
    this.openDeleteModal(purchase);
  }

  onOpenFilterModal(): void {
    this.isFilterModalOpen = true;
  }

  onCloseFilterModal(): void {
    this.isFilterModalOpen = false;
  }

  onFilterChange(filters: any): void {
    // Преобразуем фильтры в формат для API
    this.filterSettings.supplierId = filters.supplierId ? Number(filters.supplierId) : undefined;
    this.filterSettings.facilityId = filters.facilityId ? Number(filters.facilityId) : undefined;
    this.filterSettings.productId = filters.productId ? Number(filters.productId) : undefined;
    this.filterSettings.purchaseStatus = filters.purchaseStatus ? Number(filters.purchaseStatus) : undefined;

    // Обрабатываем dateRange
    if (filters.dateRange) {
      this.filterSettings.startDate = filters.dateRange.startDate;
      this.filterSettings.endDate = filters.dateRange.endDate;
    } else {
      this.filterSettings.startDate = undefined;
      this.filterSettings.endDate = undefined;
    }
  }

  get hasActiveFilters(): boolean {
    return !!(
      this.filterSettings.supplierId ||
      this.filterSettings.facilityId ||
      this.filterSettings.productId ||
      this.filterSettings.purchaseStatus ||
      this.filterSettings.startDate ||
      this.filterSettings.endDate ||
      this.filterSettings.searchTerm
    );
  }

  // Методы для управления модальным окном фильтров
  openFilterModal(): void {
    this.isFilterModalOpen = true;
  }

  closeFilterModal(): void {
    this.isFilterModalOpen = false;
  }

  // Методы для управления модальным окном удаления
  openDeleteModal(purchase: PurchaseDto): void {
    this.purchaseToDelete = purchase;
    this.isDeleteModalOpen = true;
  }

  closeDeleteModal(): void {
    this.isDeleteModalOpen = false;
    this.purchaseToDelete = null;
  }

  onStatusChange(status: string): void {
    this.filterSettings.purchaseStatus = status ? Number(status) : undefined;
  }

  onProductChange(productId: string): void {
    this.filterSettings.productId = productId ? Number(productId) : undefined;
  }

  onFacilityChange(facilityId: string): void {
    this.filterSettings.facilityId = facilityId ? Number(facilityId) : undefined;
  }

  onSupplierChange(supplierId: string): void {
    this.filterSettings.supplierId = supplierId ? Number(supplierId) : undefined;
  }

  confirmDelete(): void {
    if (this.purchaseToDelete) {
      const request: ApiPurchaseIdDeleteRequestParams = {
        id: this.purchaseToDelete.id
      };
      this.purchaseService.apiPurchaseIdDelete(request).subscribe({
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

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString('ru-RU');
  }

  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('ru-RU', {
      style: 'currency',
      currency: 'RUB',
      minimumFractionDigits: 0
    }).format(amount);
  }

  // Обработчик для DateRangePicker
  onDateRangeChange(event: { startDate: string, endDate: string, selectedDates: Date[], displayValue: string }): void {
    this.filterSettings.startDate = event.startDate;
    this.filterSettings.endDate = event.endDate;
  }

  // Геттеры для пагинации
  get startItem(): number {
    return this.totalItems === 0 ? 0 : (this.currentPage - 1) * this.pageSize + 1;
  }

  get endItem(): number {
    return Math.min(this.currentPage * this.pageSize, this.totalItems);
  }
}