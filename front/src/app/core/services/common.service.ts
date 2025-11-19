import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { ApiService } from './api.service';

export interface SupplierDto {
  id: number;
  name: string;
  address: string;
}

export interface FacilityDto {
  id: number;
  name: string;
  address: string;
}

export interface ProductDto {
  id: number;
  name: string;
  productType: number;
  unitOfMeasure: number;
}

@Injectable({
  providedIn: 'root'
})
export class CommonService {
  constructor(private apiService: ApiService) {}

  getSuppliers(): Observable<SupplierDto[]> {
    return this.apiService.get<SupplierDto[]>('/api/Supplier');
  }

  getFacilities(): Observable<FacilityDto[]> {
    return this.apiService.get<FacilityDto[]>('/api/Facility');
  }

  getProducts(): Observable<ProductDto[]> {
    return this.apiService.get<ProductDto[]>('/api/Product');
  }
}