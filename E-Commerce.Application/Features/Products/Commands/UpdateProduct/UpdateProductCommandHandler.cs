using AutoMapper;
using ECommerce.Application.Common;
using ECommerce.Application.Common.Exceptions;
using ECommerce.Application.DTOs.Products;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using MediatR;

namespace ECommerce.Application.Features.Products.Commands.UpdateProduct;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ProductDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICacheService _cache;

    public UpdateProductCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _cache = cache;
    }

    public async Task<ProductDto> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(request.Id)
            ?? throw new NotFoundException($"Product with id {request.Id} not found.");

        if (!await _unitOfWork.Categories.ExistsAsync(c => c.Id == request.Dto.CategoryId))
            throw new NotFoundException($"Category with id {request.Dto.CategoryId} not found.");

        _mapper.Map(request.Dto, product);
        _unitOfWork.Products.Update(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await Task.WhenAll(
            _cache.RemoveAsync(CacheKeys.AllProducts),
            _cache.RemoveAsync(CacheKeys.Product(request.Id)));

        var updated = await _unitOfWork.Products.GetByIdWithCategoryAsync(request.Id);
        return _mapper.Map<ProductDto>(updated);
    }
}
