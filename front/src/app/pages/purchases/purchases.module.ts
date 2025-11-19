import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

// Импортируем standalone компоненты
import { PurchaseListComponent } from './purchase-list/purchase-list.component';

@NgModule({
  imports: [
    CommonModule,
    RouterModule,
    FormsModule,
    ReactiveFormsModule,
    PurchaseListComponent
  ]
})
export class PurchasesModule { }