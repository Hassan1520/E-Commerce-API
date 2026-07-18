namespace ECommerce.Application.Interfaces.Services;

public interface IDistributedLockService
{
    /// <summary>
    /// ÈíÍÇæá íÍÌÒ ÇáÜ Lock.
    /// ÈíÑÌÚ true áæ äÌÍ¡ false áæ ÍÏ ÊÇäí ÔÇíá ÇáÜ Lock.
    /// </summary>
    Task<(bool Acquired, string LockValue)> TryAcquireAsync(string key, TimeSpan expiry);

    /// <summary>
    /// ÈíÍÑÑ ÇáÜ Lock — ÈÓ áæ ÅäÊ Çááí ÍÌÒÊå (Atomic Release).
    /// </summary>
    Task ReleaseAsync(string key, string lockValue);
}