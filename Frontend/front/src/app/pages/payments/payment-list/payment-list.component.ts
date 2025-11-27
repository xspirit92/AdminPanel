import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { BadgeComponent } from '../../../shared/components/ui/badge/badge.component';
import { TableDropdownComponent } from '../../../shared/components/common/table-dropdown/table-dropdown.component';
import { PageBreadcrumbComponent } from '../../../shared/components/common/page-breadcrumb/page-breadcrumb.component';
import { LabelComponent } from '../../../shared/components/form/label/label.component';
import { DateRangePickerComponent } from '../../../shared/components/date-range-picker/date-range-picker.component';
import { SelectComponent } from '../../../shared/components/form/select/select.component';
import { ModalComponent } from '../../../shared/components/ui/modal/modal.component';
import { ButtonComponent } from '../../../shared/components/ui/button/button.component';
import { ApiPaymentGetRequestParams, ApiPaymentIdDeleteRequestParams, PaymentDto, PaymentMethodEnum, PaymentService, PaymentStatusEnum, PurchaseDto, PurchaseService } from '../../../api';
import { PaymentMethodLabels, PaymentStatusLabels } from '../../../core/models/payment.models';

interface SortConfig {
  key: string;
  asc: boolean;
}

@Component({
  selector: 'app-payment-list',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    FormsModule,
    BadgeComponent,
    TableDropdownComponent,
    PageBreadcrumbComponent,
    DateRangePickerComponent,
    LabelComponent,
    SelectComponent,
    ModalComponent,
    ButtonComponent
  ],
  templateUrl: './payment-list.component.html'
})
export class PaymentListComponent implements OnInit {
  payments: PaymentDto[] = [];
  purchases: PurchaseDto[] = [];
  loading = false;
  totalItems = 0;
  currentPage = 1;
  pageSize = 10;
  totalPages = 0;

  // Модальное окно фильтров
  isFilterModalOpen = false;

  // Модальное окно подтверждения удаления
  isDeleteModalOpen = false;
  paymentToDelete: PaymentDto | null = null;

  // Сортировка
  sort: SortConfig = {
    key: 'dateCreated',
    asc: false
  };

  filter: ApiPaymentGetRequestParams = {
    pageNumber: 1,
    pageSize: 10,
    sortBy: 'dateCreated',
    sortDescending: true
  };

  // Фильтры из Swagger
  filterSettings = {
    purchaseId: undefined as string | undefined,
    paymentMethod: undefined as PaymentMethodEnum | undefined,
    paymentStatus: undefined as PaymentStatusEnum | undefined,
    startDate: undefined as string | undefined,
    endDate: undefined as string | undefined,
    searchTerm: ''
  };

  paymentStatusLabels = PaymentStatusLabels;
  paymentStatusEnum = PaymentStatusEnum;
  paymentMethodLabels = PaymentMethodLabels;
  paymentMethodEnum = PaymentMethodEnum;

  // Опции для селекта метода оплаты
  get paymentMethodOptions() {
    return [
      { value: '', label: 'Все методы' },
      { value: PaymentMethodEnum.NUMBER_1.toString(), label: PaymentMethodLabels[PaymentMethodEnum.NUMBER_1] },
      { value: PaymentMethodEnum.NUMBER_2.toString(), label: PaymentMethodLabels[PaymentMethodEnum.NUMBER_2] },
      { value: PaymentMethodEnum.NUMBER_3.toString(), label: PaymentMethodLabels[PaymentMethodEnum.NUMBER_3] }
    ];
  }

  // Опции для селекта статуса
  get statusOptions() {
    return [
      { value: '', label: 'Все статусы' },
      { value: PaymentStatusEnum.NUMBER_1.toString(), label: PaymentStatusLabels[PaymentStatusEnum.NUMBER_1] },
      { value: PaymentStatusEnum.NUMBER_2.toString(), label: PaymentStatusLabels[PaymentStatusEnum.NUMBER_2] },
      { value: PaymentStatusEnum.NUMBER_3.toString(), label: PaymentStatusLabels[PaymentStatusEnum.NUMBER_3] }
    ];
  }

  // Опции для селекта закупок
  get purchaseOptions() {
    return [
      { value: '', label: 'Все закупки' },
      ...this.purchases.map(purchase => ({
        value: purchase.id,
        label: `${purchase.name || 'Закупка'} - ${purchase.supplierName}`
      }))
    ];
  }

  // Опции для количества элементов на странице
  pageSizeOptions = [
    { value: '10', label: '10' },
    { value: '25', label: '25' },
    { value: '50', label: '50' },
    { value: '100', label: '100' }
  ];

  constructor(
    private paymentService: PaymentService,
    private purchaseService: PurchaseService,
  ) { }

  ngOnInit(): void {
    this.loadPurchases();
    this.loadPayments();
  }

  loadPurchases(): void {
    this.purchaseService.apiPurchaseListGet().subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.purchases = response.data;
        }
      },
      error: (error) => {
        console.error('Error loading purchases:', error);
      }
    });
  }

  loadPayments(): void {
    this.loading = true;

    // Применяем фильтры и сортировку
    const filterParams: ApiPaymentGetRequestParams = {
      ...this.filter,
      purchaseId: this.filterSettings.purchaseId,
      paymentMethod: this.filterSettings.paymentMethod,
      paymentStatus: this.filterSettings.paymentStatus,
      startDate: this.filterSettings.startDate,
      endDate: this.filterSettings.endDate,
      searchTerm: this.filterSettings.searchTerm,
      sortBy: this.sort.key,
      sortDescending: !this.sort.asc
    };

    this.paymentService.apiPaymentGet(filterParams).subscribe({
      next: (response) => {
        let data = response.data;
        this.payments = data?.items || [];
        this.totalItems = data?.totalCount ?? 0;
        this.totalPages = data?.totalPages ?? 0;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading payments:', error);
        this.loading = false;
      }
    });
  }

  // Сортировка по колонке
  sortBy(key: string): void {
    if (this.sort.key === key) {
      this.sort.asc = !this.sort.asc;
    } else {
      this.sort.key = key;
      this.sort.asc = false;
    }

    this.filter.sortBy = this.sort.key;
    this.filter.sortDescending = !this.sort.asc;
    this.loadPayments();
  }

  // Получить класс для иконки сортировки
  getSortIconClass(columnKey: string): string {
    if (this.sort.key !== columnKey) {
      return 'text-gray-300 dark:text-gray-400/50';
    }
    return this.sort.asc ? 'text-gray-500 dark:text-gray-400' : 'text-gray-500 dark:text-gray-400';
  }

  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.filter.pageNumber = page;
      this.currentPage = page;
      this.loadPayments();
    }
  }

  onStatusChange(status: string): void {
    this.filterSettings.paymentStatus = status ? (Number(status) as PaymentStatusEnum) : undefined;
  }

  onPaymentMethodChange(method: string): void {
    this.filterSettings.paymentMethod = method ? (Number(method) as PaymentMethodEnum) : undefined;
  }

  onPurchaseChange(purchaseId: string): void {
    this.filterSettings.purchaseId = purchaseId || undefined;
  }

  onPageSizeChange(size?: number): void {
    this.filter.pageSize = size ?? 10;
    this.filter.pageNumber = 1;
    this.currentPage = 1;
    this.loadPayments();
  }

  onSearch(): void {
    this.applyFilters();
  }

  applyFilters(): void {
    this.filter.pageNumber = 1;
    this.currentPage = 1;
    this.loadPayments();
    this.closeFilterModal();
  }

  clearFilters(): void {
    this.filterSettings = {
      purchaseId: undefined,
      paymentMethod: undefined,
      paymentStatus: undefined,
      startDate: undefined,
      endDate: undefined,
      searchTerm: ''
    };
    this.applyFilters();
  }

  get hasActiveFilters(): boolean {
    return !!(
      this.filterSettings.purchaseId ||
      this.filterSettings.paymentMethod ||
      this.filterSettings.paymentStatus ||
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
  openDeleteModal(payment: PaymentDto): void {
    this.paymentToDelete = payment;
    this.isDeleteModalOpen = true;
  }

  closeDeleteModal(): void {
    this.isDeleteModalOpen = false;
    this.paymentToDelete = null;
  }

  confirmDelete(): void {
    if (this.paymentToDelete) {
      const request: ApiPaymentIdDeleteRequestParams = {
        id: this.paymentToDelete.id
      };
      this.paymentService.apiPaymentIdDelete(request).subscribe({
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

  getStatusBadgeColor(status: PaymentStatusEnum): 'success' | 'error' | 'warning' | 'info' {
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

  getMethodBadgeColor(method: PaymentMethodEnum): 'success' | 'error' | 'warning' | 'info' {
    switch (method) {
      case PaymentMethodEnum.NUMBER_1:
        return 'info';
      case PaymentMethodEnum.NUMBER_2:
        return 'success';
      case PaymentMethodEnum.NUMBER_3:
        return 'warning';
      default:
        return 'info';
    }
  }

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString('ru-RU');
  }

  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('ru-RU', {
      style: 'currency',
      currency: 'RUB',
      minimumFractionDigits: 2
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