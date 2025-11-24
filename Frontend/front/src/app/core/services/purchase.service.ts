import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { 
  PurchaseDto, 
  PagedListDto, 
  CreateOrUpdatePurchaseCommand, 
  Result
} from '../models/purchase.models';

export interface PurchaseFilter {
  supplierId?: number;
  facilityId?: number;
  productId?: number;
  purchaseStatus?: number;
  startDate?: string;
  endDate?: string;
  pageNumber?: number;
  pageSize?: number;
  searchTerm?: string;
  sortBy?: string;
  sortDescending?: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class PurchaseService {
  private endpoint = '/api/purchase';

  constructor(private apiService: ApiService) {}

  getPurchases(filter: PurchaseFilter): Observable<Result<PagedListDto>> {
    return this.apiService.get<Result<PagedListDto>>(this.endpoint, filter);
  }

  getPurchaseById(id: string): Observable<Result<PurchaseDto>> {
    return this.apiService.get<Result<PurchaseDto>>(`${this.endpoint}/${id}`);
  }

  createPurchase(command: CreateOrUpdatePurchaseCommand): Observable<Result<PurchaseDto>> {
    return this.apiService.post<Result<PurchaseDto>>(this.endpoint, command);
  }

  updatePurchase(command: CreateOrUpdatePurchaseCommand): Observable<Result<PurchaseDto>> {
    return this.apiService.post<Result<PurchaseDto>>(this.endpoint, command);
  }

  deletePurchase(id: string): Observable<Result<PurchaseDto>> {
    return this.apiService.delete<Result<PurchaseDto>>(`${this.endpoint}/${id}`);
  }
  
}