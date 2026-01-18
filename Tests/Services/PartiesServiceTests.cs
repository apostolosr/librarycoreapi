
﻿using Moq;
using LibraryCoreApi.Database;
using LibraryCoreApi.Services.Parties;
using Xunit;


namespace LibraryCoreApi.Tests.Services;

public class PartiesServiceTests
{
  
    // [Fact]
    // public async Task TestGetParties()
    // {
    //     // Arrange
    //     var mockDataContext = new DataContext(new Mock<IConfiguration>().Object)
    //     {
    //         Parties = MockHelper.GetQueryableMockDbSet(MockHelper.GetMockParty()).Object,
    //         PartyRoles = MockHelper.GetQueryableMockDbSet(MockHelper.GetMockPartyRole()).Object,
    //         Roles = MockHelper.GetQueryableMockDbSet(MockHelper.GetMockRole()).Object
    //     };

    //     var partiesService = new PartiesService(mockDataContext);

    //     // Act
    //     var result = await partiesService.GetParties();

    //     // Assert
    //     Assert.Single(result);  
    //     Assert.Equal(MockHelper.PartyId, result.First().Id);
    //     Assert.Equal(MockHelper.Name, result.First().Name);
    //     Assert.Equal(MockHelper.Email, result.First().Email);
    //     Assert.Equal(MockHelper.Phone, result.First().Phone);
    //     Assert.Equal(MockHelper.Address, result.First().Address);
    //     Assert.Equal(new List<string> { "Author", "Customer" }, result.First().Roles);
    //     Assert.Equal(DateTime.UtcNow, result.First().CreatedAt);
    // }

    // [Fact]
    // public async Task TestGetPartiesPartyIdDoesNotExist()
    // {
    //     // Arrange
    //     var mockDataContext = new DataContext(new Mock<IConfiguration>().Object)
    //     {
    //         Parties = MockHelper.GetQueryableMockDbSet(MockHelper.GetMockParty()).Object,
    //         PartyRoles = MockHelper.GetQueryableMockDbSet(MockHelper.GetMockPartyRole()).Object,
    //         Roles = MockHelper.GetQueryableMockDbSet(MockHelper.GetMockRole()).Object
    //     };

    //     var partiesService = new PartiesService(mockDataContext);

    //     // Act, Assert
    //     await Assert.ThrowsAsync<KeyNotFoundException>(() => partiesService.GetParty(MockHelper.PartyId + 1));
    // }

    // [Fact]
    // public async Task TestCreateParty()
    // {
    //     // Arrange
    //     var mockDataContext = new DataContext(new Mock<IConfiguration>().Object)
    //     {
    //         Parties = MockHelper.GetQueryableMockDbSet(MockHelper.GetMockParty()).Object,
    //         PartyRoles = MockHelper.GetQueryableMockDbSet(MockHelper.GetMockPartyRole()).Object,
    //         Roles = MockHelper.GetQueryableMockDbSet(MockHelper.GetMockRole()).Object
    //     };
    //     var partiesService = new PartiesService(mockDataContext);
    //     var createDto = new CreatePartyDto
    //     {
    //         Name = MockHelper.Name,
    //         Email = MockHelper.Email,
    //         Phone = MockHelper.Phone,
    //         Address = MockHelper.Address,
    //         RoleIds = new List<int> { 1, 2 }
    //     };

    //     // Act
    //     PartyDto result = await partiesService.CreateParty(createDto);

    //     // Assert
    //     Assert.NotNull(result);
    //     Assert.Equal(MockHelper.PartyId, result.Id);
    //     Assert.Equal(MockHelper.Name, result.Name);
    //     Assert.Equal(MockHelper.Email, result.Email);
    //     Assert.Equal(MockHelper.Phone, result.Phone);
    //     Assert.Equal(MockHelper.Address, result.Address);
    //     Assert.Equal(new List<string> { "Author", "Customer" }, result.Roles);
    //     Assert.Equal(DateTime.UtcNow, result.CreatedAt);
    // }

    // [Fact]
    // public async Task TestCreatePartyRoleIdsDoNotExist()
    // {
    //     // Arrange
    //     var mockDataContext = new DataContext(new Mock<IConfiguration>().Object)
    //     {
    //         Parties = MockHelper.GetQueryableMockDbSet(MockHelper.GetMockParty()).Object,
    //         PartyRoles = MockHelper.GetQueryableMockDbSet(MockHelper.GetMockPartyRole()).Object,
    //         Roles = MockHelper.GetQueryableMockDbSet(MockHelper.GetMockRole()).Object
    //     };
    //     var partiesService = new PartiesService(mockDataContext);
    //     var createDto = new CreatePartyDto
    //     {
    //         Name = MockHelper.Name,
    //         Email = MockHelper.Email,
    //         Phone = MockHelper.Phone,
    //         Address = MockHelper.Address,
    //         RoleIds = new List<int> { 1, 2 }
    //     };

    //     // Act, Assert
    //     await Assert.ThrowsAsync<ApiException>(() => partiesService.CreateParty(createDto));
    // }

    // [Fact]
    // public async Task TestUpdateParty()
    // {
    //     // Arrange
    //     var mockParty = MockHelper.GetMockParty();
    //     var mockDataContext = new DataContext(new Mock<IConfiguration>().Object)
    //     {
    //         Parties = MockHelper.GetQueryableMockDbSet(MockHelper.GetMockParty()).Object,
    //         PartyRoles = MockHelper.GetQueryableMockDbSet(MockHelper.GetMockPartyRole()).Object,
    //         Roles = MockHelper.GetQueryableMockDbSet(MockHelper.GetMockRole()).Object
    //     };
    //     var partiesService = new PartiesService(mockDataContext);
    //     var updateDto = new UpdatePartyDto
    //     {
    //         Name = MockHelper.Name,
    //         Email = MockHelper.Email,
    //         Phone = MockHelper.Phone,
    //         Address = MockHelper.Address,
    //         RoleIds = new List<int> { 1, 2 }
    //     };

    //     // Act
    //     PartyDto result = await partiesService.UpdateParty(MockHelper.PartyId, updateDto);

    //     // Assert
    //     Assert.NotNull(result);
    //     Assert.Equal(MockHelper.PartyId, result.Id);
    //     Assert.Equal(MockHelper.Name, result.Name);
    //     Assert.Equal(MockHelper.Email, result.Email);
    //     Assert.Equal(MockHelper.Phone, result.Phone);
    //     Assert.Equal(MockHelper.Address, result.Address);
    //     Assert.Equal(new List<string> { "Author", "Customer" }, result.Roles);
    //     Assert.Equal(DateTime.UtcNow, result.CreatedAt);
    // }

    // [Fact]
    // public async Task TestUpdatePartyRoleIdsDoNotExist()
    // {
    //     // Arrange
    //     var mockDataContext = new DataContext(new Mock<IConfiguration>().Object)
    //     {
    //         Parties = MockHelper.GetQueryableMockDbSet(MockHelper.GetMockParty()).Object,
    //         PartyRoles = MockHelper.GetQueryableMockDbSet(MockHelper.GetMockPartyRole()).Object,
    //         Roles = MockHelper.GetQueryableMockDbSet(MockHelper.GetMockRole()).Object
    //     };
    //     var partiesService = new PartiesService(mockDataContext);
    //     var updateDto = new UpdatePartyDto
    //     {
    //         Name = MockHelper.Name,
    //         Email = MockHelper.Email,
    //         Phone = MockHelper.Phone,
    //         Address = MockHelper.Address,
    //         RoleIds = new List<int> { 1, 2 }
    //     };

    //     // Act, Assert
    //     await Assert.ThrowsAsync<KeyNotFoundException>(() => partiesService.UpdateParty(MockHelper.PartyId, updateDto));
    // }

    // [Fact]
    // public async Task TestDeleteParty()
    // {
    //     // Arrange
    //     var mockDataContext = new DataContext(new Mock<IConfiguration>().Object)
    //     {
    //         Parties = MockHelper.GetQueryableMockDbSet(MockHelper.GetMockParty()).Object,
    //         PartyRoles = MockHelper.GetQueryableMockDbSet(MockHelper.GetMockPartyRole()).Object,
    //         Roles = MockHelper.GetQueryableMockDbSet(MockHelper.GetMockRole()).Object
    //     };
    //     var partiesService = new PartiesService(mockDataContext);
    //     await partiesService.DeleteParty(MockHelper.PartyId);

    //     // Assert
    //     _mockDataContext.Verify(x => x.Parties.Remove(It.Is<Party>(p => p.Id == MockHelper.PartyId)), Times.Once);
    // }
}
