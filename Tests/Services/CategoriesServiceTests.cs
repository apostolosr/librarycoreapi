using LibraryCoreApi.Database;
using LibraryCoreApi.Services.Categories;
using Xunit;
using Microsoft.EntityFrameworkCore;
using LibraryCoreApi.Entities;
using LibraryCoreApi.Events;
using Moq;
using LibraryCoreApi.DTOs;

namespace LibraryCoreApi.Tests.Services;

public class CategoriesServiceTests : IDisposable
{
    private readonly DataContext _dbContext;
    public CategoriesServiceTests()
    {
        _dbContext = CreateContext();
        _dbContext.Database.EnsureCreated();
    }

    private static DataContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new DataContext(options);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

  
    [Fact]
    public async Task TestGetCategories()
    {
        // Arrange
        var categories = new List<Category>
        {
            new Category { Name = "Test Category 1" },
            new Category { Name = "Test Category 2" }
        };
        _dbContext.Categories.AddRange(categories);
        await _dbContext.SaveChangesAsync();

        var categoriesService = new CategoriesService(_dbContext, new Mock<IEventPublisher>().Object);

        var categoriesDto = await categoriesService.GetCategories();
        Assert.Equal(2, categoriesDto.Count());
        Assert.Equal(categories.First().Id, categoriesDto.First().Id);
        Assert.Equal(categories.First().Name, categoriesDto.First().Name);
    }

    [Fact]
    public async Task TestGetCategoriesByCategoryId()
    {
        // Arrange
        var category = MockHelper.GetMockCategory();
        _dbContext.Categories.Add(category);
        await _dbContext.SaveChangesAsync();

        var categoriesService = new CategoriesService(_dbContext, new Mock<IEventPublisher>().Object);

        var categoryDto = await categoriesService.GetCategory(category.Id);
        Assert.NotNull(categoryDto);
        Assert.Equal(category.Id, categoryDto.Id);
        Assert.Equal(category.Name, categoryDto.Name);
    }

    [Fact]
    public async Task TestGetCategoriesCategoryIdDoesNotExist()
    {
        // Arrange
        var category = MockHelper.GetMockCategory();
        _dbContext.Categories.Add(category);
        await _dbContext.SaveChangesAsync();

        var categoriesService = new CategoriesService(_dbContext, new Mock<IEventPublisher>().Object);
        
        // Act, Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => categoriesService.GetCategory(MockHelper.CategoryId + 1));
    }

    [Fact]
    public async Task TestCreateCategory()
    {
        // Arrange
        var createDto = new CreateCategoryDto
        {
            Name = "Test Category"
        };
        var mockEventPublisher = new Mock<IEventPublisher>();   
        mockEventPublisher.Setup(m => m.PublishEvent(It.IsAny<string>(), It.IsAny<object>())).Returns(Task.CompletedTask);
        var categoriesService = new CategoriesService(_dbContext, mockEventPublisher.Object);

        // Act
        var result = await categoriesService.CreateCategory(createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createDto.Name, result.Name);
        Assert.Equal(0, result.BookCount);
        Assert.Equal(DateTime.UtcNow.Date, result.CreatedAt.Date);
        Assert.Equal(DateTime.UtcNow.Minute, result.CreatedAt.Minute);
        mockEventPublisher.Verify(m => m.PublishEvent(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
    }

    [Fact]
    public async Task TestUpdateCategory()
    {
        // Arrange
        var category = MockHelper.GetMockCategory();
        _dbContext.Categories.Add(category);
        await _dbContext.SaveChangesAsync();
        var mockEventPublisher = new Mock<IEventPublisher>();
        mockEventPublisher.Setup(m => m.PublishEvent(It.IsAny<string>(), It.IsAny<object>())).Returns(Task.CompletedTask);
        var categoriesService = new CategoriesService(_dbContext, mockEventPublisher.Object);
        var updateDto = new UpdateCategoryDto
        {
            Name = "Updated Category"
        };

        // Act
        CategoryDto result = await categoriesService.UpdateCategory(category.Id, updateDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(updateDto.Name, result.Name);
        Assert.Equal(0, result.BookCount);
        mockEventPublisher.Verify(m => m.PublishEvent(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
    }

    [Fact]
    public async Task TestDeleteCategory()
    {
        // Arrange
        var category = new Category
        {
            Name = "Test Category"
        };
        _dbContext.Categories.Add(category);
        await _dbContext.SaveChangesAsync();

        var categoriesService = new CategoriesService(_dbContext, new Mock<IEventPublisher>().Object);

        // Act
        await categoriesService.DeleteCategory(category.Id);

        // Assert
        Assert.Null(await _dbContext.Categories.FindAsync(category.Id));
    }
}
