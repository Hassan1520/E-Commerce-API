namespace ECommerce.Application.Common;

public static class CacheKeys
{
    public const string AllProducts = "products:all";
    public static string Product(int id) => $"products:{id}";
    public static string ProductImages(int productId) => $"products:{productId}:images";

    public const string AllCategories = "categories:all";
    public static string Category(int id) => $"categories:{id}";

    public static string CheckoutLock(int userId) => $"lock:checkout:user:{userId}";
}
