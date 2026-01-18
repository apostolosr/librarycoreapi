using Microsoft.EntityFrameworkCore;
using LibraryCoreApi.Entities;

namespace LibraryCoreApi.Database;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(DataContext context)
    {
        // Seed Roles
        if (!await context.Roles.AnyAsync())
        {
            var roles = new[]
            {
                new Role
                {
                    Name = "Author",
                    Description = "A person who writes books",
                    CreatedAt = DateTime.UtcNow
                },
                new Role
                {
                    Name = "Customer",
                    Description = "A library customer who can borrow books",
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.Roles.AddRangeAsync(roles);
        }
        else
        {
            // Ensure both roles exist (in case only one was added manually)
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
        }

        // Seed Categories
        if (!await context.Categories.AnyAsync())
        {
            var categories = new[]
            {
                new Category
                {
                    Name = "Fiction",
                    CreatedAt = DateTime.UtcNow
                },
                new Category
                {
                    Name = "Mystery",
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.Categories.AddRangeAsync(categories);
        }
        else
        {
            // Ensure both categories exist (in case only one was added manually)
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
        }

        await context.SaveChangesAsync();
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        DatabaseSeeder.SeedAsync(host.Services.GetRequiredService<DataContext>()).Wait();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                services.AddDbContext<DataContext>(options =>
                    options.UseNpgsql(hostContext.Configuration.GetConnectionString("WebApiDatabase")));
            });
}