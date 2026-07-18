using AutoMapper;
using ECommerce.Application.Common.Exceptions;
using ECommerce.Application.DTOs.Cart;
using ECommerce.Application.Interfaces.Repositories;
using MediatR;

namespace ECommerce.Application.Features.Cart.Commands.DeleteCart;

public class RemoveFromCartCommandHandler : IRequestHandler<RemoveFromCartCommand, CartDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public RemoveFromCartCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CartDto> Handle(RemoveFromCartCommand request, CancellationToken cancellationToken)
    {
        var cartItem = await _unitOfWork.Carts.GetByUserAndProductAsync(request.UserId, request.Dto.ProductId)
            ?? throw new NotFoundException("Item not found in cart.");

        _unitOfWork.Carts.Remove(cartItem);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var items = await _unitOfWork.Carts.GetByUserIdAsync(request.UserId);
        return _mapper.Map<CartDto>(items);
    }
}