using AutoMapper;
using ECommerce.Application.Common.Exceptions;
using ECommerce.Application.DTOs.Cart;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Domain.Entities;
using MediatR;

namespace ECommerce.Application.Features.Cart.Commands.CreateCart;

public class AddToCartCommandHandler : IRequestHandler<AddToCartCommand, CartDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public AddToCartCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CartDto> Handle(AddToCartCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        var product = await _unitOfWork.Products.GetByIdAsync(dto.ProductId)
            ?? throw new NotFoundException($"Product with id {dto.ProductId} not found.");

        if (product.Stock < dto.Quantity)
            throw new AppException($"Insufficient stock. Only {product.Stock} items available.");

        var existingItem = await _unitOfWork.Carts.GetByUserAndProductAsync(request.UserId, dto.ProductId);

        if (existingItem is not null)
        {
            var newQuantity = existingItem.Quantity + dto.Quantity;
            if (product.Stock < newQuantity)
                throw new AppException($"Insufficient stock. Only {product.Stock} items available.");
            existingItem.Quantity = newQuantity;
            _unitOfWork.Carts.Update(existingItem);
        }
        else
        {
            await _unitOfWork.Carts.AddAsync(new CartItem
            {
                UserId = request.UserId,
                ProductId = dto.ProductId,
                Quantity = dto.Quantity
            });
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        var items = await _unitOfWork.Carts.GetByUserIdAsync(request.UserId);
        return _mapper.Map<CartDto>(items);
    }
}