using AutoMapper;
using ECommerce.Application.Common.Exceptions;
using ECommerce.Application.DTOs.Orders;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Domain.Enums;
using MediatR;

namespace ECommerce.Application.Features.Orders.Commands.UpdateOrder;

public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, OrderDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateOrderStatusCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<OrderDto> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await _unitOfWork.Orders.GetByIdWithItemsAsync(request.OrderId)
            ?? throw new NotFoundException($"Order with id {request.OrderId} not found.");

        if (!Enum.TryParse<OrderStatus>(request.Dto.Status, ignoreCase: true, out var newStatus))
            throw new AppException($"Invalid status: {request.Dto.Status}");

        var validTransitions = new Dictionary<OrderStatus, OrderStatus>
        {
            { OrderStatus.Confirmed, OrderStatus.Shipped },
            { OrderStatus.Shipped, OrderStatus.Delivered }
        };

        if (!validTransitions.TryGetValue(order.Status, out var expectedNext))
            throw new AppException($"Cannot change status from '{order.Status}'. Order is either Delivered or Cancelled.");

        if (newStatus != expectedNext)
            throw new AppException($"Invalid transition. Order is '{order.Status}', next allowed status is '{expectedNext}'.");

        order.Status = newStatus;
        _unitOfWork.Orders.Update(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return _mapper.Map<OrderDto>(order);
    }
}