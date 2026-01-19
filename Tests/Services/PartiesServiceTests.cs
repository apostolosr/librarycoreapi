using LibraryCoreApi.Database;
using LibraryCoreApi.Services.Parties;
using Xunit;
using Microsoft.EntityFrameworkCore;
using LibraryCoreApi.Entities;
using LibraryCoreApi.Events;
using Moq;
using LibraryCoreApi.DTOs;
using LibraryCoreApi.Errors;

namespace LibraryCoreApi.Tests.Services;

public class PartiesServiceTests : IDisposable
{
    private readonly DataContext _dbContext;
    public PartiesServiceTests()
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
    public async Task TestGetParties()
    {
        // Arrange
        var role = MockHelper.GetMockRole();
        _dbContext.Roles.Add(role);
        await _dbContext.SaveChangesAsync();
        var parties = new List<Party>
        {
            new Party { Name = "Test Party 1", Email = "test1@test.com", Phone = "1234567890", Address = "123 Test St, Test City, Test Country", PartyRoles = new List<PartyRole> { new PartyRole { RoleId = 1 } } },
            new Party { Name = "Test Party 2", Email = "test2@test.com", Phone = "1234567890", Address = "123 Test St, Test City, Test Country", PartyRoles = new List<PartyRole> { new PartyRole { RoleId = 1 } } }
        };
        _dbContext.Parties.AddRange(parties);
        await _dbContext.SaveChangesAsync();

        var partiesService = new PartiesService(_dbContext, new Mock<IEventPublisher>().Object);

        var partiesDto   = await partiesService.GetParties();
        Assert.Equal(2, partiesDto.Count());
        Assert.Equal(parties.First().Id, partiesDto.First().Id);
        Assert.Equal(parties.First().Name, partiesDto.First().Name);
        Assert.Equal(parties.First().Email, partiesDto.First().Email);
        Assert.Equal(parties.First().Phone, partiesDto.First().Phone);
        Assert.Equal(parties.First().Address, partiesDto.First().Address); 
        Assert.Equal(new List<string> { "Author" }, partiesDto.First().Roles);
    }

    [Fact]
    public async Task TestGetPartiesByPartyId()
    {
        // Arrange
        var role = MockHelper.GetMockRole();
        _dbContext.Roles.Add(role);
        await _dbContext.SaveChangesAsync();
        var party = MockHelper.GetMockParty();
        _dbContext.Parties.Add(party);
        await _dbContext.SaveChangesAsync();

        var partiesService = new PartiesService(_dbContext, new Mock<IEventPublisher>().Object);

        var partyDto = await partiesService.GetParty(party.Id);
        Assert.NotNull(partyDto);
        Assert.Equal(party.Id, partyDto.Id);
        Assert.Equal(party.Name, partyDto.Name);
        Assert.Equal(party.Email, partyDto.Email);
        Assert.Equal(party.Phone, partyDto.Phone);
        Assert.Equal(party.Address, partyDto.Address); 
    }

    [Fact]
    public async Task TestGetPartiesPartyIdDoesNotExist()
    {
        // Arrange
        var party = new Party
            {
                Name = "Test Party",
                Email = "test@test.com",
                Phone = "1234567890",
                Address = "123 Test St, Test City, Test Country",
                PartyRoles = new List<PartyRole> { new PartyRole { RoleId = 1 } }
            };
        _dbContext.Parties.Add(party);
        await _dbContext.SaveChangesAsync();

        var partiesService = new PartiesService(_dbContext, new Mock<IEventPublisher>().Object);
        
        // Act, Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => partiesService.GetParty(MockHelper.PartyId + 1));
    }

    [Fact]
    public async Task TestCreateParty()
    {
        // Arrange
          var role = MockHelper.GetMockRole();
        _dbContext.Roles.Add(role);
        await _dbContext.SaveChangesAsync();
        var createDto = new CreatePartyDto
        {
            Name = MockHelper.Name,
            Email = MockHelper.Email,
            Phone = MockHelper.Phone,
            Address = MockHelper.Address,
            RoleIds = new List<int> { 1 }
        };
        var mockEventPublisher = new Mock<IEventPublisher>();   
        mockEventPublisher.Setup(m => m.PublishEvent(It.IsAny<string>(), It.IsAny<object>())).Returns(Task.CompletedTask);
        var partiesService = new PartiesService(_dbContext, mockEventPublisher.Object);

        // Act
        var result = await partiesService.CreateParty(createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(MockHelper.PartyId, result.Id);
        Assert.Equal(MockHelper.Name, result.Name);
        Assert.Equal(MockHelper.Email, result.Email);
        Assert.Equal(MockHelper.Phone, result.Phone);
        Assert.Equal(MockHelper.Address, result.Address);
        Assert.Equal(new List<string> { "Author" }, result.Roles);
        Assert.Equal(DateTime.UtcNow.Date, result.CreatedAt.Date);
        Assert.Equal(DateTime.UtcNow.Minute, result.CreatedAt.Minute);
        mockEventPublisher.Verify(m => m.PublishEvent(
            It.Is<string>(eventName => eventName == "party.created"),
            It.Is<PartyEvent>(e => e.PartyId == result.Id && e.Name == result.Name)
        ), Times.Once);
    }

    [Fact]
    public async Task TestCreatePartyRoleIdsDoNotExist()
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

        var partiesService = new PartiesService(_dbContext, new Mock<IEventPublisher>().Object);

        // Act, Assert
        var exception = await Assert.ThrowsAsync<ApiException>(() => partiesService.CreateParty(createDto));
        Assert.Equal("One or more roles not found", exception.Message);
    }

    [Fact]
    public async Task TestUpdateParty()
    {
        // Arrange
          var roles = new List<Role>
            {
                new Role { Name = "Author" },
                new Role { Name = "Customer" }
            };
        _dbContext.Roles.AddRange(roles);
         var party = MockHelper.GetMockParty();
        _dbContext.Parties.Add(party);
        await _dbContext.SaveChangesAsync();
        var mockEventPublisher = new Mock<IEventPublisher>();
        mockEventPublisher.Setup(m => m.PublishEvent(It.IsAny<string>(), It.IsAny<object>())).Returns(Task.CompletedTask);
        var partiesService = new PartiesService(_dbContext, mockEventPublisher.Object);
        var updateDto = new UpdatePartyDto
        {
            Name = MockHelper.Name,
            Email = MockHelper.Email,
            Phone = MockHelper.Phone,
            Address = MockHelper.Address,
            RoleIds = new List<int> { 1, 2 }
        };

        // Act
        PartyDto result = await partiesService.UpdateParty(MockHelper.PartyId, updateDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(MockHelper.PartyId, result.Id);
        Assert.Equal(MockHelper.Name, result.Name);
        Assert.Equal(MockHelper.Email, result.Email);
        Assert.Equal(MockHelper.Phone, result.Phone);
        Assert.Equal(MockHelper.Address, result.Address);
        Assert.Equal(new List<string> { "Author", "Customer" }, result.Roles);
        Assert.Equal(DateTime.UtcNow.Date, result.CreatedAt.Date);
        Assert.Equal(DateTime.UtcNow.Minute, result.CreatedAt.Minute);
        mockEventPublisher.Verify(m => m.PublishEvent(
            It.Is<string>(eventName => eventName == "party.updated"),
            It.Is<PartyEvent>(e => e.PartyId == result.Id && e.Name == result.Name)
        ), Times.Once);
    }

     [Fact]
    public async Task TestUpdatePartyRoleIdsDoNotExist()
    {
        // Arrange
         var party = MockHelper.GetMockParty();
        _dbContext.Parties.Add(party);
        await _dbContext.SaveChangesAsync();
        var partiesService = new PartiesService(_dbContext, new Mock<IEventPublisher>().Object);
        var updateDto = new UpdatePartyDto
        {
            Name = MockHelper.Name,
            Email = MockHelper.Email,
            Phone = MockHelper.Phone,
            Address = MockHelper.Address,
            RoleIds = new List<int> { 1, 2 }
        };

         // Act, Assert
         var exception = await Assert.ThrowsAsync<ApiException>(() => partiesService.UpdateParty(MockHelper.PartyId, updateDto));
         Assert.Equal("One or more roles not found", exception.Message);
    }

    
    [Fact]
    public async Task TestDeleteParty()
    {
        // Arrange
        var roles = new List<Role>
            {
                new Role { Name = "Author" },
                new Role { Name = "Customer" }
            };
        _dbContext.Roles.AddRange(roles);
        var party = MockHelper.GetMockParty();
        _dbContext.Parties.Add(party);
        await _dbContext.SaveChangesAsync();

        var mockEventPublisher = new Mock<IEventPublisher>();
        mockEventPublisher.Setup(m => m.PublishEvent(It.IsAny<string>(), It.IsAny<object>())).Returns(Task.CompletedTask);
        var partiesService = new PartiesService(_dbContext, mockEventPublisher.Object);

        // Act
        await partiesService.DeleteParty(party.Id);

        // Assert
        Assert.Null(await _dbContext.Parties.FindAsync(party.Id));
        mockEventPublisher.Verify(m => m.PublishEvent(
            It.Is<string>(eventName => eventName == "party.deleted"),
            It.Is<PartyEvent>(e => e.PartyId == party.Id && e.Name == party.Name)
        ), Times.Once);
    }
}
