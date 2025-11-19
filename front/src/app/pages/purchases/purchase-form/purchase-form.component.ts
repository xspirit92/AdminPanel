import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { PurchaseService } from '../../../core/services/purchase.service';
import { CreateOrUpdatePurchaseCommand } from '../../../core/models/purchase.models';

@Component({
  selector: 'app-purchase-form',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './purchase-form.component.html'
})
export class PurchaseFormComponent implements OnInit {
  purchaseForm: FormGroup;
  isEditMode = false;
  purchaseId: string | null = null;
  loading = false;
  submitted = false;

  // Временные данные для демонстрации (замените на реальные сервисы)
  suppliers = [
    { id: 1, name: 'Поставщик 1' },
    { id: 2, name: 'Поставщик 2' },
    { id: 3, name: 'Поставщик 3' }
  ];

  facilities = [
    { id: 1, name: 'Производство Москва' },
    { id: 2, name: 'Производство СПб' },
    { id: 3, name: 'Производство Казань' }
  ];

  products = [
    { id: 1, name: 'Дубовая доска' },
    { id: 2, name: 'Сосновая доска' },
    { id: 3, name: 'Фанера' },
    { id: 4, name: 'ДСП' }
  ];

  constructor(
    private fb: FormBuilder,
    private purchaseService: PurchaseService,
    private router: Router,
    private route: ActivatedRoute
  ) {
    this.purchaseForm = this.createForm();
  }

  ngOnInit(): void {
    this.purchaseId = this.route.snapshot.paramMap.get('id');
    this.isEditMode = !!this.purchaseId;

    if (this.isEditMode && this.purchaseId) {
      this.loadPurchase(this.purchaseId);
    }
  }

  createForm(): FormGroup {
    return this.fb.group({
      supplierId: ['', Validators.required],
      facilityId: ['', Validators.required],
      productId: ['', Validators.required],
      quantity: ['', [Validators.required, Validators.min(0.1)]],
      amount: ['', [Validators.required, Validators.min(0)]]
    });
  }

  loadPurchase(id: string): void {
    this.loading = true;
    this.purchaseService.getPurchaseById(id).subscribe({
      next: (purchase) => {
        // Для редактирования нужно будет загрузить дополнительные данные
        // или изменить API чтобы возвращались ID вместо названий
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading purchase:', error);
        this.loading = false;
      }
    });
  }

  onSubmit(): void {
    this.submitted = true;

    if (this.purchaseForm.invalid) {
      return;
    }

    this.loading = true;
    const formValue = this.purchaseForm.value;

    const command: CreateOrUpdatePurchaseCommand = {
      id: this.isEditMode ? this.purchaseId! : undefined,
      supplierId: Number(formValue.supplierId),
      facilityId: Number(formValue.facilityId),
      productId: Number(formValue.productId),
      quantity: Number(formValue.quantity),
      amount: Number(formValue.amount)
    };

    const operation = this.isEditMode 
      ? this.purchaseService.updatePurchase(command)
      : this.purchaseService.createPurchase(command);

    operation.subscribe({
      next: (result) => {
        this.loading = false;
        this.router.navigate(['/purchases']);
      },
      error: (error) => {
        console.error('Error saving purchase:', error);
        this.loading = false;
      }
    });
  }

  onCancel(): void {
    this.router.navigate(['/purchases']);
  }

  get f() {
    return this.purchaseForm.controls;
  }
}