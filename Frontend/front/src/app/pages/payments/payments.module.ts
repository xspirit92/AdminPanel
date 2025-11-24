import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Routes } from '@angular/router';
import { PaymentListComponent } from './payment-list/payment-list.component';

const paymentsRoutes: Routes = [
  {
    path: '',
    component: PaymentListComponent
  },
  {
    path: 'create',
    component: PaymentListComponent
  },
  {
    path: 'edit/:id',
    component: PaymentListComponent
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