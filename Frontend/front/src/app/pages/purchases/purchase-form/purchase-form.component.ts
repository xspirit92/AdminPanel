import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { LabelComponent } from '../../../shared/components/form/label/label.component';
import { InputFieldComponent } from '../../../shared/components/form/input/input-field.component';
import { SelectComponent } from '../../../shared/components/form/select/select.component';
import { ButtonComponent } from '../../../shared/components/ui/button/button.component';
import { PageBreadcrumbComponent } from "../../../shared/components/common/page-breadcrumb/page-breadcrumb.component";
import { NotificationService } from '../../../shared/services/notification.service';
import { ApiPurchaseIdGetRequestParams, ApiPurchasePostRequestParams, CreateOrUpdatePurchaseCommand, FacilityDto, FacilityService, ProductDto, ProductService, PurchaseDto, PurchaseService, SupplierDto, SupplierService, UnitOfMeasureEnum } from '../../../api';

@Component({
  selector: 'app-purchase-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    LabelComponent,
    InputFieldComponent,
    SelectComponent,
    ButtonComponent,
    PageBreadcrumbComponent
  ],
  templateUrl: './purchase-form.component.html'
})
export class PurchaseFormComponent implements OnInit {
  purchaseForm: FormGroup;

  suppliers: SupplierDto[] = [];
  facilities: FacilityDto[] = [];
  products: ProductDto[] = [];

  isLoading = false;
  isSubmitting = false;
  isEditMode = false;
  purchaseId: string | null = null;
  currentPurchase: PurchaseDto | null = null;

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private purchaseService: PurchaseService,
    private supplierService: SupplierService,
    private facilityService: FacilityService,
    private productService: ProductService,
    private notificationService: NotificationService
  ) {
    this.purchaseForm = this.createForm();
  }

  ngOnInit() {
    this.purchaseId = this.route.snapshot.paramMap.get('id');
    this.isEditMode = !!this.purchaseId;
    
    this.loadData();
    
    if (this.isEditMode && this.purchaseId) {
      this.loadPurchaseData();
    }
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

    Promise.all([
      this.supplierService.apiSupplierListGet().toPromise(),
      this.facilityService.apiFacilityListGet().toPromise(),
      this.productService.apiProductListGet().toPromise()
    ]).then(([suppliers, facilities, products]) => {
      this.suppliers = suppliers?.data || [];
      this.facilities = facilities?.data || [];
      this.products = products?.data || [];
      this.isLoading = false;

      // Если мы в режиме редактирования и данные уже загружены, обновляем форму
      if (this.isEditMode && this.currentPurchase) {
        this.populateForm(this.currentPurchase);
      }
    }).catch(error => {
      console.error('Error loading data:', error);
      this.isLoading = false;
      this.notificationService.error('Ошибка загрузки данных');
    });
  }

  loadPurchaseData() {
    if (!this.purchaseId) return;

    this.isLoading = true;
    const request: ApiPurchaseIdGetRequestParams = {
      id: this.purchaseId
    };
    this.purchaseService.apiPurchaseIdGet(request).subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.currentPurchase = response.data;
          // Если справочные данные уже загружены, заполняем форму
          if (this.suppliers.length > 0 && this.facilities.length > 0 && this.products.length > 0) {
            this.populateForm(this.currentPurchase);
          }
        } else {
          this.notificationService.error('Закупка не найдена');
          this.router.navigate(['/purchases']);
        }
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading purchase:', error);
        this.notificationService.error('Ошибка загрузки данных закупки');
        this.isLoading = false;
        this.router.navigate(['/purchases']);
      }
    });
  }

  populateForm(purchase: PurchaseDto) {
    this.purchaseForm.patchValue({
      supplierId: purchase.supplierId.toString(),
      facilityId: purchase.facilityId.toString(),
      productId: purchase.productId.toString(),
      quantity: purchase.quantity,
      amount: purchase.amount
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

  get pageTitle(): string {
    return this.isEditMode ? 'Редактирование закупки' : 'Создание закупки';
  }

  get submitButtonText(): string {
    return this.isSubmitting 
      ? (this.isEditMode ? 'Сохранение...' : 'Создание...')
      : (this.isEditMode ? 'Сохранить изменения' : 'Создать закупку');
  }

  getUnitOfMeasureLabel(unit: UnitOfMeasureEnum): string {
    const unitLabels = {
      [UnitOfMeasureEnum.NUMBER_1]: 'шт',
      [UnitOfMeasureEnum.NUMBER_2]: 'кг',
      [UnitOfMeasureEnum.NUMBER_3]: 'п.м',
      [UnitOfMeasureEnum.NUMBER_4]: 'м',
      [UnitOfMeasureEnum.NUMBER_5]: 'м²',
      [UnitOfMeasureEnum.NUMBER_6]: 'м³'
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

  onSubmit() {
    if (this.purchaseForm.invalid) {
      this.markFormGroupTouched();
      return;
    }

    this.isSubmitting = true;

    const formValue = this.purchaseForm.value;
    const command: ApiPurchasePostRequestParams = {
      createOrUpdatePurchaseCommand: {
        supplierId: parseInt(formValue.supplierId),
        facilityId: parseInt(formValue.facilityId),
        productId: parseInt(formValue.productId),
        quantity: formValue.quantity,
        amount: formValue.amount
      }
    };

    // Добавляем ID для редактирования
    if (this.isEditMode && this.purchaseId) {
      command.createOrUpdatePurchaseCommand!.id = this.purchaseId;
    }

    const request = this.purchaseService.apiPurchasePost(command);

    request.subscribe({
      next: (response) => {
        this.isSubmitting = false;
        if (response.isSuccess) {
          const successMessage = this.isEditMode 
            ? 'Закупка успешно обновлена' 
            : 'Закупка успешно создана';
          
          this.notificationService.success(successMessage);
          this.router.navigate(['/purchases']);
        } else {
          this.notificationService.error(response.errorMessage || 'Произошла ошибка');
        }
      },
      error: (error) => {
        console.error('Error saving purchase:', error);
        this.isSubmitting = false;
        this.notificationService.error('Ошибка сохранения закупки');
      }
    });
  }

  onCancel() {
    this.router.navigate(['/purchases']);
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