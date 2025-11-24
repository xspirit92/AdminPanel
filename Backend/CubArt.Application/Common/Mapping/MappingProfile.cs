using AutoMapper;
using CubArt.Application.Common.Models;
using CubArt.Application.Facilities.DTOs;
using CubArt.Application.Payments.DTOs;
using CubArt.Application.Productions.DTOs;
using CubArt.Application.Products.DTOs;
using CubArt.Application.Purchases.DTOs;
using CubArt.Application.Suppliers.DTOs;
using CubArt.Application.Supplies.DTOs;
using CubArt.Domain.Entities;
using CubArt.Domain.Enums;

namespace CubArt.Application.Common.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Purchase, PurchaseDto>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => $"{src.Product.Name} от {src.DateCreated.ToString("dd.MM.yyyy")}"));
            CreateMap<Payment, PaymentDto>()
                .ForMember(dest => dest.PurchaseName, opt => opt.MapFrom(src => $"{src.Purchase.Product.Name} от {src.Purchase.DateCreated.ToString("dd.MM.yyyy")}"));
            CreateMap<Supply, SupplyDto>();
            CreateMap<Supplier, SupplierDto>();
            CreateMap<Facility, FacilityDto>();
            CreateMap<Production, ProductionDto>();
            CreateMap<StockMovement, StockMovementDto>()
                .ForMember(dest => dest.FacilityName, opt => opt.MapFrom(src => src.Facility.Name))
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.OperationType == OperationTypeEnum.Income ? src.Quantity : src.Quantity * -1));

            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.ActiveSpecification,
                       opt => opt.MapFrom(src => src.ProductSpecifications
                           .FirstOrDefault(ps => ps.IsActive)));

            CreateMap<ProductSpecification, ProductSpecificationDto>();
            CreateMap<ProductSpecificationItem, ProductSpecificationItemDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.ProductType, opt => opt.MapFrom(src => src.Product.ProductType))
                .ForMember(dest => dest.UnitOfMeasure, opt => opt.MapFrom(src => src.Product.UnitOfMeasure));

        }
    }

}
