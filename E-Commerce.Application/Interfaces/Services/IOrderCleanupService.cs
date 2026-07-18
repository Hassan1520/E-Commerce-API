namespace ECommerce.Application.Interfaces.Services;

public interface IOrderCleanupService
{
    Task ReleaseAbandonedOrdersAsync();
}