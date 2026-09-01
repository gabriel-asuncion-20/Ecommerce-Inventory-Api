using System.Linq;
using System.Threading.Tasks;
using BCrypt.Net;
using EcommerceInventoryApi.Core.Entities;

namespace EcommerceInventoryApi.Infrastructure.Data
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            await context.Database.EnsureCreatedAsync();

            // Seed Users if empty
            if (!context.Users.Any())
            {
                var adminUser = new User
                {
                    Email = "admin@ecommerce.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                    FirstName = "Admin",
                    LastName = "System",
                    Role = UserRole.Admin
                };

                var customerUser = new User
                {
                    Email = "customer@ecommerce.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Customer123!"),
                    FirstName = "Juan",
                    LastName = "Pérez",
                    Role = UserRole.Customer
                };

                await context.Users.AddRangeAsync(adminUser, customerUser);
                await context.SaveChangesAsync();
            }

            // Seed Categories if empty
            if (!context.Categories.Any())
            {
                var electronicsCategory = new Category
                {
                    Name = "Electrónica",
                    Description = "Laptops, smartphones y accesorios tecnológicos de última generación."
                };

                var homeCategory = new Category
                {
                    Name = "Hogar y Cocina",
                    Description = "Electrodomésticos y decoración para el hogar."
                };

                await context.Categories.AddRangeAsync(electronicsCategory, homeCategory);
                await context.SaveChangesAsync();

                // Seed Products
                if (!context.Products.Any())
                {
                    var products = new[]
                    {
                        new Product
                        {
                            Name = "Laptop Pro 15\"",
                            SKU = "TECH-LAP-001",
                            Description = "Procesador de alto rendimiento 16GB RAM SSD 512GB",
                            Price = 1299.99m,
                            StockQuantity = 25,
                            ImageUrl = "https://images.unsplash.com/photo-1496181133206-80ce9b88a853",
                            CategoryId = electronicsCategory.Id
                        },
                        new Product
                        {
                            Name = "Smartphone Galaxy S",
                            SKU = "TECH-PHN-002",
                            Description = "Pantalla AMOLED 120Hz 128GB cámara de 108MP",
                            Price = 899.50m,
                            StockQuantity = 40,
                            ImageUrl = "https://images.unsplash.com/photo-1511707171634-5f897ff02aa9",
                            CategoryId = electronicsCategory.Id
                        },
                        new Product
                        {
                            Name = "Cafetera Espreso Italiana",
                            SKU = "HOME-COF-003",
                            Description = "Cafetera de presión 15 bares con vaporizador de leche",
                            Price = 149.00m,
                            StockQuantity = 15,
                            ImageUrl = "https://images.unsplash.com/photo-1514432324607-a09d9b4aefdd",
                            CategoryId = homeCategory.Id
                        }
                    };

                    await context.Products.AddRangeAsync(products);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
