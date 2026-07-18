using AutoMapper;
using ECommerce.Application.Common;
using ECommerce.Application.Common.Exceptions;
using ECommerce.Application.DTOs.Products;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Entities;
using MediatR;

namespace ECommerce.Application.Features.Products.Commands.UpdateProductImage;

public class UploadProductImageCommandHandler : IRequestHandler<UploadProductImageCommand, ProductImageDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBlobStorageService _blobStorageService;
    private readonly IMapper _mapper;
    private readonly ICacheService _cache;

    public UploadProductImageCommandHandler(
        IUnitOfWork unitOfWork,
        IBlobStorageService blobStorageService,
        IMapper mapper,
        ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _blobStorageService = blobStorageService;
        _mapper = mapper;
        _cache = cache;
    }

    public async Task<ProductImageDto> Handle(UploadProductImageCommand request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetByIdWithCategoryAsync(request.ProductId)
            ?? throw new NotFoundException($"Product with id {request.ProductId} not found.");

        var uploadResult = await _blobStorageService.UploadAsync(
            request.FileStream, request.FileName, request.ContentType, AzureStorageContainers.ProductImages);

        var isFirstImage = !product.Images.Any();
        var image = new ProductImage
        {
            ProductId = request.ProductId,
            ImageUrl = uploadResult.Url,
            BlobName = uploadResult.BlobName,
            IsMain = isFirstImage,
            DisplayOrder = product.Images.Count
        };

        await _unitOfWork.ProductImages.AddAsync(image);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await Task.WhenAll(
            _cache.RemoveAsync(CacheKeys.AllProducts),
            _cache.RemoveAsync(CacheKeys.Product(request.ProductId)),
            _cache.RemoveAsync(CacheKeys.ProductImages(request.ProductId)));

        return _mapper.Map<ProductImageDto>(image);
    }
}