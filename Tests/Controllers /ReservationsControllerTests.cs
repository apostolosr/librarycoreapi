using Moq;
using LibraryCoreApi.Controllers;
using LibraryCoreApi.Services.Reservations;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using LibraryCoreApi.DTOs;

namespace LibraryCoreApi.Tests.Controllers;

public class ReservationsControllerTests
{
    private readonly Mock<IReservationsService> _mockReservationsService;
    private readonly ReservationsController _reservationsController;

    public ReservationsControllerTests()
    {
        _mockReservationsService = new Mock<IReservationsService>();
        _reservationsController = new ReservationsController(_mockReservationsService.Object);
    }

    [Fact]
    public async Task GetReservations_ReturnsOkResult_WithListOfReservations()
    {
        // Arrange
        var expectedReservations = new List<ReservationDto> 
        { 
            MockHelper.GetMockReservationDto()
        };
        _mockReservationsService.Setup(s => s.GetReservations()).ReturnsAsync(expectedReservations);

        // Act
        var result = await _reservationsController.GetReservations();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var reservations = Assert.IsAssignableFrom<IEnumerable<ReservationDto>>(okResult.Value);
        Assert.Single(reservations);
        Assert.Equal(200, okResult.StatusCode);
        _mockReservationsService.Verify(s => s.GetReservations(), Times.Once);
    }

    [Fact]
    public async Task GetReservations_ReturnsOkResult_WithEmptyList()
    {
        // Arrange
        var emptyList = new List<ReservationDto>();
        _mockReservationsService.Setup(s => s.GetReservations()).ReturnsAsync(emptyList);

        // Act
        var result = await _reservationsController.GetReservations();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var reservations = Assert.IsAssignableFrom<IEnumerable<ReservationDto>>(okResult.Value);
        Assert.Empty(reservations);
        _mockReservationsService.Verify(s => s.GetReservations(), Times.Once);
    }

    [Fact]
    public async Task GetReservation_ReturnsOkResult_WhenReservationExists()
    {
        // Arrange
        var expectedReservation = MockHelper.GetMockReservationDto();
        _mockReservationsService.Setup(s => s.GetReservation(It.IsAny<int>())).ReturnsAsync(expectedReservation);

        // Act
        var result = await _reservationsController.GetReservation(MockHelper.ReservationId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var reservation = Assert.IsType<ReservationDto>(okResult.Value);
        Assert.Equal(MockHelper.ReservationId, reservation.Id);
        Assert.Equal(MockHelper.BookTitle, reservation.BookTitle);
        Assert.Equal(MockHelper.CopyNumber, reservation.CopyNumber);
        Assert.Equal(MockHelper.CustomerId, reservation.CustomerId);
        Assert.Equal(MockHelper.CustomerName, reservation.CustomerName);
        Assert.Equal(MockHelper.ReservedAt, reservation.ReservedAt);
        Assert.Equal(MockHelper.BorrowedAt, reservation.BorrowedAt);
        Assert.Equal(MockHelper.ReturnedAt, reservation.ReturnedAt);
        Assert.Equal(MockHelper.DueDate, reservation.DueDate);
        Assert.Equal(MockHelper.Status, reservation.Status);
        _mockReservationsService.Verify(s => s.GetReservation(MockHelper.ReservationId), Times.Once);
    }

    [Fact]
    public async Task CreateReservation_ReturnsCreatedAtAction_WhenReservationIsCreated()
    {
        // Arrange
        var createDto = new CreateReservationDto
        {
            BookId = MockHelper.BookCopyId,
            CustomerId = MockHelper.CustomerId
        };
        _mockReservationsService.Setup(s => s.CreateReservation(It.IsAny<CreateReservationDto>())).ReturnsAsync(MockHelper.GetMockReservationDto());

        // Act
        var result = await _reservationsController.CreateReservation(createDto);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var reservation = Assert.IsType<ReservationDto>(createdAtActionResult.Value);
        Assert.Equal(MockHelper.ReservationId, reservation.Id);
        Assert.Equal(MockHelper.BookTitle, reservation.BookTitle);
        Assert.Equal(MockHelper.CopyNumber, reservation.CopyNumber);
        Assert.Equal(MockHelper.CustomerId, reservation.CustomerId);
        Assert.Equal(MockHelper.CustomerName, reservation.CustomerName);
        Assert.Equal(MockHelper.ReservedAt, reservation.ReservedAt);
        Assert.Equal(MockHelper.BorrowedAt, reservation.BorrowedAt);
        Assert.Equal(MockHelper.ReturnedAt, reservation.ReturnedAt);
        Assert.Equal(MockHelper.DueDate, reservation.DueDate);
        Assert.Equal(MockHelper.Status, reservation.Status);
        Assert.Equal(201, createdAtActionResult.StatusCode);
        Assert.Equal(nameof(ReservationsController.GetReservation), createdAtActionResult.ActionName);
        _mockReservationsService.Verify(s => s.CreateReservation(It.Is<CreateReservationDto>(d => d.BookId == MockHelper.BookId && d.CustomerId == MockHelper.CustomerId)), Times.Once);
    }


    [Fact]
    public async Task BorrowBook_ReturnsOkResult_WhenBookIsBorrowed()
    {   
        // Arrange
        var borrowDto = new BorrowBookDto
        {
            ReservationId = MockHelper.ReservationId,
            DueDate = MockHelper.DueDate
        };
        _mockReservationsService.Setup(s => s.BorrowBook(It.IsAny<BorrowBookDto>())).ReturnsAsync(MockHelper.GetMockReservationDto());

        // Act
        var result = await _reservationsController.BorrowBook(borrowDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var reservation = Assert.IsType<ReservationDto>(okResult.Value);
        Assert.Equal(MockHelper.ReservationId, reservation.Id);
        Assert.Equal(MockHelper.BookTitle, reservation.BookTitle);
        Assert.Equal(MockHelper.CopyNumber, reservation.CopyNumber);
        Assert.Equal(MockHelper.CustomerId, reservation.CustomerId);
        Assert.Equal(MockHelper.CustomerName, reservation.CustomerName);
        Assert.Equal(MockHelper.ReservedAt, reservation.ReservedAt);
        Assert.Equal(MockHelper.BorrowedAt, reservation.BorrowedAt);
        Assert.Equal(MockHelper.ReturnedAt, reservation.ReturnedAt);
        Assert.Equal(MockHelper.DueDate, reservation.DueDate);
        Assert.Equal(MockHelper.Status, reservation.Status);
        _mockReservationsService.Verify(s => s.BorrowBook(It.IsAny<BorrowBookDto>()), Times.Once);
    }

    [Fact]
    public async Task ReturnBook_ReturnsOkResult_WhenBookIsReturned()
    {
        // Arrange
        _mockReservationsService.Setup(s => s.ReturnBook(It.IsAny<ReturnBookDto>())).ReturnsAsync(MockHelper.GetMockReservationDto());
        var returnDto = new ReturnBookDto
        {
            ReservationId = MockHelper.ReservationId
        };
        // Act
        var result = await _reservationsController.ReturnBook(returnDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var reservation = Assert.IsType<ReservationDto>(okResult.Value);
        Assert.Equal(MockHelper.ReservationId, reservation.Id);
        Assert.Equal(MockHelper.BookTitle, reservation.BookTitle);
        Assert.Equal(MockHelper.CopyNumber, reservation.CopyNumber);
        Assert.Equal(MockHelper.CustomerId, reservation.CustomerId);
        Assert.Equal(MockHelper.CustomerName, reservation.CustomerName);
        Assert.Equal(MockHelper.ReservedAt, reservation.ReservedAt);
        Assert.Equal(MockHelper.BorrowedAt, reservation.BorrowedAt);
        Assert.Equal(MockHelper.ReturnedAt, reservation.ReturnedAt);
        Assert.Equal(MockHelper.DueDate, reservation.DueDate);
        Assert.Equal(MockHelper.Status, reservation.Status);
        Assert.Equal(200, okResult.StatusCode);
        _mockReservationsService.Verify(s => s.ReturnBook(It.IsAny<ReturnBookDto>()), Times.Once);
    }

    [Fact]
    public async Task GetBorrowingVisibility_ReturnsOkResult_WithListOfBorrowingVisibility()
    {
        // Arrange
        var expectedBorrowingVisibility = new List<BorrowingVisibilityDto> 
        { 
            MockHelper.GetMockBorrowingVisibilityDto()
        };
        _mockReservationsService.Setup(s => s.GetBorrowingVisibility()).ReturnsAsync(expectedBorrowingVisibility);

        // Act
        var result = await _reservationsController.GetBorrowingVisibility();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var borrowingVisibility = Assert.IsAssignableFrom<IEnumerable<BorrowingVisibilityDto>>(okResult.Value);
        Assert.Single(borrowingVisibility);
        Assert.Equal(200, okResult.StatusCode);
        _mockReservationsService.Verify(s => s.GetBorrowingVisibility(), Times.Once);
    }

    [Fact]
    public async Task GetBorrowingVisibility_ReturnsOkResult_WithEmptyList()
    {
        // Arrange
        var emptyList = new List<BorrowingVisibilityDto>();
        _mockReservationsService.Setup(s => s.GetBorrowingVisibility()).ReturnsAsync(emptyList);

        // Act
        var result = await _reservationsController.GetBorrowingVisibility();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var borrowingVisibility = Assert.IsAssignableFrom<IEnumerable<BorrowingVisibilityDto>>(okResult.Value);
        Assert.Empty(borrowingVisibility);
        Assert.Equal(200, okResult.StatusCode);
        _mockReservationsService.Verify(s => s.GetBorrowingVisibility(), Times.Once);
    }

    [Fact]
    public async Task GetBorrowingVisibility_ReturnsOkResult_WithListOfBorrowingVisibility_WhenThereAreBorrowedBooks()
    {
        // Arrange
        var expectedBorrowingVisibility = new List<BorrowingVisibilityDto> 
        { 
            MockHelper.GetMockBorrowingVisibilityDto()
        };
        _mockReservationsService.Setup(s => s.GetBorrowingVisibility()).ReturnsAsync(expectedBorrowingVisibility);

        // Act
        var result = await _reservationsController.GetBorrowingVisibility();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var borrowingVisibility = Assert.IsAssignableFrom<IEnumerable<BorrowingVisibilityDto>>(okResult.Value);
        Assert.Single(borrowingVisibility);
        Assert.Equal(200, okResult.StatusCode);
        _mockReservationsService.Verify(s => s.GetBorrowingVisibility(), Times.Once);
    }

    [Fact]
    public async Task GetBorrowingVisibility_ReturnsOkResult_WithEmptyList_WhenThereAreNoBorrowedBooks()
    {
        // Arrange
        var emptyList = new List<BorrowingVisibilityDto>();
        _mockReservationsService.Setup(s => s.GetBorrowingVisibility()).ReturnsAsync(emptyList);

        // Act
        var result = await _reservationsController.GetBorrowingVisibility();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var borrowingVisibility = Assert.IsAssignableFrom<IEnumerable<BorrowingVisibilityDto>>(okResult.Value);
        Assert.Empty(borrowingVisibility);
        Assert.Equal(200, okResult.StatusCode);
        _mockReservationsService.Verify(s => s.GetBorrowingVisibility(), Times.Once);
    }
}