using AutoMapper;
using ECommerce.Application.Common.Exceptions;
using ECommerce.Application.DTOs.Cart;
using ECommerce.Application.Interfaces.Repositories;
using MediatR;

namespace ECommerce.Application.Features.Cart.Commands.UpdateCart;

public class UpdateCartItemCommandHandler : IRequestHandler<UpdateCartItemCommand, CartDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateCartItemCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CartDto> Handle(UpdateCartItemCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        var cartItem = await _unitOfWork.Carts.GetByUserAndProductAsync(request.UserId, dto.ProductId)
            ?? throw new NotFoundException("Item not found in cart.");

        var product = await _unitOfWork.Products.GetByIdAsync(dto.ProductId)
            ?? throw new NotFoundException($"Product with id {dto.ProductId} not found.");

        if (product.Stock < dto.Quantity)
            throw new AppException($"Insufficient stock. Only {product.Stock} items available.");

        cartItem.Quantity = dto.Quantity;
        _unitOfWork.Carts.Update(cartItem);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var items = await _unitOfWork.Carts.GetByUserIdAsync(request.UserId);
        return _mapper.Map<CartDto>(items);
    }
}