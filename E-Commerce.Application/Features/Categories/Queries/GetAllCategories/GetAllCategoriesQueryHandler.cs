
using AutoMapper;
using ECommerce.Application.Common;
using ECommerce.Application.DTOs.Categories;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using MediatR;

namespace ECommerce.Application.Features.Categories.Queries.GetAllCategories
{
    public class GetAllCategoriesQueryHandler : IRequestHandler<GetAllCategoriesQuery, IEnumerable<CategoryDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICacheService _cache;
        private static readonly TimeSpan ListCacheTtl = TimeSpan.FromMinutes(15);

        public GetAllCategoriesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ICacheService cache)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cache = cache;
        }

        public async Task<IEnumerable<CategoryDto>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
        {
            return await _cache.GetOrSetAsync(
                CacheKeys.AllCategories,
                async () =>
                {
                    var categories = await _unitOfWork.Categories.GetAllAsync();
                    return _mapper.Map<IEnumerable<CategoryDto>>(categories);
                },
                ListCacheTtl);
        }
    }
}
