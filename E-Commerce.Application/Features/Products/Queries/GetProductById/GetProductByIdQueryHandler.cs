using AutoMapper;
using ECommerce.Application.Common;
using ECommerce.Application.Common.Exceptions;
using ECommerce.Application.DTOs.Products;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using MediatR;

namespace ECommerce.Application.Features.Products.Queries.GetProductById;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICacheService _cache;
    private static readonly TimeSpan SingleCacheTtl = TimeSpan.FromMinutes(10);

    public GetProductByIdQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _cache = cache;
    }

    public async Task<ProductDto> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        return await _cache.GetOrSetAsync(
            CacheKeys.Product(request.Id),
            async () =>
            {
                var product = await _unitOfWork.Products.GetByIdWithCategoryAsync(request.Id)
                    ?? throw new NotFoundException($"Product with id {request.Id} not found.");
                return _mapper.Map<ProductDto>(product);
            },
            SingleCacheTtl);
    }
}