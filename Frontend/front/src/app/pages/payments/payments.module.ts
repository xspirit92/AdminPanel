import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Routes } from '@angular/router';
import { PaymentListComponent } from './payment-list/payment-list.component';
import { PaymentFormComponent } from './payment-form/payment-form.component';

const paymentsRoutes: Routes = [
  {
    path: '',
    component: PaymentListComponent
  },
  {
    path: 'create',
    component: PaymentFormComponent
  },
  {
    path: 'edit/:id',
    component: PaymentFormComponent
  }
];

@NgModule({
  declarations: [],
  imports: [
    CommonModule,
    PaymentListComponent,
    RouterModule.forChild(paymentsRoutes)
  ],
  exports: [
    RouterModule
  ]
})
export class PaymentsModule { }