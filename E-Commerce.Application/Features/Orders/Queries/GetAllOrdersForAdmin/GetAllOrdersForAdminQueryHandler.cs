using AutoMapper;
using ECommerce.Application.DTOs.Orders;
using ECommerce.Application.Interfaces.Repositories;
using MediatR;

namespace ECommerce.Application.Features.Orders.Queries.GetAllOrdersForAdmin;

public class GetAllOrdersForAdminQueryHandler : IRequestHandler<GetAllOrdersForAdminQuery, IEnumerable<OrderDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllOrdersForAdminQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<OrderDto>> Handle(GetAllOrdersForAdminQuery request, CancellationToken cancellationToken)
    {
        var orders = await _unitOfWork.Orders.GetAllOrdersWithItemsAsync();
        return _mapper.Map<IEnumerable<OrderDto>>(orders);
    }
}