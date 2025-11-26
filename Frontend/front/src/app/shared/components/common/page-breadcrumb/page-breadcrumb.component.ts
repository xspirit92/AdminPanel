import { Component, Input } from '@angular/core';
import { RouterModule } from '@angular/router';
import { CommonModule, NgClass } from '@angular/common';

export interface BreadcrumbItem {
  label: string;
  routerLink?: string; // опциональная ссылка
}

@Component({
  selector: 'app-page-breadcrumb',
  imports: [
    CommonModule,
    RouterModule,
    NgClass
  ],
  templateUrl: './page-breadcrumb.component.html',
  styles: ``
})
export class PageBreadcrumbComponent {
  @Input() pageTitle = '';
  
  // Новый входной параметр для хлебных крошек
  @Input() breadcrumbs: BreadcrumbItem[] = [];

  // Геттер для объединения дефолтных и кастомных хлебных крошек
  get breadcrumbItems(): BreadcrumbItem[] {
    // Если переданы кастомные хлебные крошки, используем их
    if (this.breadcrumbs && this.breadcrumbs.length > 0) {
      return [
        { label: 'Домой', routerLink: '/' },
        ...this.breadcrumbs
      ];
    }
    
    // Иначе используем только заголовок страницы как последний элемент
    return [
      { label: 'Домой', routerLink: '/' },
      { label: this.pageTitle }
    ];
  }
}