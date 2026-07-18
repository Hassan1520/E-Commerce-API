using AutoMapper;
using ECommerce.Application.Common;
using ECommerce.Application.Common.Exceptions;
using ECommerce.Application.DTOs.Categories;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using MediatR;

namespace ECommerce.Application.Features.Categories.Queries.GetCategorieById;

public class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, CategoryDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICacheService _cache;
    private static readonly TimeSpan SingleCacheTtl = TimeSpan.FromMinutes(20);

    public GetCategoryByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _cache = cache;
    }

    public async Task<CategoryDto> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        return await _cache.GetOrSetAsync(
            CacheKeys.Category(request.Id),
            async () =>
            {
                var category = await _unitOfWork.Categories.GetByIdAsync(request.Id)
                    ?? throw new NotFoundException($"Category with id {request.Id} not found.");
                return _mapper.Map<CategoryDto>(category);
            },
            SingleCacheTtl);
    }
}