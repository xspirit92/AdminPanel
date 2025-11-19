export interface Result<T> {
  data: T
  errorMessage: string;
  isSuccess: boolean;
}

export interface PurchaseDto {
  id: string;
  supplierName: string;
  productName: string;
  facilityName: string;
  amount: number;
  quantity: number;
  purchaseStatus: PurchaseStatusEnum;
  dateCreated: string;
}

export interface PagedListDto {
  items: PurchaseDto[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface CreateOrUpdatePurchaseCommand {
  id?: string;
  supplierId: number;
  facilityId: number;
  productId: number;
  amount: number;
  quantity: number;
}

export enum PurchaseStatusEnum {
  Pending = 1,
  Confirmed = 2,
  Completed = 3,
  Cancelled = 4
}

export const PurchaseStatusLabels = {
  [PurchaseStatusEnum.Pending]: 'В ожидании',
  [PurchaseStatusEnum.Confirmed]: 'Подтверждена',
  [PurchaseStatusEnum.Completed]: 'Завершена',
  [PurchaseStatusEnum.Cancelled]: 'Отменена'
};