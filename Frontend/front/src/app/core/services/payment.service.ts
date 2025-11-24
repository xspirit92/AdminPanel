import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { CreateOrUpdatePaymentCommand, PaymentDto, PaymentDtoPagedListDto, Result } from '../models/payment.models';

export interface PaymentFilter {
  purchaseId?: string;
  paymentMethod?: number;
  paymentStatus?: number;
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
export class PaymentService {
  private endpoint = '/api/Payment';

  constructor(private apiService: ApiService) {}

  getPayments(filter: PaymentFilter): Observable<Result<PaymentDtoPagedListDto>> {
    return this.apiService.get<Result<PaymentDtoPagedListDto>>(this.endpoint, filter);
  }

  getPaymentById(id: string): Observable<Result<PaymentDto>> {
    return this.apiService.get<Result<PaymentDto>>(`${this.endpoint}/${id}`);
  }

  createPayment(command: CreateOrUpdatePaymentCommand): Observable<Result<PaymentDto>> {
    return this.apiService.post<Result<PaymentDto>>(this.endpoint, command);
  }

  updatePayment(command: CreateOrUpdatePaymentCommand): Observable<Result<PaymentDto>> {
    return this.apiService.post<Result<PaymentDto>>(this.endpoint, command);
  }

  deletePayment(id: string): Observable<Result<PaymentDto>> {
    return this.apiService.delete<Result<PaymentDto>>(`${this.endpoint}/${id}`);
  }
}