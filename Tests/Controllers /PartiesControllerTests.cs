using Moq;
using LibraryCoreApi.Controllers;
using LibraryCoreApi.Services.Parties;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using LibraryCoreApi.DTOs;
using LibraryCoreApi.Errors;

namespace LibraryCoreApi.Tests.Controllers;

public class PartiesControllerTests
{
    private readonly Mock<IPartiesService> _mockPartiesService;
    private readonly PartiesController _partiesController;

    public PartiesControllerTests()
    {
        _mockPartiesService = new Mock<IPartiesService>();
        _partiesController = new PartiesController(_mockPartiesService.Object);
    }

    [Fact]
    public async Task GetParties_ReturnsOkResult_WithListOfParties()
    {
        // Arrange
        var expectedParties = new List<PartyDto> 
        { 
            MockHelper.GetMockPartyDto()
        };
        _mockPartiesService.Setup(s => s.GetParties()).ReturnsAsync(expectedParties);

        // Act
        var result = await _partiesController.GetParties();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var parties = Assert.IsAssignableFrom<IEnumerable<PartyDto>>(okResult.Value);
        Assert.Single(parties);
        Assert.Equal(200, okResult.StatusCode);
        _mockPartiesService.Verify(s => s.GetParties(), Times.Once);
    }

    [Fact]
    public async Task GetBooks_ReturnsOkResult_WithEmptyList()
    {
        // Arrange
        var emptyList = new List<PartyDto>();
        _mockPartiesService.Setup(s => s.GetParties()).ReturnsAsync(emptyList);

        // Act
        var result = await _partiesController.GetParties();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var parties = Assert.IsAssignableFrom<IEnumerable<PartyDto>>(okResult.Value);
        Assert.Empty(parties);
        _mockPartiesService.Verify(s => s.GetParties(), Times.Once);
    }

    [Fact]
    public async Task GetParty_ReturnsOkResult_WhenPartyExists()
    {
        // Arrange
        var expectedParty = MockHelper.GetMockPartyDto();
        _mockPartiesService.Setup(s => s.GetParty(It.IsAny<int>())).ReturnsAsync(expectedParty);

        // Act
        var result = await _partiesController.GetParty(MockHelper.PartyId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var party = Assert.IsType<PartyDto>(okResult.Value);
        Assert.Equal(MockHelper.PartyId, party.Id);
        Assert.Equal(MockHelper.Name, party.Name);
        Assert.Equal(200, okResult.StatusCode);
        _mockPartiesService.Verify(s => s.GetParty(MockHelper.PartyId), Times.Once);
    }

    [Fact]
    public async Task CreateParty_ReturnsCreatedAtAction_WhenPartyIsCreated()
    {
        // Arrange
        var createDto = new CreatePartyDto
        {
            Name = MockHelper.Name,
            Email = MockHelper.Email,
            Phone = MockHelper.Phone,
            Address = MockHelper.Address,
            RoleIds = new List<int> { 1, 2 }
        };
        _mockPartiesService.Setup(s => s.CreateParty(It.IsAny<CreatePartyDto>())).ReturnsAsync(MockHelper.GetMockPartyDto());

        // Act
        var result = await _partiesController.CreateParty(createDto);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var party = Assert.IsType<PartyDto>(createdAtActionResult.Value);
        Assert.Equal(MockHelper.PartyId, party.Id);
        Assert.Equal(MockHelper.Name, party.Name);
        Assert.Equal(201, createdAtActionResult.StatusCode);
        Assert.Equal(nameof(PartiesController.GetParty), createdAtActionResult.ActionName);
        _mockPartiesService.Verify(s => s.CreateParty(It.Is<CreatePartyDto>(d => d.Name == MockHelper.Name)), Times.Once);
    }


    [Fact]
    public async Task UpdateParty_ReturnsOkResult_WhenPartyIsUpdated()
    {   
        // Arrange
        var updateDto = new UpdatePartyDto
        {
            Name = "Updated Name",
            Email = MockHelper.Email,
            Phone = MockHelper.Phone,
            Address = MockHelper.Address,
            RoleIds = new List<int> { 1, 2 }
        };
        _mockPartiesService.Setup(s => s.UpdateParty(It.IsAny<int>(), It.IsAny<UpdatePartyDto>())).ReturnsAsync(MockHelper.GetMockPartyDto());

        // Act
        var result = await _partiesController.UpdateParty(MockHelper.PartyId, updateDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var party = Assert.IsType<PartyDto>(okResult.Value);
        Assert.Equal(MockHelper.PartyId, party.Id);
        Assert.Equal(MockHelper.Name, party.Name);
        Assert.Equal(200, okResult.StatusCode);
        _mockPartiesService.Verify(s => s.UpdateParty(MockHelper.PartyId, It.IsAny<UpdatePartyDto>()), Times.Once);
    }

    [Fact]
    public async Task DeleteParty_ReturnsNoContent_WhenPartyIsDeleted()
    {
        // Arrange
        _mockPartiesService.Setup(s => s.DeleteParty(It.IsAny<int>()));

        // Act
        var result = await _partiesController.DeleteParty(MockHelper.PartyId);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result);
        Assert.Equal(204, noContentResult.StatusCode);
        _mockPartiesService.Verify(s => s.DeleteParty(MockHelper.PartyId), Times.Once);
    }

}