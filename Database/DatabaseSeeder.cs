using Microsoft.EntityFrameworkCore;
using LibraryCoreApi.Entities;

namespace LibraryCoreApi.Database;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(DataContext context)
    {
        // Seed Roles
        var authorExists = await context.Roles.AnyAsync(r => r.Name == "Author");
        var customerExists = await context.Roles.AnyAsync(r => r.Name == "Customer");

        if (!authorExists)
        {
            await context.Roles.AddAsync(new Role
            {
                Name = "Author",
                Description = "A person who writes books",
                CreatedAt = DateTime.UtcNow
            });
        }

        if (!customerExists)
        {
            await context.Roles.AddAsync(new Role
            {
                Name = "Customer",
                Description = "A library customer who can borrow books",
                CreatedAt = DateTime.UtcNow
            });
        }
        
        // Seed Categories
        var fictionExists = await context.Categories.AnyAsync(c => c.Name == "Fiction");
        var mysteryExists = await context.Categories.AnyAsync(c => c.Name == "Mystery");

        if (!fictionExists)
        {
            await context.Categories.AddAsync(new Category
            {
                Name = "Fiction",
                CreatedAt = DateTime.UtcNow
            });
        }

        if (!mysteryExists)
        {
            await context.Categories.AddAsync(new Category
            {
                Name = "Mystery",
                CreatedAt = DateTime.UtcNow
            });
        }
            
        await context.SaveChangesAsync();
    }
}