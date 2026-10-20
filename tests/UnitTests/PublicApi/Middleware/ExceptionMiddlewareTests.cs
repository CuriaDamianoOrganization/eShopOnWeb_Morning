using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using BlazorShared.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.eShopWeb.ApplicationCore.Exceptions;
using Microsoft.eShopWeb.PublicApi.Middleware;
using Xunit;

namespace Microsoft.eShopWeb.UnitTests.PublicApi.Middleware;

public class ExceptionMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_PassesThroughSuccessfulRequests()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var called = false;
        var middleware = new ExceptionMiddleware(httpContext =>
        {
            called = true;
            Assert.Same(context, httpContext);
            httpContext.Response.StatusCode = StatusCodes.Status201Created;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context);

        Assert.True(called);
        Assert.Equal(StatusCodes.Status201Created, context.Response.StatusCode);
        Assert.Null(context.Response.ContentType);
        Assert.Equal(0, context.Response.Body.Length);
    }

    [Fact]
    public async Task InvokeAsync_ReturnsConflictForDuplicateException()
    {
        await AssertErrorResponse(
            new DuplicateException("already exists"),
            StatusCodes.Status409Conflict,
            "already exists");
    }

    [Fact]
    public async Task InvokeAsync_ReturnsInternalServerErrorForOtherExceptions()
    {
        await AssertErrorResponse(
            new InvalidOperationException("unexpected failure"),
            StatusCodes.Status500InternalServerError,
            "unexpected failure");
    }

    private static async Task AssertErrorResponse(Exception exception, int expectedStatusCode, string expectedMessage)
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var middleware = new ExceptionMiddleware(_ => Task.FromException(exception));

        await middleware.InvokeAsync(context);

        Assert.Equal(expectedStatusCode, context.Response.StatusCode);
        Assert.Equal("application/json", context.Response.ContentType);
        context.Response.Body.Position = 0;
        var error = await JsonSerializer.DeserializeAsync<ErrorDetails>(context.Response.Body);
        Assert.NotNull(error);
        Assert.Equal(expectedStatusCode, error.StatusCode);
        Assert.Equal(expectedMessage, error.Message);
    }
}
