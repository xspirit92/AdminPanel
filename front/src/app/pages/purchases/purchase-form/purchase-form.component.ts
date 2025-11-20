import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { LabelComponent } from '../../../shared/components/form/label/label.component';
import { InputFieldComponent } from '../../../shared/components/form/input/input-field.component';
import { SelectComponent } from '../../../shared/components/form/select/select.component';
import { ButtonComponent } from '../../../shared/components/ui/button/button.component';
import { CreateOrUpdatePurchaseCommand, FacilityDto, ProductDto, ProductTypeEnum, SupplierDto, UnitOfMeasureEnum } from '../../../core/models/purchase.models';
import { CommonService } from '../../../core/services/common.service';
import { PurchaseService } from '../../../core/services/purchase.service';
import { PageBreadcrumbComponent } from "../../../shared/components/common/page-breadcrumb/page-breadcrumb.component";
import { NotificationService } from '../../../shared/services/notification.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-purchase-form',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    LabelComponent,
    InputFieldComponent,
    SelectComponent,
    ButtonComponent,
    PageBreadcrumbComponent
],
  templateUrl: './purchase-form.component.html',
  styles: ``
})
export class PurchaseFormComponent implements OnInit {
  purchaseForm: FormGroup;

  suppliers: SupplierDto[] = [];
  facilities: FacilityDto[] = [];
  products: ProductDto[] = [];

  isLoading = false;
  isSubmitting = false;

  constructor(
    private fb: FormBuilder,
    private commonService: CommonService,
    private purchaseService: PurchaseService,
    private notificationService: NotificationService,
    private router: Router
  ) {
    this.purchaseForm = this.createForm();
  }

  ngOnInit() {
    this.loadData();
  }

  createForm(): FormGroup {
    return this.fb.group({
      supplierId: ['', Validators.required],
      facilityId: ['', Validators.required],
      productId: ['', Validators.required],
      quantity: [1, [Validators.required, Validators.min(0.01)]],
      amount: [0, [Validators.required, Validators.min(0)]]
    });
  }

  loadData() {
    this.isLoading = true;

    // Загружаем данные параллельно
    Promise.all([
      this.commonService.getSuppliers().toPromise(),
      this.commonService.getFacilities().toPromise(),
      this.commonService.getProducts().toPromise()
    ]).then(([suppliers, facilities, products]) => {
      this.suppliers = suppliers?.data || [];
      this.facilities = facilities?.data || [];
      this.products = products?.data || [];
      this.isLoading = false;
    }).catch(error => {
      console.error('Error loading data:', error);
      this.isLoading = false;
    });
  }

  get supplierOptions() {
    return this.suppliers.map(supplier => ({
      value: supplier.id.toString(),
      label: `${supplier.name}`
    }));
  }

  get facilityOptions() {
    return this.facilities.map(facility => ({
      value: facility.id.toString(),
      label: `${facility.name}`
    }));
  }

  get productOptions() {
    return this.products.map(product => ({
      value: product.id.toString(),
      label: `${product.name}`
    }));
  }

  getUnitOfMeasureLabel(unit: UnitOfMeasureEnum): string {
    const unitLabels = {
      [UnitOfMeasureEnum.Piece]: 'шт',
      [UnitOfMeasureEnum.Kilogram]: 'кг',
      [UnitOfMeasureEnum.Meter]: 'м',
      [UnitOfMeasureEnum.Liter]: 'л',
      [UnitOfMeasureEnum.SquareMeter]: 'м²',
      [UnitOfMeasureEnum.CubicMeter]: 'м³'
    };
    return unitLabels[unit] || '';
  }

  getSelectedProduct() {
    const productId = this.purchaseForm.get('productId')?.value;
    return this.products.find(p => p.id.toString() === productId);
  }

  incrementQuantity() {
    const current = this.purchaseForm.get('quantity')?.value || 0;
    this.purchaseForm.patchValue({ quantity: current + 1 });
  }

  decrementQuantity() {
    const current = this.purchaseForm.get('quantity')?.value || 0;
    if (current > 1) {
      this.purchaseForm.patchValue({ quantity: current - 1 });
    }
  }

  onCreatePurchase() {
    if (this.purchaseForm.invalid) {
      this.markFormGroupTouched();
      return;
    }

    this.isSubmitting = true;

    const formValue = this.purchaseForm.value;
    const command: CreateOrUpdatePurchaseCommand = {
      supplierId: parseInt(formValue.supplierId),
      facilityId: parseInt(formValue.facilityId),
      productId: parseInt(formValue.productId),
      quantity: formValue.quantity,
      amount: formValue.amount
    };

    this.purchaseService.createPurchase(command).subscribe({
      next: (response) => {
        this.isSubmitting = false;
        if (response.isSuccess) {
          
          this.notificationService.success('Закупка успешно создана');
          setTimeout(() => {
            this.router.navigate(['/purchases']);
          }, 1000);
          
        } else {
          this.notificationService.error(response.errorMessage);
        }
      },
      error: (error) => {
        console.error('Error creating purchase:', error);
        this.isSubmitting = false;
        // Здесь можно добавить обработку ошибки
      }
    });
  }

  private markFormGroupTouched() {
    Object.keys(this.purchaseForm.controls).forEach(key => {
      const control = this.purchaseForm.get(key);
      control?.markAsTouched();
    });
  }

  // Геттеры для удобства доступа к контролам в шаблоне
  get supplierId() { return this.purchaseForm.get('supplierId'); }
  get facilityId() { return this.purchaseForm.get('facilityId'); }
  get productId() { return this.purchaseForm.get('productId'); }
  get quantity() { return this.purchaseForm.get('quantity'); }
  get amount() { return this.purchaseForm.get('amount'); }
}