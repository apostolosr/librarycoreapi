using Microsoft.EntityFrameworkCore;
using LibraryCoreApi.Database;
using LibraryCoreApi.Entities;
using LibraryCoreApi.DTOs;
using LibraryCoreApi.Errors;

namespace LibraryCoreApi.Services.Categories;

public class CategoriesService : ICategoriesService
{
    private readonly DataContext _context;

    public CategoriesService(DataContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CategoryDto>> GetCategories()
    {
        var categories = await _context.Categories
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                BookCount = c.Books.Count,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();

        return categories;
    }

    public async Task<CategoryDto> GetCategory(int id)
    {
        var category = await _context.Categories
            .Include(c => c.Books)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
        {
            throw new ApiException("Category not found");
        }

        var categoryDto = new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            BookCount = category.Books.Count,
            CreatedAt = category.CreatedAt
        };

        return categoryDto;
    }

    public async Task<CategoryDto> CreateCategory(CreateCategoryDto createDto)
    {
        var category = new Category
        {
            Name = createDto.Name,
            CreatedAt = DateTime.UtcNow
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var categoryDto = new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            BookCount = 0,
            CreatedAt = category.CreatedAt
        };

        return categoryDto;
    }

    public async Task<CategoryDto> UpdateCategory(int id, UpdateCategoryDto updateDto)
    {
        var category = await _context.Categories.FindAsync(id);

        if (category == null)
        {
            throw new ApiException("Category not found");
        }

        category.Name = updateDto.Name;
        category.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Reload with books count
        await _context.Entry(category)
            .Collection(c => c.Books)
            .LoadAsync();

        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            BookCount = category.Books.Count,
            CreatedAt = category.CreatedAt
        };
    }

    public async Task DeleteCategory(int id)
    {
        var category = await _context.Categories
            .Include(c => c.Books)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
        {
            throw new ApiException("Category not found");
        }

        if (category.Books.Any())
        {
            throw new ApiException("Cannot delete category with existing books");
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
    }
}
