using ECommerce.Infrastructure.Data;
using ECommerce.Domain.Entities;
using ECommerce.Application.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Category?> GetByNameAsync(string name) =>
        await _dbSet.FirstOrDefaultAsync(c => c.Name == name);
}
