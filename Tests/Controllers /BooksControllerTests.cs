using Moq;
using LibraryCoreApi.Controllers;
using LibraryCoreApi.Services.Books;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using LibraryCoreApi.DTOs;

namespace LibraryCoreApi.Tests.Controllers;

public class BooksControllerTests
{
    private readonly Mock<IBooksService> _mockBooksService;
    private readonly BooksController _booksController;

    public BooksControllerTests()
    {
        _mockBooksService = new Mock<IBooksService>();
        _booksController = new BooksController(_mockBooksService.Object);
    }

    [Fact]
    public async Task GetBooks_ReturnsOkResult_WithListOfBooks()
    {
        // Arrange
        var expectedBooks = new List<BookDto> 
        { 
            MockHelper.GetMockBookDto()
        };
        _mockBooksService.Setup(s => s.GetBooks()).ReturnsAsync(expectedBooks);

        // Act
        var result = await _booksController.GetBooks();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var books = Assert.IsAssignableFrom<IEnumerable<BookDto>>(okResult.Value);
        Assert.Single(books);
        Assert.Equal(200, okResult.StatusCode);
        _mockBooksService.Verify(s => s.GetBooks(), Times.Once);
    }

    [Fact]
    public async Task GetBooks_ReturnsOkResult_WithEmptyList()
    {
        // Arrange
        var emptyList = new List<BookDto>();
        _mockBooksService.Setup(s => s.GetBooks()).ReturnsAsync(emptyList);

        // Act
        var result = await _booksController.GetBooks();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var books = Assert.IsAssignableFrom<IEnumerable<BookDto>>(okResult.Value);
        Assert.Empty(books);
        _mockBooksService.Verify(s => s.GetBooks(), Times.Once);
    }

    [Fact]
    public async Task GetBook_ReturnsOkResult_WhenBookExists()
    {
        // Arrange
        var expectedBook = MockHelper.GetMockBookDto();
        _mockBooksService.Setup(s => s.GetBook(It.IsAny<int>())).ReturnsAsync(expectedBook);

        // Act
        var result = await _booksController.GetBook(MockHelper.BookId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var book = Assert.IsType<BookDto>(okResult.Value);
        Assert.Equal(MockHelper.BookId, book.Id);
        Assert.Equal(MockHelper.Title, book.Title);
        Assert.Equal(200, okResult.StatusCode);
        _mockBooksService.Verify(s => s.GetBook(MockHelper.BookId), Times.Once);
    }

    [Fact]
    public async Task CreateBook_ReturnsCreatedAtAction_WhenBookIsCreated()
    {
        // Arrange
        var createDto = new CreateBookDto
        {
            Title = MockHelper.Title,
            ISBN = MockHelper.ISBN,
            Description = MockHelper.Description,
            AuthorId = MockHelper.AuthorId,
            CategoryId = MockHelper.CategoryId,
            PublishedDate = MockHelper.PublishedDate,
            NumberOfCopies = 3
        };
        var createdBook = MockHelper.GetMockBookDto();
        _mockBooksService.Setup(s => s.CreateBook(It.IsAny<CreateBookDto>())).ReturnsAsync(createdBook);

        // Act
        var result = await _booksController.CreateBook(createDto);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var book = Assert.IsType<BookDto>(createdAtActionResult.Value);
        Assert.Equal(MockHelper.BookId, book.Id);
        Assert.Equal(nameof(BooksController.GetBook), createdAtActionResult.ActionName);
        Assert.Equal(201, createdAtActionResult.StatusCode);
        _mockBooksService.Verify(s => s.CreateBook(It.Is<CreateBookDto>(d => d.Title == MockHelper.Title)), Times.Once);
    }

    [Fact]
    public async Task UpdateBook_ReturnsOkResult_WhenBookIsUpdated()
    {
        // Arrange
        var updateDto = new UpdateBookDto
        {
            Title = "Updated Title",
            Description = "Updated Description",
            CategoryId = MockHelper.CategoryId,
            PublishedDate = MockHelper.PublishedDate
        };
        var updatedBook = MockHelper.GetMockBookDto();
        updatedBook.Title = "Updated Title";
        _mockBooksService.Setup(s => s.UpdateBook(It.IsAny<int>(), It.IsAny<UpdateBookDto>())).ReturnsAsync(updatedBook);

        // Act
        var result = await _booksController.UpdateBook(MockHelper.BookId, updateDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var book = Assert.IsType<BookDto>(okResult.Value);
        Assert.Equal("Updated Title", book.Title);
        Assert.Equal(200, okResult.StatusCode);
        _mockBooksService.Verify(s => s.UpdateBook(MockHelper.BookId, It.IsAny<UpdateBookDto>()), Times.Once);
    }

    [Fact]
    public async Task DeleteBook_ReturnsNoContent_WhenBookIsDeleted()
    {
        // Arrange
        var deletedBook = MockHelper.GetMockBookDto();
        _mockBooksService.Setup(s => s.DeleteBook(It.IsAny<int>()));

        // Act
        var result = await _booksController.DeleteBook(MockHelper.BookId);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result);
        Assert.Equal(204, noContentResult.StatusCode);
        _mockBooksService.Verify(s => s.DeleteBook(MockHelper.BookId), Times.Once);
    }
}