using ECommerce.Infrastructure.Data;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

public class RefreshTokenRepository : Repository<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token) =>
        await _dbSet.FirstOrDefaultAsync(rt => rt.Token == token && !rt.IsRevoked);

    public async Task RevokeUserTokensAsync(int userId)
    {
        var tokens = await _dbSet.Where(rt => rt.UserId == userId && !rt.IsRevoked).ToListAsync();
        foreach (var token in tokens)
            token.IsRevoked = true;
    }
}
