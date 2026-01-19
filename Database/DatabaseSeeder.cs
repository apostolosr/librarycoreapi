using Microsoft.EntityFrameworkCore;
using LibraryCoreApi.Entities;

namespace LibraryCoreApi.Database;

/// <summary>
/// Database seeder class to seed the database with initial data
/// </summary>
public static class DatabaseSeeder
{
    /// <summary>
    /// Seed the database with initial roles, categories and a party
    /// </summary>
    /// <param name="context">The database context</param>
    /// <returns>A task representing the asynchronous operation</returns>
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
            

        // Seed Parties
        await context.Parties.AddAsync(new Party
        {
            Name = "A R",
            Email = "a.r@example.com",
            Phone = "1234567890",
            Address = "Galaxy Far Far Away",
            CreatedAt = DateTime.UtcNow,
            PartyRoles = new List<PartyRole>
            {
                new PartyRole
                {
                    RoleId = 1,
                    AssignedAt = DateTime.UtcNow
                },
                new PartyRole
                {
                    RoleId = 2,
                    AssignedAt = DateTime.UtcNow
                }
            }
        });

        await context.SaveChangesAsync();
    }
}