using AutoMapper;
using ECommerce.Application.Common;
using ECommerce.Application.Common.Exceptions;
using ECommerce.Application.DTOs.Categories;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using MediatR;

namespace ECommerce.Application.Features.Categories.Commands.UpdateCategorie;

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, CategoryDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICacheService _cache;

    public UpdateCategoryCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _cache = cache;
    }

    public async Task<CategoryDto> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(request.Id)
            ?? throw new NotFoundException($"Category with id {request.Id} not found.");

        if (await _unitOfWork.Categories.ExistsAsync(c => c.Name == request.Dto.Name && c.Id != request.Id))
            throw new ConflictException($"Category '{request.Dto.Name}' already exists.");

        category.Name = request.Dto.Name;
        _unitOfWork.Categories.Update(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await Task.WhenAll(
            _cache.RemoveAsync(CacheKeys.AllCategories),
            _cache.RemoveAsync(CacheKeys.Category(request.Id)));

        return _mapper.Map<CategoryDto>(category);
    }
}