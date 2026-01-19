using LibraryCoreApi.Database;
using LibraryCoreApi.Services.Reservations;
using Xunit;
using Microsoft.EntityFrameworkCore;
using LibraryCoreApi.Entities;
using LibraryCoreApi.Events;
using Moq;
using LibraryCoreApi.DTOs;
using LibraryCoreApi.Errors;

namespace LibraryCoreApi.Tests.Services;

public class ReservationsServiceTests : IDisposable
{
    private readonly DataContext _dbContext;
    public ReservationsServiceTests()
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
    public async Task TestGetReservations()
    {
        // Arrange

        var bookCopy = MockHelper.GetMockBookCopy();
        bookCopy.Book = MockHelper.GetMockBook();
        _dbContext.BookCopies.Add(bookCopy);

        var customer = MockHelper.GetMockParty();
        _dbContext.Parties.Add(customer);

        var reservation = MockHelper.GetMockReservation();
        reservation.BookCopy = bookCopy;
        reservation.Customer = customer;

        _dbContext.Reservations.Add(reservation);
        await _dbContext.SaveChangesAsync();
        

        var reservationsService = new ReservationsService(_dbContext, new Mock<IEventPublisher>().Object);

        // Act
        var reservationsDto = await reservationsService.GetReservations();

        // Assert
        Assert.Single(reservationsDto);
        Assert.Equal(reservation.Id, reservationsDto.First().Id);
        Assert.Equal(reservation.BookCopyId, reservationsDto.First().BookCopyId);
        Assert.Equal(reservation.BookCopy.Book.Title, reservationsDto.First().BookTitle);
        Assert.Equal(reservation.BookCopy.CopyNumber, reservationsDto.First().CopyNumber);
        Assert.Equal(reservation.CustomerId, reservationsDto.First().CustomerId);
        Assert.Equal(reservation.Customer.Name, reservationsDto.First().CustomerName);
        Assert.Equal(reservation.ReservedAt, reservationsDto.First().ReservedAt);
        Assert.Equal(reservation.BorrowedAt, reservationsDto.First().BorrowedAt);
        Assert.Equal(reservation.ReturnedAt, reservationsDto.First().ReturnedAt);
        Assert.Equal(reservation.DueDate, reservationsDto.First().DueDate);
        Assert.Equal(reservation.Status.ToString(), reservationsDto.First().Status);
    }

    [Fact]
    public async Task TestGetReservationByReservationId()
    {
        // Arrange
        var bookCopy = MockHelper.GetMockBookCopy();
        bookCopy.Book = MockHelper.GetMockBook();
        _dbContext.BookCopies.Add(bookCopy);

        var customer = MockHelper.GetMockParty();
        _dbContext.Parties.Add(customer);

        var reservation = MockHelper.GetMockReservation();
        reservation.BookCopy = bookCopy;
        reservation.Customer = customer;
        _dbContext.Reservations.Add(reservation);
        await _dbContext.SaveChangesAsync();
        
        var reservationsService = new ReservationsService(_dbContext, new Mock<IEventPublisher>().Object);

        // Act
        var reservationDto = await reservationsService.GetReservation(reservation.Id);

        // Assert
        Assert.NotNull(reservationDto);
        Assert.Equal(reservation.Id, reservationDto.Id);
        Assert.Equal(reservation.BookCopyId, reservationDto.BookCopyId);
        Assert.Equal(reservation.BookCopy.Book.Title, reservationDto.BookTitle);
        Assert.Equal(reservation.BookCopy.CopyNumber, reservationDto.CopyNumber);
        Assert.Equal(reservation.CustomerId, reservationDto.CustomerId);
        Assert.Equal(reservation.Customer.Name, reservationDto.CustomerName);
        Assert.Equal(reservation.ReservedAt, reservationDto.ReservedAt);
        Assert.Equal(reservation.BorrowedAt, reservationDto.BorrowedAt);
        Assert.Equal(reservation.ReturnedAt, reservationDto.ReturnedAt);
        Assert.Equal(reservation.DueDate, reservationDto.DueDate);
        Assert.Equal(reservation.Status.ToString(), reservationDto.Status);
    }

    [Fact]
    public async Task TestGetReservationByReservationIdDoesNotExist()
    {
        // Arrange
         var bookCopy = MockHelper.GetMockBookCopy();
        bookCopy.Book = MockHelper.GetMockBook();
        _dbContext.BookCopies.Add(bookCopy);

        var customer = MockHelper.GetMockParty();
        _dbContext.Parties.Add(customer);

        var reservation = MockHelper.GetMockReservation();
        reservation.BookCopy = bookCopy;
        reservation.Customer = customer;
        _dbContext.Reservations.Add(reservation);
        await _dbContext.SaveChangesAsync();

        var reservationsService = new ReservationsService(_dbContext, new Mock<IEventPublisher>().Object);

        // Act, Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => reservationsService.GetReservation(MockHelper.ReservationId + 1));
    }

    [Fact]
    public async Task TestCreateReservationCustomerDoesNotExist()
    {
        // Arrange
        var bookCopy = MockHelper.GetMockBookCopy();
        bookCopy.Book = MockHelper.GetMockBook();
        _dbContext.BookCopies.Add(bookCopy);

        var role = MockHelper.GetMockRole();
        _dbContext.Roles.Add(role);

        var partyRole = MockHelper.GetMockPartyRole();
        partyRole.Role = role;
        _dbContext.PartyRoles.Add(partyRole);

        var customer = MockHelper.GetMockParty();
        customer.PartyRoles.Add(partyRole);
        _dbContext.Parties.Add(customer);

        await _dbContext.SaveChangesAsync();

        var reservationsService = new ReservationsService(_dbContext, new Mock<IEventPublisher>().Object);
        
        // Act
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => reservationsService.CreateReservation(
            new CreateReservationDto { BookId = bookCopy.BookId, CustomerId = customer.Id + 1 }));
        Assert.Equal("Customer not found", exception.Message);
    }

    [Fact]
    public async Task TestCreateReservationPartyDoesNotHaveCustomerRole()
    {
        // Arrange
        var bookCopy = MockHelper.GetMockBookCopy();
        bookCopy.Book = MockHelper.GetMockBook();
        _dbContext.BookCopies.Add(bookCopy);

        var role = MockHelper.GetMockRole();
        _dbContext.Roles.Add(role);

        var partyRole = MockHelper.GetMockPartyRole();
        partyRole.Role = role;
        _dbContext.PartyRoles.Add(partyRole);

        var customer = MockHelper.GetMockParty();
        customer.PartyRoles.Add(partyRole);
        _dbContext.Parties.Add(customer);

        await _dbContext.SaveChangesAsync();

        var reservationsService = new ReservationsService(_dbContext, new Mock<IEventPublisher>().Object);
        
        // Act, Assert
        var exception = await Assert.ThrowsAsync<ApiException>(() => reservationsService.CreateReservation(
            new CreateReservationDto { BookId = bookCopy.BookId, CustomerId = customer.Id }));
        Assert.Equal("Party does not have Customer role", exception.Message);
    }

    [Fact]
    public async Task TestCreateReservationNoAvailableCopies()
    {
         // Arrange
        var bookCopy = MockHelper.GetMockBookCopy();
        bookCopy.IsAvailable = false;
        bookCopy.Book = MockHelper.GetMockBook();
        _dbContext.BookCopies.Add(bookCopy);

        var role = MockHelper.GetMockRole();
        role.Name = MockHelper.CustomerRoleName;
        _dbContext.Roles.Add(role);

        var partyRole = MockHelper.GetMockPartyRole();
        partyRole.Role = role;
        _dbContext.PartyRoles.Add(partyRole);

        var customer = MockHelper.GetMockParty();
        customer.PartyRoles.Add(partyRole);
        _dbContext.Parties.Add(customer);

        await _dbContext.SaveChangesAsync();

        var reservationsService = new ReservationsService(_dbContext, new Mock<IEventPublisher>().Object);
        
        // Act, Assert
        var exception = await Assert.ThrowsAsync<ApiException>(() => reservationsService.CreateReservation(
            new CreateReservationDto { BookId = bookCopy.BookId, CustomerId = customer.Id }));
        Assert.Equal("No available copies of this book", exception.Message);
    }

    [Fact]
    public async Task TestCreateReservationSuccessfully()
    {
        // Arrange
        var bookCopy = MockHelper.GetMockBookCopy();
        bookCopy.Book = MockHelper.GetMockBook();
        _dbContext.BookCopies.Add(bookCopy);
    
        var role = MockHelper.GetMockRole();
        role.Name = MockHelper.CustomerRoleName;
        _dbContext.Roles.Add(role);

        var partyRole = MockHelper.GetMockPartyRole();
        partyRole.Role = role;
        _dbContext.PartyRoles.Add(partyRole);

        var customer = MockHelper.GetMockParty();
        customer.PartyRoles.Add(partyRole);
        _dbContext.Parties.Add(customer);

        await _dbContext.SaveChangesAsync();

        var mockEventPublisher = new Mock<IEventPublisher>();
        mockEventPublisher.Setup(m => m.PublishEvent(It.IsAny<string>(), It.IsAny<object>())).Returns(Task.CompletedTask);
        var reservationsService = new ReservationsService(_dbContext, mockEventPublisher.Object);
        
        // Act
        var reservationDto = await reservationsService.CreateReservation(new CreateReservationDto { BookId = bookCopy.BookId, CustomerId = customer.Id });
       
        // Assert
        Assert.NotNull(reservationDto);
        Assert.Equal(bookCopy.BookId, reservationDto.BookCopyId);
        Assert.Equal(bookCopy.Book.Title, reservationDto.BookTitle);
        Assert.Equal(bookCopy.CopyNumber, reservationDto.CopyNumber);
        Assert.Equal(customer.Id, reservationDto.CustomerId);
        Assert.Equal(customer.Name, reservationDto.CustomerName);
        Assert.Equal(DateTime.UtcNow.Date, reservationDto.ReservedAt.Date);
        Assert.Equal(DateTime.UtcNow.Minute, reservationDto.ReservedAt.Minute);
        Assert.Equal(ReservationStatus.Reserved.ToString(), reservationDto.Status);

        mockEventPublisher.Verify(m => m.PublishEvent(
            It.Is<string>(eventName => eventName == "reservation.created"),
            It.Is<ReservationEvent>(e => e.ReservationId == reservationDto.Id && e.BookCopyId == bookCopy.Id && e.BookTitle == bookCopy.Book.Title && e.CopyNumber == bookCopy.CopyNumber && e.CustomerId == customer.Id)
        ), Times.Once);
    }

    [Fact]
    public async Task TestBorrowBookReservationDoesNotExist()
    {
        // Arrange
        var reservationsService = new ReservationsService(_dbContext, new Mock<IEventPublisher>().Object);
        
        // Act, Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => reservationsService.BorrowBook(new BorrowBookDto { ReservationId = MockHelper.ReservationId + 1 }));
        Assert.Equal("Reservation not found", exception.Message);
    }

    [Fact]
    public async Task TestBorrowBookReservationNotReserved()
    {
        // Arrange
        var reservation = MockHelper.GetMockReservation();
        reservation.Status = ReservationStatus.Borrowed;
        _dbContext.Reservations.Add(reservation);
        await _dbContext.SaveChangesAsync();

        var reservationsService = new ReservationsService(_dbContext, new Mock<IEventPublisher>().Object);
        
        // Act, Assert
        var exception = await Assert.ThrowsAsync<ApiException>(() => reservationsService.BorrowBook(new BorrowBookDto { ReservationId = reservation.Id }));
        Assert.Equal("Only reserved books can be borrowed", exception.Message);
    }

    [Fact]
    public async Task TestBorrowBookSuccessfully()
    {
        // Arrange
        var role = MockHelper.GetMockRole();
        role.Name = MockHelper.CustomerRoleName;
        _dbContext.Roles.Add(role);

        var partyRole = MockHelper.GetMockPartyRole();
        partyRole.Role = role;
        _dbContext.PartyRoles.Add(partyRole);

        var customer = MockHelper.GetMockParty();
        customer.PartyRoles.Add(partyRole);
        _dbContext.Parties.Add(customer);

        var bookCopy = MockHelper.GetMockBookCopy();
        bookCopy.Book = MockHelper.GetMockBook();
        _dbContext.BookCopies.Add(bookCopy);

        var reservation = MockHelper.GetMockReservation();
        reservation.Status = ReservationStatus.Reserved;
        reservation.BookCopy = bookCopy;
        reservation.Customer = customer;
        _dbContext.Reservations.Add(reservation);
        await _dbContext.SaveChangesAsync();

        var mockEventPublisher = new Mock<IEventPublisher>();
        mockEventPublisher.Setup(m => m.PublishEvent(It.IsAny<string>(), It.IsAny<object>())).Returns(Task.CompletedTask);
        var reservationsService = new ReservationsService(_dbContext, mockEventPublisher.Object);
        
        // Act
        var reservationDto = await reservationsService.BorrowBook(new BorrowBookDto { ReservationId = reservation.Id });
        
        // Assert
        Assert.NotNull(reservationDto);
        Assert.Equal(reservation.Id, reservationDto.Id);
        Assert.Equal(reservation.BookCopyId, reservationDto.BookCopyId);
        Assert.Equal(reservation.BookCopy.Book.Title, reservationDto.BookTitle);
        Assert.Equal(reservation.BookCopy.CopyNumber, reservationDto.CopyNumber);
        Assert.Equal(reservation.CustomerId, reservationDto.CustomerId);
        Assert.Equal(reservation.Customer.Name, reservationDto.CustomerName);
        Assert.Equal(DateTime.UtcNow.Date, reservationDto.BorrowedAt?.Date);
        Assert.Equal(DateTime.UtcNow.Minute, reservationDto.BorrowedAt?.Minute);
        Assert.Equal(ReservationStatus.Borrowed.ToString(), reservationDto.Status);

        mockEventPublisher.Verify(m => m.PublishEvent(
            It.Is<string>(eventName => eventName == "reservation.borrowed"),
            It.Is<ReservationEvent>(e => e.ReservationId == reservationDto.Id && e.BookCopyId == reservation.BookCopyId && e.BookTitle == reservation.BookCopy.Book.Title && e.CopyNumber == reservation.BookCopy.CopyNumber && e.CustomerId == reservation.CustomerId)
        ), Times.Once);
    }

    [Fact]
    public async Task TestReturnBookReservationDoesNotExist()
    {
        // Arrange
        var reservationsService = new ReservationsService(_dbContext, new Mock<IEventPublisher>().Object);
        
        // Act, Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => reservationsService.ReturnBook(new ReturnBookDto { ReservationId = MockHelper.ReservationId + 1 }));
        Assert.Equal("Reservation not found", exception.Message);
    }

    [Fact]
    public async Task TestReturnBookReservationNotBorrowed()
    {
        // Arrange
        var reservation = MockHelper.GetMockReservation();
        reservation.Status = ReservationStatus.Reserved;
        _dbContext.Reservations.Add(reservation);
        await _dbContext.SaveChangesAsync();

        var reservationsService = new ReservationsService(_dbContext, new Mock<IEventPublisher>().Object);
        
        // Act, Assert
        var exception = await Assert.ThrowsAsync<ApiException>(() => reservationsService.ReturnBook(new ReturnBookDto { ReservationId = reservation.Id }));
        Assert.Equal("Only borrowed books can be returned", exception.Message);
    }

    [Fact] 
    public async Task TestReturnBookSuccessfully()
    {
        // Arrange
        var role = MockHelper.GetMockRole();
        role.Name = MockHelper.CustomerRoleName;
        _dbContext.Roles.Add(role);

        var partyRole = MockHelper.GetMockPartyRole();
        partyRole.Role = role;
        _dbContext.PartyRoles.Add(partyRole);

        var customer = MockHelper.GetMockParty();
        customer.PartyRoles.Add(partyRole);
        _dbContext.Parties.Add(customer);

        var bookCopy = MockHelper.GetMockBookCopy();
        bookCopy.Book = MockHelper.GetMockBook();
        _dbContext.BookCopies.Add(bookCopy);

        var reservation = MockHelper.GetMockReservation();
        reservation.Status = ReservationStatus.Borrowed;
        bookCopy.CurrentReservation = reservation;
        reservation.BookCopy = bookCopy;
        reservation.Customer = customer;
        _dbContext.Reservations.Add(reservation);

        await _dbContext.SaveChangesAsync();

        var mockEventPublisher = new Mock<IEventPublisher>();
        mockEventPublisher.Setup(m => m.PublishEvent(It.IsAny<string>(), It.IsAny<object>())).Returns(Task.CompletedTask);
        var reservationsService = new ReservationsService(_dbContext, mockEventPublisher.Object);
        
        // Act
        var reservationDto = await reservationsService.ReturnBook(new ReturnBookDto { ReservationId = reservation.Id });
        
        // Assert
        Assert.NotNull(reservationDto);
        Assert.Equal(reservation.Id, reservationDto.Id);
        Assert.Equal(reservation.BookCopyId, reservationDto.BookCopyId);
        Assert.Equal(reservation.BookCopy.Book.Title, reservationDto.BookTitle);
        Assert.Equal(reservation.BookCopy.CopyNumber, reservationDto.CopyNumber);
        Assert.Equal(reservation.CustomerId, reservationDto.CustomerId);
        Assert.Equal(reservation.Customer.Name, reservationDto.CustomerName);
        Assert.Equal(DateTime.UtcNow.Date, reservationDto.ReturnedAt?.Date);
        Assert.Equal(DateTime.UtcNow.Minute, reservationDto.ReturnedAt?.Minute);
        Assert.Equal(ReservationStatus.Returned.ToString(), reservationDto.Status);

        mockEventPublisher.Verify(m => m.PublishEvent(
            It.Is<string>(eventName => eventName == "reservation.returned"),
            It.Is<ReservationEvent>(e => e.ReservationId == reservationDto.Id && e.BookCopyId == reservation.BookCopyId && e.BookTitle == reservation.BookCopy.Book.Title && e.CopyNumber == reservation.BookCopy.CopyNumber && e.CustomerId == reservation.CustomerId)
        ), Times.Once);
    }

    [Fact]
    public async Task TestGetBorrowingVisibility()
    {
        // Arrange

        var bookCopy = MockHelper.GetMockBookCopy();
        bookCopy.Book = MockHelper.GetMockBook();
        _dbContext.BookCopies.Add(bookCopy);

        var role = MockHelper.GetMockRole();
        role.Name = MockHelper.CustomerRoleName;
        _dbContext.Roles.Add(role);

        var partyRole = MockHelper.GetMockPartyRole();
        partyRole.Role = role;
        _dbContext.PartyRoles.Add(partyRole);

        var customer = MockHelper.GetMockParty();
        customer.PartyRoles.Add(partyRole);
        _dbContext.Parties.Add(customer);

        var reservation = MockHelper.GetMockReservation();
        reservation.BookCopy = bookCopy;
        reservation.Customer = customer;
        reservation.Status = ReservationStatus.Borrowed;
        _dbContext.Reservations.Add(reservation);
        await _dbContext.SaveChangesAsync();

        var reservationsService = new ReservationsService(_dbContext, new Mock<IEventPublisher>().Object);
        
        // Act
        var borrowingVisibilityDto = await reservationsService.GetBorrowingVisibility();

        // Assert
        Assert.NotNull(borrowingVisibilityDto);
        Assert.Single(borrowingVisibilityDto);
        Assert.Equal(bookCopy.Book.Id, borrowingVisibilityDto.First().BookId);
        Assert.Equal(bookCopy.Book.Title, borrowingVisibilityDto.First().BookTitle);
        Assert.Single(borrowingVisibilityDto.First().CurrentBorrowers);
        Assert.Equal(customer.Id, borrowingVisibilityDto.First().CurrentBorrowers.First().CustomerId);
        Assert.Equal(customer.Name, borrowingVisibilityDto.First().CurrentBorrowers.First().CustomerName);
        Assert.Equal(customer.Email, borrowingVisibilityDto.First().CurrentBorrowers.First().CustomerEmail);
        Assert.Equal(bookCopy.CopyNumber, borrowingVisibilityDto.First().CurrentBorrowers.First().CopyNumber);
        Assert.Equal(reservation.BorrowedAt, borrowingVisibilityDto.First().CurrentBorrowers.First().BorrowedAt);
        Assert.Equal(reservation.DueDate, borrowingVisibilityDto.First().CurrentBorrowers.First().DueDate);
    }
}
