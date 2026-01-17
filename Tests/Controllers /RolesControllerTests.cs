using Moq;
using LibraryCoreApi.Controllers;
using LibraryCoreApi.Services.Roles;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using LibraryCoreApi.DTOs;
using LibraryCoreApi.Errors;

namespace LibraryCoreApi.Tests.Controllers;

public class RolesControllerTests
{
    private readonly Mock<IRolesService> _mockRolesService;
    private readonly RolesController _rolesController;

    public RolesControllerTests()
    {
        _mockRolesService = new Mock<IRolesService>();
        _rolesController = new RolesController(_mockRolesService.Object);
    }

    [Fact]
    public async Task GetRoles_ReturnsOkResult_WithListOfRoles()
    {
        // Arrange
        var expectedRoles = new List<RoleDto> 
        { 
            MockHelper.GetMockRoleDto()
        };
        _mockRolesService.Setup(s => s.GetRoles()).ReturnsAsync(expectedRoles);

        // Act
        var result = await _rolesController.GetRoles();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var roles = Assert.IsAssignableFrom<IEnumerable<RoleDto>>(okResult.Value);
        Assert.Single(roles);
        Assert.Equal(200, okResult.StatusCode);
        _mockRolesService.Verify(s => s.GetRoles(), Times.Once);
    }

    [Fact]
    public async Task GetRoles_ReturnsOkResult_WithEmptyList()
    {
        // Arrange
        var emptyList = new List<RoleDto>();
        _mockRolesService.Setup(s => s.GetRoles()).ReturnsAsync(emptyList);

        // Act
        var result = await _rolesController.GetRoles();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var roles = Assert.IsAssignableFrom<IEnumerable<RoleDto>>(okResult.Value);
        Assert.Empty(roles);
        _mockRolesService.Verify(s => s.GetRoles(), Times.Once);
    }

    [Fact]
    public async Task GetRole_ReturnsOkResult_WhenRoleExists()
    {
        // Arrange
        var expectedRole = MockHelper.GetMockRoleDto();
        _mockRolesService.Setup(s => s.GetRole(It.IsAny<int>())).ReturnsAsync(expectedRole);

        // Act
        var result = await _rolesController.GetRole(MockHelper.RoleId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var role = Assert.IsType<RoleDto>(okResult.Value);
        Assert.Equal(MockHelper.RoleId, role.Id);
        Assert.Equal(MockHelper.RoleName, role.Name);
        Assert.Equal(200, okResult.StatusCode);
        _mockRolesService.Verify(s => s.GetRole(MockHelper.RoleId), Times.Once);
    }

    [Fact]
    public async Task CreateRole_ReturnsCreatedAtAction_WhenRoleIsCreated()
    {
        // Arrange
        var createDto = new CreateRoleDto
        {
            Name = MockHelper.RoleName,
            Description = MockHelper.RoleDescription
        };
        _mockRolesService.Setup(s => s.CreateRole(It.IsAny<CreateRoleDto>())).ReturnsAsync(MockHelper.GetMockRoleDto());

        // Act
        var result = await _rolesController.CreateRole(createDto);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var role = Assert.IsType<RoleDto>(createdAtActionResult.Value);
        Assert.Equal(MockHelper.RoleId, role.Id);
        Assert.Equal(MockHelper.RoleName, role.Name);
        Assert.Equal(201, createdAtActionResult.StatusCode);
        Assert.Equal(nameof(RolesController.GetRole), createdAtActionResult.ActionName);
        _mockRolesService.Verify(s => s.CreateRole(It.Is<CreateRoleDto>(d => d.Name == MockHelper.RoleName)), Times.Once);
    }


    [Fact]
    public async Task UpdateRole_ReturnsOkResult_WhenRoleIsUpdated()
    {   
        // Arrange
        var updateDto = new UpdateRoleDto
        {
            Name = "Updated Name",
            Description = MockHelper.RoleDescription
        };
        _mockRolesService.Setup(s => s.UpdateRole(It.IsAny<int>(), It.IsAny<UpdateRoleDto>())).ReturnsAsync(MockHelper.GetMockRoleDto());

        // Act
        var result = await _rolesController.UpdateRole(MockHelper.RoleId, updateDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var role = Assert.IsType<RoleDto>(okResult.Value);
        Assert.Equal(MockHelper.RoleId, role.Id);
        Assert.Equal(MockHelper.RoleName, role.Name);
        Assert.Equal(200, okResult.StatusCode);
        _mockRolesService.Verify(s => s.UpdateRole(MockHelper.RoleId, It.IsAny<UpdateRoleDto>()), Times.Once);
    }

    [Fact]
    public async Task DeleteRole_ReturnsNoContent_WhenRoleIsDeleted()
    {
        // Arrange
        _mockRolesService.Setup(s => s.DeleteRole(It.IsAny<int>()));

        // Act
        var result = await _rolesController.DeleteRole(MockHelper.RoleId);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result);
        Assert.Equal(204, noContentResult.StatusCode);
        _mockRolesService.Verify(s => s.DeleteRole(MockHelper.RoleId), Times.Once);
    }

}