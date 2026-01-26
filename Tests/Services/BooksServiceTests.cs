using LibraryCoreApi.Database;
using LibraryCoreApi.Services.Books;
using Xunit;
using Microsoft.EntityFrameworkCore;
using LibraryCoreApi.Entities;
using LibraryCoreApi.Events;
using Moq;
using LibraryCoreApi.DTOs;
using LibraryCoreApi.Errors;

namespace LibraryCoreApi.Tests.Services;

public class BooksServiceTests : IDisposable
{
    private readonly DataContext _dbContext;
    public BooksServiceTests()
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
    public async Task TestGetBooks()
    {
        // Arrange

        var role = MockHelper.GetMockRole();
        _dbContext.Roles.Add(role);

        var partyRole = MockHelper.GetMockPartyRole();
        partyRole.Role = role;
        _dbContext.PartyRoles.Add(partyRole);

        var party = MockHelper.GetMockParty();
        party.PartyRoles.Add(partyRole);
        _dbContext.Parties.Add(party);

        var category = MockHelper.GetMockCategory();
        _dbContext.Categories.Add(category);

        var book = MockHelper.GetMockBook();
        book.Author = party;
        book.Category = category;
        _dbContext.Books.Add(book);
        await _dbContext.SaveChangesAsync();

        var booksService = new BooksService(_dbContext, new Mock<IEventPublisher>().Object);

        var booksDto = await booksService.GetBooks();
        Assert.Single(booksDto);
        Assert.Equal(book.Id, booksDto.First().Id);
        Assert.Equal(book.Title, booksDto.First().Title);
        Assert.Equal(book.ISBN, booksDto.First().ISBN);
        Assert.Equal(book.Description, booksDto.First().Description);
        Assert.Equal(book.AuthorId, booksDto.First().AuthorId);
        Assert.Equal(book.Author.Name, booksDto.First().AuthorName);
        Assert.Equal(book.CategoryId, booksDto.First().CategoryId);
        Assert.Equal(book.Category.Name, booksDto.First().CategoryName);
        Assert.Equal(book.PublishedDate, booksDto.First().PublishedDate);
        Assert.Equal(book.Copies.Count, booksDto.First().TotalCopies);
        Assert.Equal(book.Copies.Count(c => c.IsAvailable), booksDto.First().AvailableCopies);
        Assert.Equal(book.CreatedAt, booksDto.First().CreatedAt);
    }

    [Fact]
    public async Task TestGetBookByBookId()
    {
        // Arrange
        var role = new Role 
        { 
            Id = MockHelper.RoleId, 
            Name = MockHelper.CustomerRoleName, 
            Description = MockHelper.RoleDescription, 
            CreatedAt = DateTime.UtcNow 
        };
        _dbContext.Roles.Add(role);

        var partyRole = MockHelper.GetMockPartyRole();
        partyRole.Role = role;
        _dbContext.PartyRoles.Add(partyRole);

        var party = MockHelper.GetMockParty();
        party.PartyRoles.Add(partyRole);
        _dbContext.Parties.Add(party);

        var category = MockHelper.GetMockCategory();
        _dbContext.Categories.Add(category);

        var book = MockHelper.GetMockBook();
        book.Author = party;
        book.Category = category;
        _dbContext.Books.Add(book);
        await _dbContext.SaveChangesAsync();

        var booksService = new BooksService(_dbContext, new Mock<IEventPublisher>().Object);

        var bookDto = await booksService.GetBook(book.Id);
        Assert.NotNull(bookDto);
        Assert.Equal(book.Id, bookDto.Id);
        Assert.Equal(book.Title, bookDto.Title);
    }

    [Fact]
    public async Task TestGetBookByBookIdDoesNotExist()
    {
        // Arrange
        var book = MockHelper.GetMockBook();
        _dbContext.Books.Add(book);
        await _dbContext.SaveChangesAsync();

        var booksService = new BooksService(_dbContext, new Mock<IEventPublisher>().Object);
        
        // Act, Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => booksService.GetBook(MockHelper.BookId + 1));
    }

    [Fact]
    public async Task TestCreateBookAuthorDoesNotExist()
    {
        // Arrange
        var createDto = new CreateBookDto
        {
            Title = "Test Book",
            ISBN = "1234567890",
            Description = "Test Description",
            AuthorId = MockHelper.AuthorId,
            CategoryId = MockHelper.CategoryId,
            PublishedDate = MockHelper.PublishedDate,
            NumberOfCopies = 1
        };
        var mockEventPublisher = new Mock<IEventPublisher>();   
        mockEventPublisher.Setup(m => m.PublishEvent(It.IsAny<string>(), It.IsAny<object>())).Returns(Task.CompletedTask);
        var booksService = new BooksService(_dbContext, mockEventPublisher.Object);

        // Act, Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => booksService.CreateBook(createDto));
        Assert.Equal("Author not found", exception.Message);
    }

    [Fact]
    public async Task TestCreateBookPartyDoesNotHaveAuthorRole()
    {
        // Arrange
        var party = MockHelper.GetMockParty();

        var role = MockHelper.GetMockRole();
        role.Name = MockHelper.CustomerRoleName;
        _dbContext.Roles.Add(role);

        var partyRole = MockHelper.GetMockPartyRole();
        partyRole.Role = role;
        _dbContext.PartyRoles.Add(partyRole);

        party.PartyRoles.Add(partyRole);
        _dbContext.Parties.Add(party);
        
        await _dbContext.SaveChangesAsync();

        var createDto = new CreateBookDto
        {
            Title = "Test Book",
            ISBN = "1234567890",
            Description = "Test Description",
            AuthorId = MockHelper.AuthorId,
            CategoryId = MockHelper.CategoryId,
            PublishedDate = MockHelper.PublishedDate,
            NumberOfCopies = 1
        };
        var mockEventPublisher = new Mock<IEventPublisher>();   
        mockEventPublisher.Setup(m => m.PublishEvent(It.IsAny<string>(), It.IsAny<object>())).Returns(Task.CompletedTask);
        var booksService = new BooksService(_dbContext, mockEventPublisher.Object);

        // Act, Assert
        var exception = await Assert.ThrowsAsync<ApiException>(() => booksService.CreateBook(createDto));
        Assert.Equal("Party does not have Author role", exception.Message);
    }

    [Fact]
    public async Task TestCreateBookPartyCategoryDoesNotExist()
    {
        // Arrange
        var party = MockHelper.GetMockParty();

        var role = MockHelper.GetMockRole();
        _dbContext.Roles.Add(role);

        var partyRole = MockHelper.GetMockPartyRole();
        partyRole.Role = role;
        _dbContext.PartyRoles.Add(partyRole);
        
        party.PartyRoles.Add(partyRole);
        _dbContext.Parties.Add(party);
        
        await _dbContext.SaveChangesAsync();

        var createDto = new CreateBookDto
        {
            Title = "Test Book",
            ISBN = "1234567890",
            Description = "Test Description",
            AuthorId = MockHelper.AuthorId,
            CategoryId = MockHelper.CategoryId,
            PublishedDate = MockHelper.PublishedDate,
            NumberOfCopies = 1
        };
        var mockEventPublisher = new Mock<IEventPublisher>();   
        mockEventPublisher.Setup(m => m.PublishEvent(It.IsAny<string>(), It.IsAny<object>())).Returns(Task.CompletedTask);
        var booksService = new BooksService(_dbContext, mockEventPublisher.Object);

        // Act, Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => booksService.CreateBook(createDto));
        Assert.Equal("Category not found", exception.Message);
    }

    [Fact]
    public async Task TestCreateBookISBNAlreadyExists()
    {
        // Arrange
        var category = MockHelper.GetMockCategory();
        _dbContext.Categories.Add(category);
        
        var role = MockHelper.GetMockRole();
        _dbContext.Roles.Add(role);

        var partyRole = MockHelper.GetMockPartyRole();
        partyRole.Role = role;
        _dbContext.PartyRoles.Add(partyRole);

        var party = MockHelper.GetMockParty();
        party.PartyRoles.Add(partyRole);
        _dbContext.Parties.Add(party);

        var book = MockHelper.GetMockBook();
        _dbContext.Books.Add(book);
        
        await _dbContext.SaveChangesAsync();

        var createDto = new CreateBookDto
        {
            Title = "Test Book",
            ISBN = MockHelper.ISBN,
            Description = "Test Description",
            AuthorId = MockHelper.AuthorId,
            CategoryId = MockHelper.CategoryId,
            PublishedDate = MockHelper.PublishedDate,
            NumberOfCopies = 1
        };
        var mockEventPublisher = new Mock<IEventPublisher>();   
        mockEventPublisher.Setup(m => m.PublishEvent(It.IsAny<string>(), It.IsAny<object>())).Returns(Task.CompletedTask);
        var booksService = new BooksService(_dbContext, mockEventPublisher.Object);

        // Act, Assert
        var exception = await Assert.ThrowsAsync<ApiException>(() => booksService.CreateBook(createDto));
        Assert.Equal("Book with this ISBN already exists", exception.Message);
    }

     [Fact]
    public async Task TestCreateBookSuccessfully()
    {
        // Arrange
        var category = MockHelper.GetMockCategory();
        _dbContext.Categories.Add(category);

        var role = MockHelper.GetMockRole();
        _dbContext.Roles.Add(role);

        var partyRole = MockHelper.GetMockPartyRole();
        partyRole.Role = role;
        _dbContext.PartyRoles.Add(partyRole);

        var party = MockHelper.GetMockParty();
        party.PartyRoles.Add(partyRole);
        _dbContext.Parties.Add(party);
 
        await _dbContext.SaveChangesAsync();

        var createDto = new CreateBookDto
        {
            Title = "Test Book",
            ISBN = MockHelper.ISBN,
            Description = "Test Description",
            AuthorId = MockHelper.AuthorId,
            CategoryId = MockHelper.CategoryId,
            PublishedDate = MockHelper.PublishedDate,
            NumberOfCopies = 1
        };
        var mockEventPublisher = new Mock<IEventPublisher>();   
        mockEventPublisher.Setup(m => m.PublishEvent(It.IsAny<string>(), It.IsAny<object>())).Returns(Task.CompletedTask);
        var booksService = new BooksService(_dbContext, mockEventPublisher.Object);

        // Act
        var result = await booksService.CreateBook(createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createDto.Title, result.Title);
        Assert.Equal(createDto.ISBN, result.ISBN);
        Assert.Equal(createDto.Description, result.Description);
        Assert.Equal(createDto.CategoryId, result.CategoryId);
        Assert.Equal(createDto.PublishedDate, result.PublishedDate);
        Assert.Equal(createDto.NumberOfCopies, result.TotalCopies);
        Assert.Equal(createDto.NumberOfCopies, result.AvailableCopies);
        Assert.Equal(DateTime.UtcNow.Date, result.CreatedAt.Date);
        Assert.Equal(DateTime.UtcNow.Minute, result.CreatedAt.Minute);
        mockEventPublisher.Verify(m => m.PublishEvent(
            It.Is<string>(eventName => eventName == "book.created"),
            It.Is<BookEvent>(e => e.BookId == result.Id && e.Title == result.Title && e.CategoryId == result.CategoryId)
        ), Times.Once);
    }


    [Fact]
    public async Task TestUpdateBookButBookDoesNotExist()
    {
        // Arrange
        var category = MockHelper.GetMockCategory();
        _dbContext.Categories.Add(category);
        
        var role = MockHelper.GetMockRole();
        _dbContext.Roles.Add(role);

        var partyRole = MockHelper.GetMockPartyRole();
        partyRole.Role = role;
        _dbContext.PartyRoles.Add(partyRole);

        var party = MockHelper.GetMockParty();
        party.PartyRoles.Add(partyRole);
        _dbContext.Parties.Add(party);

        await _dbContext.SaveChangesAsync();

        var updateDto = new UpdateBookDto
        {
            Title = "Updated Book",
            Description = "Updated Description",
            CategoryId = MockHelper.CategoryId,
            PublishedDate = MockHelper.PublishedDate
        };


        var booksService = new BooksService(_dbContext, new Mock<IEventPublisher>().Object);

        // Act, Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => booksService.UpdateBook(MockHelper.BookId, updateDto));
        Assert.Equal("Book not found", exception.Message);
    }

    [Fact]
    public async Task TestUpdateBookCategoryDoesNotExist()
    {
        // Arrange
        var category = MockHelper.GetMockCategory();
        _dbContext.Categories.Add(category);
        
        var role = MockHelper.GetMockRole();
        _dbContext.Roles.Add(role);

        var partyRole = MockHelper.GetMockPartyRole();
        partyRole.Role = role;
        _dbContext.PartyRoles.Add(partyRole);

        var party = MockHelper.GetMockParty();
        party.PartyRoles.Add(partyRole);
        _dbContext.Parties.Add(party);

        var book = MockHelper.GetMockBook();
        book.Category = category;
        book.Author = party;
        _dbContext.Books.Add(book);

        await _dbContext.SaveChangesAsync();
    
        var updateDto = new UpdateBookDto
        {
            Title = "Updated Book",
            Description = "Updated Description",
            CategoryId = MockHelper.CategoryId + 1,
            PublishedDate = MockHelper.PublishedDate
        };

        var booksService = new BooksService(_dbContext, new Mock<IEventPublisher>().Object);

        // Act, Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => booksService.UpdateBook(book.Id, updateDto));
        Assert.Equal("Category not found", exception.Message);
    }

    [Fact]
    public async Task TestUpdateBook()
    {
        // Arrange
        var category = MockHelper.GetMockCategory();
        _dbContext.Categories.Add(category);
        
        var role = MockHelper.GetMockRole();
        _dbContext.Roles.Add(role);

        var partyRole = MockHelper.GetMockPartyRole();
        partyRole.Role = role;
        _dbContext.PartyRoles.Add(partyRole);

        var party = MockHelper.GetMockParty();
        party.PartyRoles.Add(partyRole);
        _dbContext.Parties.Add(party);

        var book = MockHelper.GetMockBook();
        book.Category = category;
        book.Author = party;
        _dbContext.Books.Add(book);

        await _dbContext.SaveChangesAsync();

        var mockEventPublisher = new Mock<IEventPublisher>();
        mockEventPublisher.Setup(m => m.PublishEvent(It.IsAny<string>(), It.IsAny<object>())).Returns(Task.CompletedTask);
        var booksService = new BooksService(_dbContext, mockEventPublisher.Object);
        var updateDto = new UpdateBookDto
        {
            Title = "Updated Book",
            Description = "Updated Description",
            CategoryId = MockHelper.CategoryId,
            PublishedDate = MockHelper.PublishedDate
        };

        // Act
        BookDto result = await booksService.UpdateBook(book.Id, updateDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(updateDto.Title, result.Title);
        Assert.Equal(updateDto.Description, result.Description);
        Assert.Equal(updateDto.CategoryId, result.CategoryId);
        Assert.Equal(updateDto.PublishedDate, result.PublishedDate);
        Assert.Equal(book.Copies.Count, result.TotalCopies);
        Assert.Equal(book.Copies.Count(c => c.IsAvailable), result.AvailableCopies);
        Assert.Equal(DateTime.UtcNow.Date, result.CreatedAt.Date);
        Assert.Equal(DateTime.UtcNow.Minute, result.CreatedAt.Minute);
        mockEventPublisher.Verify(m => m.PublishEvent(
            It.Is<string>(eventName => eventName == "book.updated"),
            It.Is<BookEvent>(e => e.BookId == book.Id && e.Title == updateDto.Title && e.CategoryId == updateDto.CategoryId)
        ), Times.Once);
    }

    [Fact]
    public async Task TestDeleteBook()
    {
        // Arrange
        var book = MockHelper.GetMockBook();
        _dbContext.Books.Add(book);
        await _dbContext.SaveChangesAsync();

        var mockEventPublisher = new Mock<IEventPublisher>();
        mockEventPublisher.Setup(m => m.PublishEvent(It.IsAny<string>(), It.IsAny<object>())).Returns(Task.CompletedTask);
        var booksService = new BooksService(_dbContext, mockEventPublisher.Object);

        // Act
        await booksService.DeleteBook(book.Id);

        // Assert
        Assert.Null(await _dbContext.Books.FindAsync(book.Id));
        mockEventPublisher.Verify(m => m.PublishEvent(
            It.Is<string>(eventName => eventName == "book.deleted"),
            It.Is<BookEvent>(e => e.BookId == book.Id && e.Title == book.Title && e.CategoryId == book.CategoryId)
        ), Times.Once);
    }

    [Fact]
    public async Task TestDeleteBookWithBorrowedCopies()
    {
        // Arrange
        var book = MockHelper.GetMockBook();
        book.Copies.Add(MockHelper.GetMockBookCopy());
        book.Copies.First().IsAvailable = false;
        _dbContext.Books.Add(book);
        await _dbContext.SaveChangesAsync();

        var booksService = new BooksService(_dbContext, new Mock<IEventPublisher>().Object);

        // Act, Assert
        var exception = await Assert.ThrowsAsync<ApiException>(() => booksService.DeleteBook(book.Id));
        Assert.Equal("Cannot delete book with borrowed copies", exception.Message);
    }
}
