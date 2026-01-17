using LibraryCoreApi.Errors;
using System.Net;
using Moq;
using Xunit;

namespace LibraryCoreApi.Tests;

public class DefaultErrorHandlerTests
{
    [Fact]
    public async Task TestInvokeAsyncApiException()
    {
        // Arrange 
        HttpContext ctx = new DefaultHttpContext();

        RequestDelegate next = (HttpContext hc) => throw new ApiException();
        var defaultErrorHandler = new DefaultErrorHandler(next, new Mock<ILogger<DefaultErrorHandler>>().Object);

        // Act
        await defaultErrorHandler.InvokeAsync(ctx);


        // Assert
        Assert.Equal((int)HttpStatusCode.BadRequest, ctx.Response.StatusCode);
    }

    [Fact]
    public async Task TestInvokeAsyncKeyNotFoundException()
    {
        // Arrange 
        HttpContext ctx = new DefaultHttpContext();

        RequestDelegate next = (HttpContext hc) => throw new KeyNotFoundException();
        var defaultErrorHandler = new DefaultErrorHandler(next, new Mock<ILogger<DefaultErrorHandler>>().Object);

        // Act
        await defaultErrorHandler.InvokeAsync(ctx);


        // Assert
        Assert.Equal((int)HttpStatusCode.NotFound, ctx.Response.StatusCode);
    }

    [Fact]
    public async Task TestInvokeAsyncDefaultException()
    {
        // Arrange 
        HttpContext ctx = new DefaultHttpContext();

        RequestDelegate next = (HttpContext hc) => throw new Exception();
        var defaultErrorHandler = new DefaultErrorHandler(next, new Mock<ILogger<DefaultErrorHandler>>().Object);

        // Act
        await defaultErrorHandler.InvokeAsync(ctx);


        // Assert
        Assert.Equal((int)HttpStatusCode.InternalServerError, ctx.Response.StatusCode);
    }
}