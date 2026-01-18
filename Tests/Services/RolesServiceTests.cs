using LibraryCoreApi.Database;
using LibraryCoreApi.Services.Roles;
using Xunit;
using Microsoft.EntityFrameworkCore;
using LibraryCoreApi.Entities;
using LibraryCoreApi.Events;
using Moq;
using LibraryCoreApi.DTOs;
using LibraryCoreApi.Errors;

namespace LibraryCoreApi.Tests.Services;

public class RolesServiceTests : IDisposable
{
    private readonly DataContext _dbContext;
    public RolesServiceTests()
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
    public async Task TestGetRoles()
    {
        // Arrange
        var roles = new List<Role>  
        {
            new Role
            {
                Name = "Test Role 1"
            },
            new Role
            {
                Name = "Test Role 2"
            }
        };

        _dbContext.Roles.AddRange(roles);
        await _dbContext.SaveChangesAsync();

        var rolesService = new RolesService(_dbContext, new Mock<IEventPublisher>().Object);

        var rolesDto = await rolesService.GetRoles();
        Assert.Equal(2, rolesDto.Count());
        Assert.Equal(roles.First().Id, rolesDto.First().Id);
        Assert.Equal(roles.First().Name, rolesDto.First().Name);
    }

    [Fact]
    public async Task TestGetRolesRoleIdDoesNotExist()
    {
        // Arrange
        var role = new Role
        {
            Name = "Test Role"
        };
        _dbContext.Roles.Add(role);
        await _dbContext.SaveChangesAsync();

        var rolesService = new RolesService(_dbContext, new Mock<IEventPublisher>().Object);
        
        // Act, Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => rolesService.GetRole(MockHelper.RoleId + 1));
    }

    [Fact]
    public async Task TestCreateRole()
    {
        // Arrange
        var createDto = new CreateRoleDto
        {
            Name = MockHelper.RoleName,
            Description = MockHelper.RoleDescription
        };

        var mockEventPublisher = new Mock<IEventPublisher>();
        mockEventPublisher.Setup(m => m.PublishEvent(It.IsAny<string>(), It.IsAny<object>())).Returns(Task.CompletedTask);
        var rolesService = new RolesService(_dbContext, mockEventPublisher.Object);

        // Act
        var result = await rolesService.CreateRole(createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(MockHelper.RoleId, result.Id);
        Assert.Equal(MockHelper.RoleName, result.Name);
        Assert.Equal(MockHelper.RoleDescription, result.Description);
        Assert.Equal(DateTime.UtcNow.Date, result.CreatedAt.Date);
        Assert.Equal(DateTime.UtcNow.Minute, result.CreatedAt.Minute);
        mockEventPublisher.Verify(m => m.PublishEvent(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
    }

    [Fact]
    public async Task TestCreateRoleNameAlreadyExists()
    {
        // Arrange
        var role = new Role
        {
            Name = MockHelper.RoleName,
            Description = MockHelper.RoleDescription
        };
        _dbContext.Roles.Add(role);
        await _dbContext.SaveChangesAsync();
        var createDto = new CreateRoleDto
        {
            Name = MockHelper.RoleName,
            Description = MockHelper.RoleDescription
        };

        var rolesService = new RolesService(_dbContext, new Mock<IEventPublisher>().Object);

        // Act, Assert
        await Assert.ThrowsAsync<ApiException>(() => rolesService.CreateRole(createDto));
    }

    [Fact]
    public async Task TestUpdateRole()
    {
        // Arrange
        var role = new Role
        {
            Name = "Test Role",
            Description = "Test Description"
        };
        _dbContext.Roles.Add(role);
        await _dbContext.SaveChangesAsync();
        var roleId = role.Id;
        var mockEventPublisher = new Mock<IEventPublisher>();
        mockEventPublisher.Setup(m => m.PublishEvent(It.IsAny<string>(), It.IsAny<object>())).Returns(Task.CompletedTask);
        var rolesService = new RolesService(_dbContext, mockEventPublisher.Object);
        var updateDto = new UpdateRoleDto
        {
            Name = MockHelper.RoleName,
            Description = MockHelper.RoleDescription
        };

        // Act
        RoleDto result = await rolesService.UpdateRole(roleId, updateDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(roleId, result.Id);
        Assert.Equal(MockHelper.RoleName, result.Name);
        Assert.Equal(MockHelper.RoleDescription, result.Description);
        Assert.Equal(DateTime.UtcNow.Date, result.CreatedAt.Date);
        Assert.Equal(DateTime.UtcNow.Minute, result.CreatedAt.Minute);
        mockEventPublisher.Verify(m => m.PublishEvent(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
    }

    [Fact]
    public async Task TestUpdateRoleNameAlreadyExists()
    {
        // Arrange
        var roles = new List<Role>
        {
            new Role
            {
                Name = MockHelper.RoleName,
                Description = MockHelper.RoleDescription
            },
            new Role
            {
                Name = "Customer",
                Description = "Test Description"
            }
        };
        _dbContext.Roles.AddRange(roles);
        await _dbContext.SaveChangesAsync();
        var roleId = roles.First().Id;
        var mockEventPublisher = new Mock<IEventPublisher>();
        mockEventPublisher.Setup(m => m.PublishEvent(It.IsAny<string>(), It.IsAny<object>())).Returns(Task.CompletedTask);
        var rolesService = new RolesService(_dbContext, mockEventPublisher.Object);
        var updateDto = new UpdateRoleDto
        {
            Name = "Customer",
            Description = MockHelper.RoleDescription
        };

        // Act, Assert
        await Assert.ThrowsAsync<ApiException>(() => rolesService.UpdateRole(roleId, updateDto));
    }
 
    [Fact]
    public async Task TestDeleteRole()
    {
        // Arrange
        var role = new Role
        {
            Name = "Test Role",
            Description = "Test Description"
        };
        _dbContext.Roles.Add(role);
        await _dbContext.SaveChangesAsync();

        var rolesService = new RolesService(_dbContext, new Mock<IEventPublisher>().Object);

        // Act
        await rolesService.DeleteRole(role.Id);

        // Assert
        Assert.Null(await _dbContext.Roles.FindAsync(role.Id));
    }
}
