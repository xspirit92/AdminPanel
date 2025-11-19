import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { PurchaseService, PurchaseFilter } from '../../../core/services/purchase.service';
import { PurchaseDto, PurchaseStatusLabels, PurchaseStatusEnum } from '../../../core/models/purchase.models';
import { SelectComponent } from '../../../shared/components/form/select/select.component';
import { ButtonComponent } from '../../../shared/components/ui/button/button.component';
import { BadgeComponent } from '../../../shared/components/ui/badge/badge.component';
import { TableDropdownComponent } from '../../../shared/components/common/table-dropdown/table-dropdown.component';
import { PageBreadcrumbComponent } from '../../../shared/components/common/page-breadcrumb/page-breadcrumb.component';

@Component({
  selector: 'app-purchase-list',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    FormsModule,
    SelectComponent,
    ButtonComponent,
    BadgeComponent,
    TableDropdownComponent,
    PageBreadcrumbComponent
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

  filter: PurchaseFilter = {
    pageNumber: 1,
    pageSize: 10
  };

  purchaseStatusLabels = PurchaseStatusLabels;
  purchaseStatusEnum = PurchaseStatusEnum;

  // Опции для селекта статуса на основе PurchaseStatusEnum и PurchaseStatusLabels
  get statusOptions() {
    return [
      { value: '', label: 'Все статусы' },
      { value: PurchaseStatusEnum.Pending.toString(), label: PurchaseStatusLabels[PurchaseStatusEnum.Pending] },
      { value: PurchaseStatusEnum.Confirmed.toString(), label: PurchaseStatusLabels[PurchaseStatusEnum.Confirmed] },
      { value: PurchaseStatusEnum.Completed.toString(), label: PurchaseStatusLabels[PurchaseStatusEnum.Completed] },
      { value: PurchaseStatusEnum.Cancelled.toString(), label: PurchaseStatusLabels[PurchaseStatusEnum.Cancelled] }
    ];
  }


  // Для поиска
  searchTerm: string = '';

  constructor(private purchaseService: PurchaseService) { }

  ngOnInit(): void {
    this.loadPurchases();
  }

  loadPurchases(): void {
    this.loading = true;
    this.purchaseService.getPurchases(this.filter).subscribe({
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

  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.filter.pageNumber = page;
      this.currentPage = page;
      this.loadPurchases();
    }
  }

  onStatusChange(status: string): void {
    this.filter.purchaseStatus = status ? Number(status) : undefined;
    this.filter.pageNumber = 1;
    this.currentPage = 1;
    this.loadPurchases();
  }

  onSearch(): void {
    this.filter.pageNumber = 1;
    this.currentPage = 1;
    this.loadPurchases();
  }

  onDeletePurchase(id: string): void {
    if (confirm('Вы уверены, что хотите удалить эту закупку?')) {
      this.purchaseService.deletePurchase(id).subscribe({
        next: () => {
          this.loadPurchases();
        },
        error: (error) => {
          console.error('Error deleting purchase:', error);
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
}