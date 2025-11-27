import { PaymentMethodEnum, PaymentStatusEnum } from "../../api";

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

export const PaymentMethodLabels = {
  [PaymentMethodEnum.NUMBER_1]: 'Наличные',
  [PaymentMethodEnum.NUMBER_2]: 'Банковский перевод',
  [PaymentMethodEnum.NUMBER_3]: 'Электронный платеж'
};

export const PaymentStatusLabels = {
  [PaymentStatusEnum.NUMBER_1]: 'В ожидании',
  [PaymentStatusEnum.NUMBER_2]: 'Завершена',
  [PaymentStatusEnum.NUMBER_3]: 'Ошибка'
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