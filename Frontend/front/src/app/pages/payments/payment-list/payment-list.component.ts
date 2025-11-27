import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Subject, takeUntil, debounceTime, distinctUntilChanged, finalize } from 'rxjs';
import { PaymentMethodLabels, PaymentStatusLabels } from '../../../core/models/payment.models';
import { BadgeComponent } from '../../../shared/components/ui/badge/badge.component';
import { PageBreadcrumbComponent } from '../../../shared/components/common/page-breadcrumb/page-breadcrumb.component';
import { ModalComponent } from '../../../shared/components/ui/modal/modal.component';
import { ButtonComponent } from '../../../shared/components/ui/button/button.component';
import {
  ApiPaymentGetRequestParams,
  ApiPaymentIdDeleteRequestParams,
  PaymentDto,
  PaymentMethodEnum,
  PaymentService,
  PaymentStatusEnum,
  PurchaseDto,
  PurchaseService
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
  selector: 'app-payment-list',
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
  templateUrl: './payment-list.component.html'
})
export class PaymentListComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  private searchSubject = new Subject<string>();

  // Table configuration
  tableColumns: TableColumn[] = [
    {
      key: 'purchaseName',
      label: 'Закупка',
      sortable: false
    },
    {
      key: 'amount',
      label: 'Сумма',
      sortable: true,
      type: 'currency'
    },
    {
      key: 'paymentMethod',
      label: 'Метод оплаты',
      sortable: true,
      type: 'badge',
      badgeColorMap: {
        [PaymentMethodEnum.NUMBER_1]: 'info',
        [PaymentMethodEnum.NUMBER_2]: 'success',
        [PaymentMethodEnum.NUMBER_3]: 'warning'
      },
      format: (value: PaymentMethodEnum) => PaymentMethodLabels[value]
    },
    {
      key: 'paymentStatus',
      label: 'Статус',
      sortable: true,
      type: 'badge',
      badgeColorMap: {
        [PaymentStatusEnum.NUMBER_1]: 'warning',
        [PaymentStatusEnum.NUMBER_2]: 'success',
        [PaymentStatusEnum.NUMBER_3]: 'error'
      },
      format: (value: PaymentStatusEnum) => PaymentStatusLabels[value]
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
      key: 'purchaseId',
      label: 'Закупка',
      type: 'select',
      placeholder: 'Все закупки'
    },
    {
      key: 'paymentMethod',
      label: 'Метод оплаты',
      type: 'select',
      placeholder: 'Все методы'
    },
    {
      key: 'paymentStatus',
      label: 'Статус',
      type: 'select',
      placeholder: 'Все статусы'
    }
  ];

  // Data
  payments: PaymentDto[] = [];
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
  paymentToDelete: PaymentDto | null = null;
  resetFiltersTrigger = false;

  // Filter data
  purchases: PurchaseDto[] = [];

  // Constants
  paymentStatusLabels = PaymentStatusLabels;
  paymentStatusEnum = PaymentStatusEnum;
  paymentMethodLabels = PaymentMethodLabels;
  paymentMethodEnum = PaymentMethodEnum;

  constructor(
    private paymentService: PaymentService,
    private purchaseService: PurchaseService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.loadFilterData();
    this.loadPayments();

    // Debounce поиска (300ms)
    this.searchSubject.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      takeUntil(this.destroy$)
    ).subscribe(term => {
      this.searchTerm = term;
      this.currentPage = 1;
      this.loadPayments();
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /**
   * Загрузка данных для фильтров
   */
  loadFilterData(): void {
    // Загрузка закупок
    this.purchaseService.apiPurchaseListGet()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          this.purchases = response.data || [];
          this.updateFilterOptions();
        },
        error: (error) => {
          console.error('Error loading purchases:', error);
        }
      });
  }

  /**
   * Обновление опций фильтров
   */
  updateFilterOptions(): void {
    this.filterFields = this.filterFields.map(field => {
      switch (field.key) {
        case 'purchaseId':
          return { ...field, options: this.purchaseOptions };
        case 'paymentMethod':
          return { ...field, options: this.paymentMethodOptions };
        case 'paymentStatus':
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
      { value: PaymentStatusEnum.NUMBER_1.toString(), label: PaymentStatusLabels[PaymentStatusEnum.NUMBER_1] },
      { value: PaymentStatusEnum.NUMBER_2.toString(), label: PaymentStatusLabels[PaymentStatusEnum.NUMBER_2] },
      { value: PaymentStatusEnum.NUMBER_3.toString(), label: PaymentStatusLabels[PaymentStatusEnum.NUMBER_3] }
    ];
  }

  get paymentMethodOptions(): FilterOption[] {
    return [
      { value: '', label: 'Все методы' },
      { value: PaymentMethodEnum.NUMBER_1.toString(), label: PaymentMethodLabels[PaymentMethodEnum.NUMBER_1] },
      { value: PaymentMethodEnum.NUMBER_2.toString(), label: PaymentMethodLabels[PaymentMethodEnum.NUMBER_2] },
      { value: PaymentMethodEnum.NUMBER_3.toString(), label: PaymentMethodLabels[PaymentMethodEnum.NUMBER_3] }
    ];
  }

  get purchaseOptions(): FilterOption[] {
    return [
      { value: '', label: 'Все закупки' },
      ...this.purchases.map(purchase => ({
        value: purchase.id?.toString() || '',
        label: `${purchase.name || 'Закупка'} - ${purchase.supplierName || ''}`
      }))
    ];
  }

  /**
   * Загрузка данных оплат
   */
  loadPayments(): void {
    this.loading = true;

    const filterParams: ApiPaymentGetRequestParams = {
      pageNumber: this.currentPage,
      pageSize: this.pageSize,
      sortBy: this.sortConfig.key,
      sortDescending: !this.sortConfig.asc,
      ...this.convertFiltersToApiParams(this.filterValues),
      searchTerm: this.searchTerm || undefined
    };

    this.paymentService.apiPaymentGet(filterParams)
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => this.loading = false)
      )
      .subscribe({
        next: (response) => {
          const data = response.data || {};
          this.payments = data.items || [];
          this.totalItems = data.totalCount ?? 0;
          this.totalPages = data.totalPages ?? 0;
        },
        error: (error) => {
          console.error('Error loading payments:', error);
          this.payments = [];
          this.totalItems = 0;
          this.totalPages = 0;
        }
      });
  }

  /**
   * Преобразование фильтров в параметры API
   */
  private convertFiltersToApiParams(filters: FilterValues): Partial<ApiPaymentGetRequestParams> {
    const params: Partial<ApiPaymentGetRequestParams> = {};
    const validPaymentMethods = Object.values(PaymentMethodEnum).filter(v => typeof v === 'number') as number[];
    const validPaymentStatuses = Object.values(PaymentStatusEnum).filter(v => typeof v === 'number') as number[];

    if (filters['purchaseId'] && filters['purchaseId'] !== '') {
      params.purchaseId = filters['purchaseId'] as string;
    }

    if (filters['paymentMethod'] && filters['paymentMethod'] !== '') {
      const methodValue = Number(filters['paymentMethod']);
      if (!isNaN(methodValue) && validPaymentMethods.includes(methodValue)) {
        params.paymentMethod = methodValue as PaymentMethodEnum;
      }
    }

    if (filters['paymentStatus'] && filters['paymentStatus'] !== '') {
      const statusValue = Number(filters['paymentStatus']);
      if (!isNaN(statusValue) && validPaymentStatuses.includes(statusValue)) {
        params.paymentStatus = statusValue as PaymentStatusEnum;
      }
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
    this.loadPayments();
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadPayments();
  }

  onPageSizeChange(size: number): void {
    this.pageSize = size;
    this.currentPage = 1;
    this.loadPayments();
  }

  /**
   * Обработчики поиска
   */
  onSearch(term: string): void {
    this.searchSubject.next(term);
  }

  /**
   * Обработчики фильтров
   */
  onFiltersChange(filters: FilterValues): void {
    this.filterValues = filters;
  }

  onFiltersApply(): void {
    this.currentPage = 1;
    this.loadPayments();
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

    this.loadPayments();
  }

  onFilterModalToggle(isOpen: boolean): void {
    this.isFilterModalOpen = isOpen;
  }

  /**
   * Обработчики действий
   */
  onEditPayment(payment: PaymentDto): void {
    this.router.navigate(['/payments/edit', payment.id]);
  }

  onDeletePayment(payment: PaymentDto): void {
    this.openDeleteModal(payment);
  }

  /**
   * Управление модальным окном удаления
   */
  openDeleteModal(payment: PaymentDto): void {
    this.paymentToDelete = payment;
    this.isDeleteModalOpen = true;
  }

  closeDeleteModal(): void {
    this.isDeleteModalOpen = false;
    this.paymentToDelete = null;
  }

  confirmDelete(): void {
    if (this.paymentToDelete?.id) {
      const request: ApiPaymentIdDeleteRequestParams = {
        id: this.paymentToDelete.id
      };

      this.paymentService.apiPaymentIdDelete(request)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: () => {
            this.loadPayments();
            this.closeDeleteModal();
          },
          error: (error) => {
            console.error('Error deleting payment:', error);
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

  getBadgeColor(status: PaymentStatusEnum): 'success' | 'error' | 'warning' | 'info' {
    switch (status) {
      case PaymentStatusEnum.NUMBER_1:
        return 'warning';
      case PaymentStatusEnum.NUMBER_2:
        return 'success';
      case PaymentStatusEnum.NUMBER_3:
        return 'error';
      default:
        return 'warning';
    }
  }

  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('ru-RU', {
      style: 'currency',
      currency: 'RUB',
      minimumFractionDigits: 2
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