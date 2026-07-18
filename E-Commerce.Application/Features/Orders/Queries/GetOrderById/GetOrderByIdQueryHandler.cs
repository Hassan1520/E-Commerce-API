using AutoMapper;
using ECommerce.Application.Common.Exceptions;
using ECommerce.Application.DTOs.Orders;
using ECommerce.Application.Interfaces.Repositories;
using MediatR;

namespace ECommerce.Application.Features.Orders.Queries.GetOrderById;

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetOrderByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<OrderDto> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = request.IsAdmin
            ? await _unitOfWork.Orders.GetByIdWithItemsAsync(request.OrderId)
            : await _unitOfWork.Orders.GetByIdWithItemsAsync(request.OrderId, request.UserId);

        if (order is null)
            throw new NotFoundException($"Order with id {request.OrderId} not found.");

        return _mapper.Map<OrderDto>(order);
    }
}