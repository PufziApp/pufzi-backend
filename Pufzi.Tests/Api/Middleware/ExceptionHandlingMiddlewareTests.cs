using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Pufzi.Api.Middleware;

namespace Pufzi.Tests.Api.Middleware;

public class ExceptionHandlingMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_WhenNoException_ShouldCallNext()
    {
        var nextCalled = false;

        RequestDelegate next =
            _ =>
            {
                nextCalled = true;

                return Task.CompletedTask;
            };

        var logger =
            new Mock<
                ILogger<ExceptionHandlingMiddleware>>();

        var middleware =
            new ExceptionHandlingMiddleware(
                next,
                logger.Object);

        var context =
            CreateContext();

        await middleware.InvokeAsync(
            context);

        nextCalled.Should()
            .BeTrue();

        context.Response.StatusCode
            .Should()
            .Be(StatusCodes.Status200OK);
    }

    [Fact]
    public async Task InvokeAsync_WhenUnauthorizedAccessException_ShouldReturn401()
    {
        RequestDelegate next =
            _ => throw new UnauthorizedAccessException(
                "Utilizator neautentificat.");

        var logger =
            new Mock<
                ILogger<ExceptionHandlingMiddleware>>();

        var middleware =
            new ExceptionHandlingMiddleware(
                next,
                logger.Object);

        var context =
            CreateContext();

        await middleware.InvokeAsync(
            context);

        context.Response.StatusCode
            .Should()
            .Be(
                StatusCodes.Status401Unauthorized);

        context.Response.ContentType
            .Should()
            .Be("application/json");

        var response =
            await ReadResponse(context);

        response.StatusCode.Should()
            .Be(401);

        response.Message.Should()
            .Be(
                "Utilizator neautentificat.");
    }

    [Fact]
    public async Task InvokeAsync_WhenInvalidOperationException_ShouldReturn400()
    {
        RequestDelegate next =
            _ => throw new InvalidOperationException(
                "Date invalide.");

        var logger =
            new Mock<
                ILogger<ExceptionHandlingMiddleware>>();

        var middleware =
            new ExceptionHandlingMiddleware(
                next,
                logger.Object);

        var context =
            CreateContext();

        await middleware.InvokeAsync(
            context);

        context.Response.StatusCode
            .Should()
            .Be(
                StatusCodes.Status400BadRequest);

        context.Response.ContentType
            .Should()
            .Be("application/json");

        var response =
            await ReadResponse(context);

        response.StatusCode.Should()
            .Be(400);

        response.Message.Should()
            .Be("Date invalide.");
    }

    [Fact]
    public async Task InvokeAsync_WhenUnhandledException_ShouldReturn500()
    {
        var exception =
            new Exception(
                "Database crashed");

        RequestDelegate next =
            _ => throw exception;

        var logger =
            new Mock<
                ILogger<ExceptionHandlingMiddleware>>();

        var middleware =
            new ExceptionHandlingMiddleware(
                next,
                logger.Object);

        var context =
            CreateContext();

        await middleware.InvokeAsync(
            context);

        context.Response.StatusCode
            .Should()
            .Be(
                StatusCodes.Status500InternalServerError);

        context.Response.ContentType
            .Should()
            .Be("application/json");

        var response =
            await ReadResponse(context);

        response.StatusCode.Should()
            .Be(500);

        response.Message.Should()
            .Be(
                "A apărut o eroare internă. Încearcă din nou.");
    }

    [Fact]
    public async Task InvokeAsync_WhenUnhandledException_ShouldLogError()
    {
        var exception =
            new Exception(
                "Unexpected exception");

        RequestDelegate next =
            _ => throw exception;

        var logger =
            new Mock<
                ILogger<ExceptionHandlingMiddleware>>();

        var middleware =
            new ExceptionHandlingMiddleware(
                next,
                logger.Object);

        var context =
            CreateContext();

        await middleware.InvokeAsync(
            context);

        logger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>(
                    (value, _) =>
                        value.ToString()!
                            .Contains(
                                "An unhandled exception occurred.")),
                exception,
                It.IsAny<
                    Func<
                        It.IsAnyType,
                        Exception?,
                        string>>()),
            Times.Once);
    }

    [Fact]
    public async Task UnauthorizedResponse_ShouldContainValidJson()
    {
        RequestDelegate next =
            _ => throw new UnauthorizedAccessException(
                "Unauthorized");

        var logger =
            new Mock<
                ILogger<ExceptionHandlingMiddleware>>();

        var middleware =
            new ExceptionHandlingMiddleware(
                next,
                logger.Object);

        var context =
            CreateContext();

        await middleware.InvokeAsync(
            context);

        context.Response.Body.Position = 0;

        using var document =
            await JsonDocument.ParseAsync(
                context.Response.Body);

        document.RootElement
            .GetProperty("statusCode")
            .GetInt32()
            .Should()
            .Be(401);

        document.RootElement
            .GetProperty("message")
            .GetString()
            .Should()
            .Be("Unauthorized");
    }

    private static DefaultHttpContext
        CreateContext()
    {
        var context =
            new DefaultHttpContext();

        context.Response.Body =
            new MemoryStream();

        return context;
    }

    private static async Task<ErrorResponse>
        ReadResponse(
            HttpContext context)
    {
        context.Response.Body.Position = 0;

        var response =
            await JsonSerializer
                .DeserializeAsync<ErrorResponse>(
                    context.Response.Body,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive =
                            true
                    });

        response.Should()
            .NotBeNull();

        return response!;
    }

    private sealed class ErrorResponse
    {
        public int StatusCode { get; set; }

        public string Message { get; set; } =
            string.Empty;
    }
}