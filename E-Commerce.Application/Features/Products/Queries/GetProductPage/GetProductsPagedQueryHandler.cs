using AutoMapper;
using ECommerce.Application.Common;
using ECommerce.Application.DTOs.Products;
using ECommerce.Application.Interfaces.Repositories;
using MediatR;

namespace ECommerce.Application.Features.Products.Queries.GetProductPage;

public class GetProductsPagedQueryHandler : IRequestHandler<GetProductsPagedQuery, PaginationResult<ProductDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetProductsPagedQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginationResult<ProductDto>> Handle(GetProductsPagedQuery request, CancellationToken cancellationToken)
    {
        var (products, totalCount) = await _unitOfWork.Products.GetPagedProductsAsync(request.Params);
        var dtos = _mapper.Map<IEnumerable<ProductDto>>(products);
        return new PaginationResult<ProductDto>(request.Params.PageNumber, request.Params.PageSize, totalCount, dtos);
    }
}