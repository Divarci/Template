using AwesomeAssertions;
using Core.Library.Exceptions;
using Core.Library.ResultPattern;
using System.Net;
using Xunit;

namespace Core.Library.Tests.ResultPattern.ResultExtensions;

public sealed partial class ResultExtensionsTests
{
    [Fact]
    public void MapTo_WithFailedResult_ThrowsTemplateException()
    {
        // Arrange
        Result<string> result = Result<string>.Failure(Message, HttpStatusCode.NotFound);

        // Act
        Action act = () => result.MapTo(MapToLength);

        // Assert
        act.Should().Throw<TemplateException>()
            .WithMessage("Cannot map a failed result.");
    }

    [Fact]
    public void MapTo_WithFailedResultCarryingData_ThrowsTemplateException()
    {
        // Arrange
        Result<string> result = CreateFailedResultWithData(Title);

        // Act
        Action act = () => result.MapTo(MapToLength);

        // Assert
        act.Should().Throw<TemplateException>()
            .WithMessage("Cannot map a failed result.");
    }

    [Fact]
    public void MapTo_WithFailedResultCarryingValueTypeData_ThrowsTemplateException()
    {
        // Arrange
        Result<int> result = CreateFailedResultWithData(Title.Length);

        // Act
        Action act = () => result.MapTo(MapToText);

        // Assert
        act.Should().Throw<TemplateException>()
            .WithMessage("Cannot map a failed result.");
    }

    [Fact]
    public void MapTo_WithFailedPaginatedResult_ThrowsTemplateException()
    {
        // Arrange
        PaginatedResult<string[]> result = PaginatedResult<string[]>.Failure(Message, HttpStatusCode.InternalServerError);

        // Act
        Action act = () => result.MapTo(MapToLengths);

        // Assert
        act.Should().Throw<TemplateException>()
            .WithMessage("Cannot map a failed result.");
    }

    [Fact]
    public void MapTo_WithFailedPaginatedResultCarryingData_ThrowsTemplateException()
    {
        // Arrange
        string[] titles = ["First", "Second"];
        PaginatedResult<string[]> result = CreateFailedPaginatedResultWithData(titles);

        // Act
        Action act = () => result.MapTo(MapToLengths);

        // Assert
        act.Should().Throw<TemplateException>()
            .WithMessage("Cannot map a failed result.");
    }
}
