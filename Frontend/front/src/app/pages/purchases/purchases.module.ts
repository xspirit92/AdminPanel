import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Routes } from '@angular/router';
import { PurchaseListComponent } from './purchase-list/purchase-list.component';
import { PurchaseFormComponent } from './purchase-form/purchase-form.component';

const purchasesRoutes: Routes = [
  {
    path: '',
    component: PurchaseListComponent
  },
  {
    path: 'create',
    component: PurchaseFormComponent
  },
  {
    path: 'edit/:id',
    component: PurchaseFormComponent
  }
];

@NgModule({
  declarations: [],
  imports: [
    CommonModule,
    PurchaseListComponent,
    RouterModule.forChild(purchasesRoutes)
  ],
  exports: [
    RouterModule
  ]
})
export class PurchasesModule { }