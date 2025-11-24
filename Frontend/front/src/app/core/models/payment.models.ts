export interface PaymentDto {
  id: string;
  purchaseId: string;
  purchaseName: string;
  amount: number;
  paymentMethod: PaymentMethodEnum;
  paymentStatus: PaymentStatusEnum;
  dateCreated: string;
  purchaseNumber?: string;
  supplierName?: string;
}

export interface PaymentDtoPagedListDto {
  items: PaymentDto[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface CreateOrUpdatePaymentCommand {
  id?: string;
  purchaseId: string;
  amount: number;
  paymentMethod: PaymentMethodEnum;
}

export enum PaymentMethodEnum {
  Cash = 1,
  BankTransfer = 2,
  Electronic = 3
}

export enum PaymentStatusEnum {
  Pending = 1,
  Completed = 2,
  Failed = 3
}

export const PaymentMethodLabels = {
  [PaymentMethodEnum.Cash]: 'Наличные',
  [PaymentMethodEnum.BankTransfer]: 'Банковский перевод',
  [PaymentMethodEnum.Electronic]: 'Электронный платеж'
};

export const PaymentStatusLabels = {
  [PaymentStatusEnum.Pending]: 'В ожидании',
  [PaymentStatusEnum.Completed]: 'Завершена',
  [PaymentStatusEnum.Failed]: 'Ошибка'
};

export interface Result<T> {
  data: T;
  errorMessage: string;
  isSuccess: boolean;
}

// Добавляем интерфейс для закупок
export interface PurchaseDto {
  id: string;
  name: string;
  supplierId: number;
  supplierName: string;
  productId: number;
  productName: string;
  facilityId: number;
  facilityName: string;
  amount: number;
  quantity: number;
  purchaseStatus: PurchaseStatusEnum;
  dateCreated: string;
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