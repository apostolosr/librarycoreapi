using LibraryCoreApi.DTOs;

namespace LibraryCoreApi.Services.Categories;

public interface ICategoriesService
{
    Task<IEnumerable<CategoryDto>> GetCategories();
    Task<CategoryDto> GetCategory(int id);
    Task<CategoryDto> CreateCategory(CreateCategoryDto createDto);
    Task<CategoryDto> UpdateCategory(int id, UpdateCategoryDto updateDto);
    Task DeleteCategory(int id);
}
