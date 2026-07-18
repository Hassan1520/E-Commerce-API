using ECommerce.Application.Common;
using ECommerce.Application.Common.Exceptions;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using MediatR;

namespace ECommerce.Application.Features.Products.Commands.UpdateProductImage;

public class SetMainProductImageCommandHandler : IRequestHandler<SetMainProductImageCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;

    public SetMainProductImageCommandHandler(IUnitOfWork unitOfWork, ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<Unit> Handle(SetMainProductImageCommand request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetByIdWithCategoryAsync(request.ProductId)
            ?? throw new NotFoundException($"Product with id {request.ProductId} not found.");

        if (!product.Images.Any(i => i.Id == request.ImageId))
            throw new NotFoundException("Image not found for this product.");

        foreach (var img in product.Images)
            img.IsMain = img.Id == request.ImageId;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await Task.WhenAll(
            _cache.RemoveAsync(CacheKeys.AllProducts),
            _cache.RemoveAsync(CacheKeys.Product(request.ProductId)),
            _cache.RemoveAsync(CacheKeys.ProductImages(request.ProductId)));

        return Unit.Value;
    }
}
