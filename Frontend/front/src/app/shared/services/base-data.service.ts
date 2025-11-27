import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

export interface SortConfig {
  key: string;
  asc: boolean;
}

export interface FilterConfig {
  [key: string]: any;
}

export interface PaginationConfig {
  pageNumber: number;
  pageSize: number;
  sortBy?: string;
  sortDescending?: boolean;
}

export interface DataResponse<T> {
  items: T[];
  totalCount: number;
  totalPages: number;
}

@Injectable({
  providedIn: 'root'
})
export abstract class BaseDataService<T> {
  abstract getData(params: any): Observable<DataResponse<T>>;
  abstract deleteItem(id: string | number): Observable<any>;
}