using AutoMapper;
using CubArt.Application.Common.Models;
using CubArt.Application.Products.Commands;
using CubArt.Application.Products.DTOs;
using CubArt.Domain.Entities;
using CubArt.Domain.Enums;
using CubArt.Domain.Exceptions;
using CubArt.Infrastructure.Common;
using CubArt.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CubArt.Application.Products.Handlers
{
    public class CreateOrUpdateProductCommandHandler : IRequestHandler<CreateOrUpdateProductCommand, Result<ProductDto>>
    {
        private readonly IRepository<Product, int> _productRepository;
        private readonly IRepository<ProductSpecification, int> _specificationRepository;
        private readonly IRepository<ProductSpecificationItem, int> _specificationItemRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CreateOrUpdateProductCommandHandler(
            IRepository<Product, int> productRepository,
            IRepository<ProductSpecification, int> specificationRepository,
            IRepository<ProductSpecificationItem, int> specificationItemRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _productRepository = productRepository;
            _specificationRepository = specificationRepository;
            _specificationItemRepository = specificationItemRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<ProductDto>> Handle(
            CreateOrUpdateProductCommand request,
            CancellationToken cancellationToken)
        {
            var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                Product product;

                if (request.Id.HasValue)
                {
                    // Обновление существующего продукта
                    product = await UpdateProductAsync(request.Id.Value, request, cancellationToken);

                    // Обработка спецификации если предоставлена
                    if (request.Specification != null && request.Specification.Items.Any())
                    {
                        await HandleProductSpecificationAsync(product, request.Specification, cancellationToken);
                    }
                }
                else
                {
                    // Создание нового продукта
                    product = await CreateProductAsync(request, cancellationToken);

                    // Для нового продукта всегда создаем спецификацию если предоставлена
                    if (request.Specification != null && request.Specification.Items.Any())
                    {
                        await CreateNewSpecificationAsync(product, request.Specification, cancellationToken);
                    }
                }

                await _unitOfWork.CommitAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(transaction, cancellationToken);

                // Загружаем полные данные для ответа
                var fullProduct = await _productRepository.GetQueryable()
                    .Include(p => p.ProductSpecifications.Where(ps => ps.IsActive))
                    .ThenInclude(ps => ps.Items)
                    .ThenInclude(psi => psi.Product)
                    .FirstOrDefaultAsync(p => p.Id == product.Id, cancellationToken);

                var result = _mapper.Map<ProductDto>(fullProduct);
                return Result.Success(result);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(transaction, cancellationToken);
                return Result.Failure<ProductDto>($"Ошибка сохранения продукции: {ex.Message}");
            }
        }

        private async Task<Product> CreateProductAsync(CreateOrUpdateProductCommand request, CancellationToken cancellationToken)
        {
            // Проверяем уникальность имени
            var existingProduct = await _productRepository.GetQueryable()
                .FirstOrDefaultAsync(p => p.Name == request.Name, cancellationToken);

            if (existingProduct != null)
            {
                throw new DomainException($"Продукция с названием '{request.Name}' уже существует");
            }

            // Создаем продукт
            var product = new Product(request.Name, request.ProductType, request.UnitOfMeasure);
            await _productRepository.AddAsync(product);
            await _unitOfWork.CommitAsync(cancellationToken);

            return product;
        }

        private async Task<Product> UpdateProductAsync(int productId, CreateOrUpdateProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
            {
                throw new NotFoundException(nameof(Product), productId);
            }

            // Проверяем уникальность имени (исключая текущий продукт)
            var existingProduct = await _productRepository.GetQueryable()
                .FirstOrDefaultAsync(p => p.Name == request.Name && p.Id != productId, cancellationToken);

            if (existingProduct != null)
            {
                throw new DomainException($"Продукция с названием '{request.Name}' уже существует");
            }

            // Обновляем свойства
            product.Name = request.Name;
            product.ProductType = request.ProductType;
            product.UnitOfMeasure = request.UnitOfMeasure;

            _productRepository.Update(product);

            return product;
        }

        private async Task HandleProductSpecificationAsync(
        Product product,
        ProductSpecificationData specificationData,
        CancellationToken cancellationToken)
        {
            // Получаем активную спецификацию продукта
            var activeSpecification = await _specificationRepository.GetQueryable()
                .Include(ps => ps.Items)
                .FirstOrDefaultAsync(ps => ps.ProductId == product.Id && ps.IsActive, cancellationToken);

            if (activeSpecification != null)
            {
                // Сравниваем с активной спецификацией
                if (AreSpecificationsEqual(activeSpecification.Items.ToList(), specificationData.Items))
                {
                    // Спецификации идентичны - просто обновляем свойства если нужно
                    await UpdateSpecificationPropertiesAsync(activeSpecification, specificationData, cancellationToken);
                }
                else
                {
                    // Спецификации разные - создаем новую и делаем ее активной
                    await DeactivateSpecificationAsync(activeSpecification, cancellationToken);
                    await CreateNewSpecificationAsync(product, specificationData, cancellationToken);                    
                }
            }
            else
            {
                // Нет активной спецификации - создаем новую
                await CreateNewSpecificationAsync(product, specificationData, cancellationToken);
            }
        }

        private bool AreSpecificationsEqual(List<ProductSpecificationItem> existingItems, List<SpecificationItem> newItems)
        {
            if (existingItems.Count != newItems.Count)
                return false;

            // Создаем словари для сравнения
            var existingDict = existingItems
                .GroupBy(x => x.ProductId)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

            var newDict = newItems
                .GroupBy(x => x.ProductId)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

            // Сравниваем количество по каждому ProductId
            if (existingDict.Count != newDict.Count)
                return false;

            foreach (var kvp in existingDict)
            {
                if (!newDict.TryGetValue(kvp.Key, out var newQuantity) ||
                    Math.Abs(newQuantity - kvp.Value) > 0.001m) // учитываем погрешность decimal
                {
                    return false;
                }
            }

            return true;
        }

        private async Task UpdateSpecificationPropertiesAsync(
            ProductSpecification specification,
            ProductSpecificationData specificationData,
            CancellationToken cancellationToken)
        {
            bool wasUpdated = false;

            // Обновляем версию если предоставлена и отличается
            if (!string.IsNullOrEmpty(specificationData.Version) &&
                specification.Version != specificationData.Version)
            {
                // Проверяем уникальность новой версии
                var existingWithSameVersion = await _specificationRepository.GetQueryable()
                    .FirstOrDefaultAsync(ps =>
                        ps.ProductId == specification.ProductId &&
                        ps.Id != specification.Id &&
                        ps.Version == specificationData.Version,
                        cancellationToken);

                if (existingWithSameVersion != null)
                {
                    throw new DomainException(
                        $"Спецификация с версией '{specificationData.Version}' уже существует для этого продукта");
                }

                specification.Version = specificationData.Version;
                wasUpdated = true;
            }

            // Обновляем активность если нужно
            if (specificationData.SetAsActive != specification.IsActive)
            {
                if (specificationData.SetAsActive)
                {
                    // Деактивируем другие спецификации перед активацией этой
                    await DeactivateOtherSpecificationsAsync(specification.ProductId, specification.Id, cancellationToken);
                }
                specification.IsActive = specificationData.SetAsActive;
                wasUpdated = true;
            }

            if (wasUpdated)
            {
                _specificationRepository.Update(specification);
                await _unitOfWork.CommitAsync(cancellationToken);
            }
        }

        private async Task CreateNewSpecificationAsync(
            Product product,
            ProductSpecificationData specificationData,
            CancellationToken cancellationToken)
        {
            // Проверяем уникальность версии спецификации
            if (!string.IsNullOrEmpty(specificationData.Version))
            {
                var existingVersion = await _specificationRepository.GetQueryable()
                    .FirstOrDefaultAsync(ps =>
                        ps.ProductId == product.Id &&
                        ps.Version == specificationData.Version,
                        cancellationToken);

                if (existingVersion != null)
                {
                    throw new DomainException(
                        $"Спецификация с версией '{specificationData.Version}' уже существует для этого продукта");
                }
            }

            // Деактивируем предыдущие активные спецификации если нужно
            if (specificationData.SetAsActive)
            {
                await DeactivateOtherSpecificationsAsync(product.Id, null, cancellationToken);
            }

            // Проверяем все компоненты спецификации
            foreach (var item in specificationData.Items)
            {
                var componentProduct = await _productRepository.GetByIdAsync(item.ProductId);
                if (componentProduct == null)
                {
                    throw new DomainException($"Компонент с ID {item.ProductId} не найден");
                }

                if (!ValidateComponent(product, componentProduct))
                {
                    throw new DomainException(
                        $"Недопустимый компонент {componentProduct.Name} ({componentProduct.ProductType}) " +
                        $"для продукта {product.Name} ({product.ProductType})");
                }
            }

            // Создаем спецификацию
            var specification = new ProductSpecification(
                product.Id,
                specificationData.Version,
                specificationData.SetAsActive);

            await _specificationRepository.AddAsync(specification);
            await _unitOfWork.CommitAsync(cancellationToken);

            // Создаем элементы спецификации
            foreach (var item in specificationData.Items)
            {
                var specificationItem = new ProductSpecificationItem(
                    specification.Id, item.ProductId, item.Quantity);
                await _specificationItemRepository.AddAsync(specificationItem);
            }
        }

        private async Task DeactivateOtherSpecificationsAsync(int productId, int? excludeSpecificationId, CancellationToken cancellationToken)
        {
            var query = _specificationRepository.GetQueryable()
                .Where(ps => ps.ProductId == productId && ps.IsActive);

            if (excludeSpecificationId.HasValue)
            {
                query = query.Where(ps => ps.Id != excludeSpecificationId.Value);
            }

            var activeSpecifications = await query.ToListAsync(cancellationToken);

            foreach (var activeSpec in activeSpecifications)
            {
                activeSpec.IsActive = false;
                _specificationRepository.Update(activeSpec);
            }
        }

        private async Task DeactivateSpecificationAsync(ProductSpecification specification, CancellationToken cancellationToken)
        {
            specification.IsActive = false;
            _specificationRepository.Update(specification);
            await _unitOfWork.CommitAsync(cancellationToken);
        }


        private static bool ValidateComponent(Product mainProduct, Product componentProduct)
        {
            return mainProduct.ProductType switch
            {
                // Сырье не может иметь компонентов
                ProductTypeEnum.RawMaterial => false,

                // Полуфабрикат может содержать только сырье
                ProductTypeEnum.SemiFinished =>
                    componentProduct.ProductType == ProductTypeEnum.RawMaterial,

                // Сложный полуфабрикат может содержать сырье и простые полуфабрикаты
                ProductTypeEnum.ComplexSemiFinished =>
                    componentProduct.ProductType == ProductTypeEnum.RawMaterial ||
                    componentProduct.ProductType == ProductTypeEnum.SemiFinished,

                // Готовая продукция может содержать все типы компонентов, кроме другой готовой продукции
                ProductTypeEnum.FinishedProduct =>
                    componentProduct.ProductType != ProductTypeEnum.FinishedProduct,

                _ => false
            };
        }
    }

}
