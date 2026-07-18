using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(
        ApplicationDbContext context,
        UserManager<User> userManager,
        RoleManager<IdentityRole<int>> roleManager)
    {
        await context.Database.MigrateAsync();
        await SeedRolesAsync(roleManager);

        if (await userManager.Users.AnyAsync())
            return;

        var admin = new User
        {
            Name = "Admin",
            UserName = "admin@ecommerce.com",
            Email = "admin@ecommerce.com",
            CreatedAt = DateTime.UtcNow
        };

        var customer = new User
        {
            Name = "John Doe",
            UserName = "customer@ecommerce.com",
            Email = "customer@ecommerce.com",
            CreatedAt = DateTime.UtcNow
        };

        await userManager.CreateAsync(admin, "Admin@123");
        await userManager.AddToRoleAsync(admin, UserRole.Admin.ToString());

        await userManager.CreateAsync(customer, "Customer@123");
        await userManager.AddToRoleAsync(customer, UserRole.Customer.ToString());

        var categories = new List<Category>
        {
            new() { Name = "Electronics" },
            new() { Name = "Clothing" },
            new() { Name = "Books" },
            new() { Name = "Home & Garden" }
        };

        context.Categories.AddRange(categories);
        await context.SaveChangesAsync();

        var products = new List<Product>
        {
            new() { Name = "Wireless Headphones", Description = "Noise-cancelling over-ear headphones with 30h battery life.", Price = 149.99m, Stock = 50, CategoryId = categories[0].Id },
            new() { Name = "Smartphone", Description = "Latest generation smartphone with 128GB storage.", Price = 799.99m, Stock = 30, CategoryId = categories[0].Id },
            new() { Name = "Laptop", Description = "15-inch laptop with 16GB RAM and 512GB SSD.", Price = 1299.99m, Stock = 20, CategoryId = categories[0].Id },
            new() { Name = "T-Shirt", Description = "Premium cotton t-shirt, available in multiple colors.", Price = 24.99m, Stock = 100, CategoryId = categories[1].Id },
            new() { Name = "Jeans", Description = "Classic fit denim jeans.", Price = 59.99m, Stock = 75, CategoryId = categories[1].Id },
            new() { Name = "Clean Code", Description = "A Handbook of Agile Software Craftsmanship by Robert C. Martin.", Price = 34.99m, Stock = 40, CategoryId = categories[2].Id },
            new() { Name = "Design Patterns", Description = "Elements of Reusable Object-Oriented Software.", Price = 44.99m, Stock = 35, CategoryId = categories[2].Id },
            new() { Name = "Coffee Maker", Description = "Programmable drip coffee maker with thermal carafe.", Price = 89.99m, Stock = 25, CategoryId = categories[3].Id },
            new() { Name = "Desk Lamp", Description = "LED desk lamp with adjustable brightness.", Price = 39.99m, Stock = 60, CategoryId = categories[3].Id }
        };

        context.Products.AddRange(products);
        await context.SaveChangesAsync();
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole<int>> roleManager)
    {
        foreach (var role in new[] { UserRole.Admin, UserRole.Customer })
        {
            var roleName = role.ToString();
            if (!await roleManager.RoleExistsAsync(roleName))
                await roleManager.CreateAsync(new IdentityRole<int> { Name = roleName });
        }
    }
}
