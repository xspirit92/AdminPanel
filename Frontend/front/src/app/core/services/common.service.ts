import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { ApiService } from './api.service';
import { 
  PurchaseDto, 
  Result, 
} from '../models/payment.models';
import { FacilityDto, ProductDto, SupplierDto } from '../models/purchase.models';

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

  getPurchases(): Observable<Result<PurchaseDto[]>> {
    return this.apiService.get<Result<PurchaseDto[]>>('/api/purchase/list');
  }
}