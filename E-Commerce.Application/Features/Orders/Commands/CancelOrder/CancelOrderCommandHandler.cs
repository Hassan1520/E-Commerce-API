using AutoMapper;
using ECommerce.Application.Common.Exceptions;
using ECommerce.Application.Common.Extensions;
using ECommerce.Application.DTOs.Orders;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Domain.Enums;
using MediatR;

namespace ECommerce.Application.Features.Orders.Commands.CancelOrder;

public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, OrderDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CancelOrderCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<OrderDto> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = request.IsAdmin
            ? await _unitOfWork.Orders.GetByIdWithItemsAsync(request.OrderId)
            : await _unitOfWork.Orders.GetByIdWithItemsAsync(request.OrderId, request.UserId);

        if (order is null)
            throw new NotFoundException($"Order with id {request.OrderId} not found.");

        if (order.Status == OrderStatus.Cancelled)
            throw new AppException("Order is already cancelled.");

        if (order.Status == OrderStatus.Delivered)
            throw new AppException("Cannot cancel a delivered order.");

        if (!request.IsAdmin && order.Status != OrderStatus.Confirmed)
            throw new AppException("You can only cancel a Confirmed order.");

        await _unitOfWork.Products.RestoreStockForOrderAsync(order);

        order.Status = OrderStatus.Cancelled;
        _unitOfWork.Orders.Update(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return _mapper.Map<OrderDto>(order);
    }
}