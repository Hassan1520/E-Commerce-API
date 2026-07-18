using ECommerce.Application.Interfaces.Services;
using StackExchange.Redis;

using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Services;

public class RedisDistributedLockService : IDistributedLockService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisDistributedLockService> _logger;

    // Lua Script ÷«„‰ ≈‰ «·‹ Release ⁄„·Ì… Atomic
    // Ì⁄‰Ì „‘ „„ﬂ‰ ‰„”Õ Lock „‘ » «⁄‰«
    private const string ReleaseLockScript = @"
        if redis.call('get', KEYS[1]) == ARGV[1] then
            return redis.call('del', KEYS[1])
        else
            return 0
        end";

    public RedisDistributedLockService(
        IConnectionMultiplexer redis,
        ILogger<RedisDistributedLockService> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task<(bool Acquired, string LockValue)> TryAcquireAsync(string key, TimeSpan expiry)
    {
        var db = _redis.GetDatabase();

        // ﬂ· Lock ·ÌÂ ﬁÌ„… Unique (GUID) ⁄‘«‰ ‰⁄—› ≈‰Â » «⁄‰« ·„« ‰Õ——Â
        var lockValue = Guid.NewGuid().ToString("N");

        // SetIfNotExists (NX) + Expiry (EX) ›Ì √„— Ê«Õœ = Atomic
        var acquired = await db.StringSetAsync(
            key,
            lockValue,
            expiry,
            When.NotExists // NX: «⁄„· SET »” ·Ê «·‹ Key „‘ „ÊÃÊœ
        );

        if (!acquired)
            _logger.LogDebug("Lock NOT acquired for key: {Key}. Another process holds it.", key);

        return (acquired, lockValue);
    }

    public async Task ReleaseAsync(string key, string lockValue)
    {
        var db = _redis.GetDatabase();

        // »‰‘€¯· «·‹ Lua Script «··Ì »Ì‘Ìﬂ «·√Ê· ≈‰ «·‹ Value » «⁄ ‰« ﬁ»· „« Ì„”Õ
        await db.ScriptEvaluateAsync(
            ReleaseLockScript,
            keys: [key],
            values: [lockValue]
        );
    }
}