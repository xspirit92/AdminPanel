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
}export interface SupplierDto {
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
  productType: ProductTypeEnum;
  unitOfMeasure: UnitOfMeasureEnum;
  activeSpecification?: ProductSpecificationDto;
}

export interface ProductSpecificationDto {
  id: number;
  productId: number;
  version: string;
  isActive: boolean;
  dateCreated: string;
  items: ProductSpecificationItemDto[];
}

export interface ProductSpecificationItemDto {
  id: number;
  productSpecificationId: number;
  productId: number;
  productName: string;
  productType: ProductTypeEnum;
  quantity: number;
  unitOfMeasure: UnitOfMeasureEnum;
}

export enum ProductTypeEnum {
  RawMaterial = 1,
  SemiFinished = 2,
  ComplexSemiFinished = 3,
  Finished = 4
}

export enum UnitOfMeasureEnum {
  Piece = 1,
  Kilogram = 2,
  Meter = 3,
  Liter = 4,
  SquareMeter = 5,
  CubicMeter = 6
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