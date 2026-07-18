using AutoMapper;
using ECommerce.Application.Common.Exceptions;
using ECommerce.Application.DTOs.Cart;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Domain.Entities;
using MediatR;

namespace ECommerce.Application.Features.Cart.Queries.GetCart;

public class GetCartQueryHandler : IRequestHandler<GetCartQuery, CartDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetCartQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CartDto> Handle(GetCartQuery request, CancellationToken cancellationToken)
    {
        var items = (await _unitOfWork.Carts.GetByUserIdAsync(request.UserId)).ToList();
        if (items.Count == 0)
            throw new NotFoundException("Cart is empty.");
        return _mapper.Map<CartDto>(items);
    }
}