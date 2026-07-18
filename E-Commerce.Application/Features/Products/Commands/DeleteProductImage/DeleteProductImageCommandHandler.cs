using ECommerce.Application.Common;
using ECommerce.Application.Common.Exceptions;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Entities;
using MediatR;

namespace ECommerce.Application.Features.Products.Commands.DeleteProductImage;

public class DeleteProductImageCommandHandler : IRequestHandler<DeleteProductImageCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBlobStorageService _blobStorageService;
    private readonly ICacheService _cache;

    public DeleteProductImageCommandHandler(
        IUnitOfWork unitOfWork,
        IBlobStorageService blobStorageService,
        ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _blobStorageService = blobStorageService;
        _cache = cache;
    }

    public async Task<Unit> Handle(DeleteProductImageCommand request, CancellationToken cancellationToken)
    {
        var image = await _unitOfWork.ProductImages.GetByIdAsync(request.ImageId)
            ?? throw new NotFoundException("Image not found.");

        if (image.ProductId != request.ProductId)
            throw new AppException("This image does not belong to the specified product.");

        await _blobStorageService.DeleteAsync(image.BlobName, AzureStorageContainers.ProductImages);
        _unitOfWork.ProductImages.Remove(image);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await Task.WhenAll(
            _cache.RemoveAsync(CacheKeys.AllProducts),
            _cache.RemoveAsync(CacheKeys.Product(request.ProductId)),
            _cache.RemoveAsync(CacheKeys.ProductImages(request.ProductId)));

        return Unit.Value;
    }
}