using Moq;
using LibraryCoreApi.Controllers;
using LibraryCoreApi.Services.Books;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using LibraryCoreApi.DTOs;
using LibraryCoreApi.Errors;

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
    public async Task GetBook_ReturnsNotFound_WhenBookDoesNotExist()
    {
        // Arrange
        _mockBooksService.Setup(s => s.GetBook(It.IsAny<int>())).ReturnsAsync((BookDto)null!);

        // Act
        var result = await _booksController.GetBook(999);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundResult>(result.Result);
        Assert.Equal(404, notFoundResult.StatusCode);
        _mockBooksService.Verify(s => s.GetBook(999), Times.Once);
    }

    [Fact]
    public async Task GetBookAvailability_ReturnsOkResult_WhenBookExists()
    {
        // Arrange
        var expectedAvailability = new BookAvailabilityDto
        {
            BookId = MockHelper.BookId,
            Title = MockHelper.Title,
            ISBN = MockHelper.ISBN,
            TotalCopies = 10,
            AvailableCopies = 8,
            Copies = new List<BookCopyInfoDto>
            {
                new BookCopyInfoDto { CopyId = 1, CopyNumber = "1-1", IsAvailable = true },
                new BookCopyInfoDto { CopyId = 2, CopyNumber = "1-2", IsAvailable = false, CurrentBorrower = "John Doe" }
            }
        };
        _mockBooksService.Setup(s => s.GetBookAvailability(It.IsAny<int>())).ReturnsAsync(expectedAvailability);

        // Act
        var result = await _booksController.GetBookAvailability(MockHelper.BookId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var availability = Assert.IsType<BookAvailabilityDto>(okResult.Value);
        Assert.Equal(MockHelper.BookId, availability.BookId);
        Assert.Equal(10, availability.TotalCopies);
        Assert.Equal(8, availability.AvailableCopies);
        Assert.Equal(200, okResult.StatusCode);
        _mockBooksService.Verify(s => s.GetBookAvailability(MockHelper.BookId), Times.Once);
    }

    [Fact]
    public async Task GetBookAvailability_ReturnsNotFound_WhenBookDoesNotExist()
    {
        // Arrange
        _mockBooksService.Setup(s => s.GetBookAvailability(It.IsAny<int>())).ReturnsAsync((BookAvailabilityDto)null!);

        // Act
        var result = await _booksController.GetBookAvailability(999);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundResult>(result.Result);
        Assert.Equal(404, notFoundResult.StatusCode);
        _mockBooksService.Verify(s => s.GetBookAvailability(999), Times.Once);
    }

    [Fact]
    public async Task GetBookAvailabilityByTitle_ReturnsOkResult_WhenBookExists()
    {
        // Arrange
        var expectedAvailability = new BookAvailabilityDto
        {
            BookId = MockHelper.BookId,
            Title = MockHelper.Title,
            ISBN = MockHelper.ISBN,
            TotalCopies = 5,
            AvailableCopies = 3
        };
        _mockBooksService.Setup(s => s.GetBookAvailabilityByTitle(It.IsAny<string>())).ReturnsAsync(expectedAvailability);

        // Act
        var result = await _booksController.GetBookAvailabilityByTitle(MockHelper.Title);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var availability = Assert.IsType<BookAvailabilityDto>(okResult.Value);
        Assert.Equal(MockHelper.Title, availability.Title);
        Assert.Equal(200, okResult.StatusCode);
        _mockBooksService.Verify(s => s.GetBookAvailabilityByTitle(MockHelper.Title), Times.Once);
    }

    [Fact]
    public async Task GetBookAvailabilityByTitle_ReturnsNotFound_WhenBookDoesNotExist()
    {
        // Arrange
        _mockBooksService.Setup(s => s.GetBookAvailabilityByTitle(It.IsAny<string>())).ReturnsAsync((BookAvailabilityDto)null!);

        // Act
        var result = await _booksController.GetBookAvailabilityByTitle("Non-existent Book");

        // Assert
        var notFoundResult = Assert.IsType<NotFoundResult>(result.Result);
        Assert.Equal(404, notFoundResult.StatusCode);
        _mockBooksService.Verify(s => s.GetBookAvailabilityByTitle("Non-existent Book"), Times.Once);
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
            ISBN = MockHelper.ISBN,
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
        _mockBooksService.Setup(s => s.DeleteBook(It.IsAny<int>())).ReturnsAsync(deletedBook);

        // Act
        var result = await _booksController.DeleteBook(MockHelper.BookId);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result);
        Assert.Equal(204, noContentResult.StatusCode);
        _mockBooksService.Verify(s => s.DeleteBook(MockHelper.BookId), Times.Once);
    }
}