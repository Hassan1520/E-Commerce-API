using ECommerce.Application.DTOs.Products;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Interfaces.Repositories;

public interface IProductRepository : IRepository<Product>
{
    Task<IEnumerable<Product>> GetAllWithCategoryAsync();
    Task<(IEnumerable<Product> Data, int TotalCount)> GetPagedProductsAsync(ProductSpecParams @params);
    Task<Product?> GetByIdWithCategoryAsync(int id);
    Task<bool> DeductStockAsync(int productId, int quantity);
    Task RestoreStockAsync(int productId, int quantity);
}
