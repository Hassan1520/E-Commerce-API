using AutoMapper;
using ECommerce.Application.Common;
using ECommerce.Application.DTOs.Products;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using MediatR;

namespace ECommerce.Application.Features.Products.Queries.GetAllProducts;

public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, IEnumerable<ProductDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICacheService _cache;
    private static readonly TimeSpan ListCacheTtl = TimeSpan.FromMinutes(5);

    public GetAllProductsQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _cache = cache;
    }

    public async Task<IEnumerable<ProductDto>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        return await _cache.GetOrSetAsync(
            CacheKeys.AllProducts,
            async () =>
            {
                var products = await _unitOfWork.Products.GetAllWithCategoryAsync();
                return _mapper.Map<IEnumerable<ProductDto>>(products);
            },
            ListCacheTtl);
    }
}