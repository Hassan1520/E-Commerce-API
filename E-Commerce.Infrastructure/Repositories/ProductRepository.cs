using ECommerce.Infrastructure.Data;
using ECommerce.Application.DTOs.Products;
using ECommerce.Domain.Entities;
using ECommerce.Application.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Product>> GetAllWithCategoryAsync() =>
        await _dbSet.Include(p => p.Category).Include(p => p.Images).ToListAsync();

    public async Task<Product?> GetByIdWithCategoryAsync(int id) =>
        await _dbSet.Include(p => p.Category).Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == id);
    public async Task<(IEnumerable<Product> Data, int TotalCount)> GetPagedProductsAsync(ProductSpecParams @params)
    {
        // 1. ‰»œ√ »‹ IQueryable ⁄‘«‰ „« ‰”Õ»‘ «·œ« « „‰ «·œ« « »Ì“ €Ì— ·„« ‰Œ·’ «·›· —…
        var query = _dbSet.Include(p => p.Category).AsQueryable();

        // 2.  ÿ»Ìﬁ «·‹ Search («·»ÕÀ »«”„ «·„‰ Ã √Ê «·Ê’› „À·«)
        if (!string.IsNullOrEmpty(@params.Search))
        {
            var searchLower = @params.Search.ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(searchLower));
            // Ê ﬁœ—  “Êœ: || p.Description.ToLower().Contains(searchLower) ·Ê ⁄‰œﬂ Õﬁ· Ê’›
        }

        if (@params.CategoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == @params.CategoryId.Value);
        }

        // 3. Õ”«» ≈Ã„«·Ì «·„‰ Ã«  »⁄œ «·›· —… (⁄‘«‰ «·›—Ê‰  ≈Ì‰œ ÌÕ”» ⁄œœ «·’›Õ« )
        var totalCount = await query.CountAsync();

        // 4.  ÿ»Ìﬁ «·‹ Pagination ( ŒÿÌ «·’›Õ«  «·”«»ﬁ… Ê√Œ– ÕÃ„ «·’›Õ… «·Õ«·Ì…)
        var data = await query
            .Skip((@params.PageNumber - 1) * @params.PageSize)
            .Take(@params.PageSize)
            .ToListAsync();

        return (data, totalCount);
    }
    public async Task<bool> DeductStockAsync(int productId, int quantity)
    {
        // Ã„·… SQL Ê«Õœ… Atomic:
        // UPDATE Products SET Stock = Stock - @quantity
        // WHERE Id = @productId AND Stock >= @quantity
        // ·Ê «·‹ Stock „ﬂ‰‘ ﬂ›«Ì…° „‘ ÂÌ ⁄„· Update Ê„‘ ÂÌ—Ã⁄ rows
        var rows = await _context.Database.ExecuteSqlRawAsync(
            "UPDATE Products SET Stock = Stock - {0} WHERE Id = {1} AND Stock >= {0}",
            quantity, productId);

        return rows > 0; 
    }
    public async Task RestoreStockAsync(int productId, int quantity)
    {
        // Ã„·… SQL Ê«Õœ… Atomic: » —Ã⁄ «·ﬂ„Ì… ··„Œ“Ê‰ „»«‘—… ›Ì «·œ« «»Ì“
        // „‰ €Ì— „« ‰⁄„· GetById +  ⁄œÌ· ›Ì «·‹ memory + Update
        // (ﬂœÂ »‰ﬁ›· ‰›” ‰«›–… «·‹ Race Condition «··Ì DeductStockAsync »Ìﬁ›·Â«)
        await _context.Database.ExecuteSqlRawAsync(
            "UPDATE Products SET Stock = Stock + {0} WHERE Id = {1}",
            quantity, productId);
    }
}
