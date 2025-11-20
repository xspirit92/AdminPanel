import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { ApiService } from './api.service';
import { Result } from '../models/purchase.models';

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

  getSuppliers(): Observable<Result<SupplierDto[]>> {
    return this.apiService.get<Result<SupplierDto[]>>('/api/supplier/list');
  }

  getFacilities(): Observable<Result<FacilityDto[]>> {
    return this.apiService.get<Result<FacilityDto[]>>('/api/facility/list');
  }

  getProducts(): Observable<Result<ProductDto[]>> {
    return this.apiService.get<Result<ProductDto[]>>('/api/product/list');
  }
}