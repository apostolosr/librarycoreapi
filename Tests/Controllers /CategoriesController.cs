using Moq;
using LibraryCoreApi.Controllers;
using LibraryCoreApi.Services.Categories;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using LibraryCoreApi.DTOs;

namespace LibraryCoreApi.Tests.Controllers;

public class CategoriesControllerTests
{
    private readonly Mock<ICategoriesService> _mockCategoriesService;
    private readonly CategoriesController _categoriesController;

    public CategoriesControllerTests()
    {
        _mockCategoriesService = new Mock<ICategoriesService>();
        _categoriesController = new CategoriesController(_mockCategoriesService.Object);
    }

    [Fact]
    public async Task GetCategories_ReturnsOkResult_WithListOfCategories()
    {
        // Arrange
        var expectedCategories = new List<CategoryDto> 
        { 
            MockHelper.GetMockCategoryDto()
        };
        _mockCategoriesService.Setup(s => s.GetCategories()).ReturnsAsync(expectedCategories);

        // Act
        var result = await _categoriesController.GetCategories();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var categories = Assert.IsAssignableFrom<IEnumerable<CategoryDto>>(okResult.Value);
        Assert.Single(categories);
        Assert.Equal(200, okResult.StatusCode);
        _mockCategoriesService.Verify(s => s.GetCategories(), Times.Once);
    }

    [Fact]
    public async Task GetCategories_ReturnsOkResult_WithEmptyList()
    {
        // Arrange
        var emptyList = new List<CategoryDto>();
        _mockCategoriesService.Setup(s => s.GetCategories()).ReturnsAsync(emptyList);

        // Act
        var result = await _categoriesController.GetCategories();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var categories = Assert.IsAssignableFrom<IEnumerable<CategoryDto>>(okResult.Value);
        Assert.Empty(categories);
        _mockCategoriesService.Verify(s => s.GetCategories(), Times.Once);
    }

    [Fact]
    public async Task GetCategory_ReturnsOkResult_WhenCategoryExists()
    {
        // Arrange
        var expectedCategory = MockHelper.GetMockCategoryDto();
        _mockCategoriesService.Setup(s => s.GetCategory(It.IsAny<int>())).ReturnsAsync(expectedCategory);

        // Act
        var result = await _categoriesController.GetCategory(MockHelper.CategoryId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var category = Assert.IsType<CategoryDto>(okResult.Value);
        Assert.Equal(MockHelper.CategoryId, category.Id);
        Assert.Equal(MockHelper.CategoryName, category.Name);
        Assert.Equal(200, okResult.StatusCode);
        _mockCategoriesService.Verify(s => s.GetCategory(MockHelper.CategoryId), Times.Once);
    }

    [Fact]
    public async Task CreateCategory_ReturnsCreatedAtAction_WhenCategoryIsCreated()
    {
        // Arrange
        var createDto = new CreateCategoryDto
        {
            Name = MockHelper.CategoryName,
        };
        _mockCategoriesService.Setup(s => s.CreateCategory(It.IsAny<CreateCategoryDto>())).ReturnsAsync(MockHelper.GetMockCategoryDto());

        // Act
        var result = await _categoriesController.CreateCategory(createDto);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var category = Assert.IsType<CategoryDto>(createdAtActionResult.Value);
        Assert.Equal(MockHelper.CategoryId, category.Id);
        Assert.Equal(MockHelper.CategoryName, category.Name);
        Assert.Equal(201, createdAtActionResult.StatusCode);
        Assert.Equal(nameof(CategoriesController.GetCategory), createdAtActionResult.ActionName);
        _mockCategoriesService.Verify(s => s.CreateCategory(It.Is<CreateCategoryDto>(d => d.Name == MockHelper.CategoryName)), Times.Once);
    }


    [Fact]
    public async Task UpdateCategory_ReturnsOkResult_WhenCategoryIsUpdated()
    {   
        // Arrange
        var updateDto = new UpdateCategoryDto
        {
            Name = "Updated Name",
        };
        _mockCategoriesService.Setup(s => s.UpdateCategory(It.IsAny<int>(), It.IsAny<UpdateCategoryDto>())).ReturnsAsync(MockHelper.GetMockCategoryDto());

        // Act
        var result = await _categoriesController.UpdateCategory(MockHelper.CategoryId, updateDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var category = Assert.IsType<CategoryDto>(okResult.Value);
        Assert.Equal(MockHelper.CategoryId, category.Id);
        Assert.Equal(MockHelper.CategoryName, category.Name);
        Assert.Equal(200, okResult.StatusCode);
        _mockCategoriesService.Verify(s => s.UpdateCategory(MockHelper.CategoryId, It.IsAny<UpdateCategoryDto>()), Times.Once);
    }

    [Fact]
    public async Task DeleteCategory_ReturnsNoContent_WhenCategoryIsDeleted()
    {
        // Arrange
        _mockCategoriesService.Setup(s => s.DeleteCategory(It.IsAny<int>()));

        // Act
        var result = await _categoriesController.DeleteCategory(MockHelper.CategoryId);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result);
        Assert.Equal(204, noContentResult.StatusCode);
        _mockCategoriesService.Verify(s => s.DeleteCategory(MockHelper.CategoryId), Times.Once);
    }

}