import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { PurchaseService, PurchaseFilter } from '../../../core/services/purchase.service';
import { CommonService, SupplierDto, FacilityDto, ProductDto } from '../../../core/services/common.service';
import { PurchaseDto, PurchaseStatusLabels, PurchaseStatusEnum } from '../../../core/models/purchase.models';
import { BadgeComponent } from '../../../shared/components/ui/badge/badge.component';
import { TableDropdownComponent } from '../../../shared/components/common/table-dropdown/table-dropdown.component';
import { PageBreadcrumbComponent } from '../../../shared/components/common/page-breadcrumb/page-breadcrumb.component';
import { LabelComponent } from '../../../shared/components/form/label/label.component';
import { DateRangePickerComponent } from '../../../shared/components/date-range-picker/date-range-picker.component';
import { SelectComponent } from '../../../shared/components/form/select/select.component';
import { ModalComponent } from '../../../shared/components/ui/modal/modal.component';
import { ButtonComponent } from '../../../shared/components/ui/button/button.component';

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
    BadgeComponent,
    TableDropdownComponent,
    PageBreadcrumbComponent,
    DateRangePickerComponent,
    LabelComponent,
    SelectComponent,
    ModalComponent,
    ButtonComponent
  ],
  templateUrl: './purchase-list.component.html'
})
export class PurchaseListComponent implements OnInit {
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
  purchaseToDelete: PurchaseDto | null = null;

  // Сортировка
  sort: SortConfig = {
    key: 'dateCreated',
    asc: false
  };

  // Данные для фильтров
  suppliers: SupplierDto[] = [];
  facilities: FacilityDto[] = [];
  products: ProductDto[] = [];

  filter: PurchaseFilter = {
    pageNumber: 1,
    pageSize: 10,
    sortBy: 'dateCreated',
    sortDescending: true
  };

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

  purchaseStatusLabels = PurchaseStatusLabels;
  purchaseStatusEnum = PurchaseStatusEnum;

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
        value: supplier.id.toString(),
        label: supplier.name
      }))
    ];
  }

  // Опции для объектов
  get facilityOptions() {
    return [
      { value: '', label: 'Все объекты' },
      ...this.facilities.map(facility => ({
        value: facility.id.toString(),
        label: facility.name
      }))
    ];
  }

  // Опции для продуктов
  get productOptions() {
    return [
      { value: '', label: 'Все продукты' },
      ...this.products.map(product => ({
        value: product.id.toString(),
        label: product.name
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
    private purchaseService: PurchaseService,
    private commonService: CommonService
  ) { }

  ngOnInit(): void {
    this.loadFilterData();
    this.loadPurchases();
  }

  // Загружаем данные для фильтров
  loadFilterData(): void {
    this.commonService.getSuppliers().subscribe({
      next: (response) => {
        this.suppliers = response.data || [];
      },
      error: (error) => {
        console.error('Error loading suppliers:', error);
      }
    });

    this.commonService.getFacilities().subscribe({
      next: (response) => {
        this.facilities = response.data || [];
      },
      error: (error) => {
        console.error('Error loading facilities:', error);
      }
    });

    this.commonService.getProducts().subscribe({
      next: (response) => {
        this.products = response.data || [];
      },
      error: (error) => {
        console.error('Error loading products:', error);
      }
    });
  }

  loadPurchases(): void {
    this.loading = true;

    // Применяем фильтры и сортировку
    const filterParams: PurchaseFilter = {
      ...this.filter,
      supplierId: this.filterSettings.supplierId,
      facilityId: this.filterSettings.facilityId,
      productId: this.filterSettings.productId,
      purchaseStatus: this.filterSettings.purchaseStatus,
      startDate: this.filterSettings.startDate,
      endDate: this.filterSettings.endDate,
      searchTerm: this.filterSettings.searchTerm,
      sortBy: this.sort.key,
      sortDescending: !this.sort.asc
    };

    this.purchaseService.getPurchases(filterParams).subscribe({
      next: (response) => {
        let data = response.data;
        this.purchases = data.items || [];
        this.totalItems = data.totalCount;
        this.totalPages = data.totalPages;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading purchases:', error);
        this.loading = false;
      }
    });
  }

  // Сортировка по колонке
  sortBy(key: string): void {
    if (this.sort.key === key) {
      // Если уже сортируем по этой колонке, меняем направление
      this.sort.asc = !this.sort.asc;
    } else {
      // Если новая колонка, устанавливаем сортировку по убыванию
      this.sort.key = key;
      this.sort.asc = false;
    }
    
    // Обновляем параметры сортировки в фильтре
    this.filter.sortBy = this.sort.key;
    this.filter.sortDescending = !this.sort.asc;
    
    // Перезагружаем данные
    this.loadPurchases();
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
      this.loadPurchases();
    }
  }

  onStatusChange(status: string): void {
    this.filterSettings.purchaseStatus = status ? Number(status) : undefined;
  }

  onSupplierChange(supplierId: string): void {
    this.filterSettings.supplierId = supplierId ? Number(supplierId) : undefined;
  }

  onFacilityChange(facilityId: string): void {
    this.filterSettings.facilityId = facilityId ? Number(facilityId) : undefined;
  }

  onProductChange(productId: string): void {
    this.filterSettings.productId = productId ? Number(productId) : undefined;
  }

  onPageSizeChange(size?: number): void {
    this.filter.pageSize = size ?? 10;
    this.filter.pageNumber = 1;
    this.currentPage = 1;
    this.loadPurchases();
  }

  onSearch(): void {
    this.applyFilters();
  }

  applyFilters(): void {
    this.filter.pageNumber = 1;
    this.currentPage = 1;
    this.loadPurchases();
    this.closeFilterModal(); // Закрываем модальное окно после применения
  }

  clearFilters(): void {
    this.filterSettings = {
      supplierId: undefined,
      facilityId: undefined,
      productId: undefined,
      purchaseStatus: undefined,
      startDate: undefined,
      endDate: undefined,
      searchTerm: ''
    };
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

  confirmDelete(): void {
    if (this.purchaseToDelete) {
      this.purchaseService.deletePurchase(this.purchaseToDelete.id).subscribe({
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