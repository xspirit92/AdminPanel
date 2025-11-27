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
import { 
  ApiPaymentIdGetRequestParams, 
  ApiPaymentPostRequestParams, 
  CreateOrUpdatePaymentCommand, 
  PaymentDto, 
  PaymentService, 
  PurchaseDto, 
  PurchaseService,
  PaymentMethodEnum
} from '../../../api';
import { PaymentMethodLabels } from '../../../core/models/payment.models';

@Component({
  selector: 'app-payment-form',
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
  templateUrl: './payment-form.component.html'
})
export class PaymentFormComponent implements OnInit {
  paymentForm: FormGroup;

  purchases: PurchaseDto[] = [];

  isLoading = false;
  isSubmitting = false;
  isEditMode = false;
  paymentId: string | null = null;
  currentPayment: PaymentDto | null = null;

  paymentMethodLabels = PaymentMethodLabels;
  paymentMethodEnum = PaymentMethodEnum;

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private paymentService: PaymentService,
    private purchaseService: PurchaseService,
    private notificationService: NotificationService
  ) {
    this.paymentForm = this.createForm();
  }

  ngOnInit() {
    this.paymentId = this.route.snapshot.paramMap.get('id');
    this.isEditMode = !!this.paymentId;
    
    this.loadData();
    
    if (this.isEditMode && this.paymentId) {
      this.loadPaymentData();
    }
  }

  createForm(): FormGroup {
    return this.fb.group({
      purchaseId: ['', Validators.required],
      amount: [0, [Validators.required, Validators.min(0.01)]],
      paymentMethod: ['', Validators.required]
    });
  }

  loadData() {
    this.isLoading = true;

    this.purchaseService.apiPurchaseListGet().subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.purchases = response.data;
        }
        this.isLoading = false;

        // Если мы в режиме редактирования и данные уже загружены, обновляем форму
        if (this.isEditMode && this.currentPayment) {
          this.populateForm(this.currentPayment);
        }
      },
      error: (error) => {
        console.error('Error loading purchases:', error);
        this.isLoading = false;
        this.notificationService.error('Ошибка загрузки данных');
      }
    });
  }

  loadPaymentData() {
    if (!this.paymentId) return;

    this.isLoading = true;
    const request: ApiPaymentIdGetRequestParams = {
      id: this.paymentId
    };
    this.paymentService.apiPaymentIdGet(request).subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.currentPayment = response.data;
          // Если справочные данные уже загружены, заполняем форму
          if (this.purchases.length > 0) {
            this.populateForm(this.currentPayment);
          }
        } else {
          this.notificationService.error('Оплата не найдена');
          this.router.navigate(['/payments']);
        }
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading payment:', error);
        this.notificationService.error('Ошибка загрузки данных оплаты');
        this.isLoading = false;
        this.router.navigate(['/payments']);
      }
    });
  }

  populateForm(payment: PaymentDto) {
    this.paymentForm.patchValue({
      purchaseId: payment.purchaseId,
      amount: payment.amount,
      paymentMethod: payment.paymentMethod.toString()
    });
  }

  get purchaseOptions() {
    return this.purchases.map(purchase => ({
      value: purchase.id,
      label: `${purchase.name}`
    }));
  }

  get paymentMethodOptions() {
    return [
      { value: PaymentMethodEnum.NUMBER_1.toString(), label: PaymentMethodLabels[PaymentMethodEnum.NUMBER_1] },
      { value: PaymentMethodEnum.NUMBER_2.toString(), label: PaymentMethodLabels[PaymentMethodEnum.NUMBER_2] },
      { value: PaymentMethodEnum.NUMBER_3.toString(), label: PaymentMethodLabels[PaymentMethodEnum.NUMBER_3] }
    ];
  }

  get pageTitle(): string {
    return this.isEditMode ? 'Редактирование оплаты' : 'Создание оплаты';
  }

  get submitButtonText(): string {
    return this.isSubmitting 
      ? (this.isEditMode ? 'Сохранение...' : 'Создание...')
      : (this.isEditMode ? 'Сохранить изменения' : 'Создать оплату');
  }

  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('ru-RU', {
      style: 'currency',
      currency: 'RUB',
      minimumFractionDigits: 2
    }).format(amount);
  }

  onSubmit() {
    if (this.paymentForm.invalid) {
      this.markFormGroupTouched();
      return;
    }

    this.isSubmitting = true;

    const formValue = this.paymentForm.value;
    const command: ApiPaymentPostRequestParams = {
      createOrUpdatePaymentCommand: {
        purchaseId: formValue.purchaseId,
        amount: formValue.amount,
        paymentMethod: Number(formValue.paymentMethod) as PaymentMethodEnum
      }
    };

    // Добавляем ID для редактирования
    if (this.isEditMode && this.paymentId) {
      command.createOrUpdatePaymentCommand!.id = this.paymentId;
    }

    const request = this.paymentService.apiPaymentPost(command);

    request.subscribe({
      next: (response) => {
        this.isSubmitting = false;
        if (response.isSuccess) {
          const successMessage = this.isEditMode 
            ? 'Оплата успешно обновлена' 
            : 'Оплата успешно создана';
          
          this.notificationService.success(successMessage);
          this.router.navigate(['/payments']);
        } else {
          this.notificationService.error(response.errorMessage || 'Произошла ошибка');
        }
      },
      error: (error) => {
        console.error('Error saving payment:', error);
        this.isSubmitting = false;
        this.notificationService.error('Ошибка сохранения оплаты');
      }
    });
  }

  onCancel() {
    this.router.navigate(['/payments']);
  }

  private markFormGroupTouched() {
    Object.keys(this.paymentForm.controls).forEach(key => {
      const control = this.paymentForm.get(key);
      control?.markAsTouched();
    });
  }

  // Геттеры для удобства доступа к контролам в шаблоне
  get purchaseId() { return this.paymentForm.get('purchaseId'); }
  get amount() { return this.paymentForm.get('amount'); }
  get paymentMethod() { return this.paymentForm.get('paymentMethod'); }
}