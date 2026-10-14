using AwesomeAssertions;
using Core.Library.Exceptions;
using Core.Library.ResultPattern;
using System.Net;
using Xunit;

namespace Core.Library.Tests.ResultPattern;

public sealed class ResultExtensionsMapToTests
{
    [Fact]
    public void MapTo_WhenResultSucceeds_MapsDataAndKeepsMessageAndStatus()
    {
        // Arrange
        Result<int> result = Result<int>.Success(
            "Created",
            42,
            HttpStatusCode.Created);

        // Act
        Result<string> mapped = result.MapTo(value => $"#{value}");

        // Assert
        mapped.IsSuccess.Should().BeTrue();
        mapped.Data.Should().Be("#42");
        mapped.Message.Should().Be("Created");
        mapped.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public void MapTo_WhenResultFailedWithoutData_ThrowsTemplateException()
    {
        // Arrange
        Result<string> result = Result<string>.Failure("Not found", HttpStatusCode.NotFound);
        Func<Result<int>> act = () => result.MapTo(value => value.Length);

        // Act & Assert
        act.Should().Throw<TemplateException>()
            .WithMessage("Cannot map a failed result.");
    }

    [Fact]
    public void MapTo_WhenPaginatedResultSucceeds_MapsDataAndKeepsMetadata()
    {
        // Arrange
        PaginatedResult<int[]> result = PaginatedResult<int[]>.Success(
            [1, 2],
            "List retrieved",
            2,
            10,
            25,
            3);

        // Act
        PaginatedResult<string[]> mapped = result.MapTo<int[], string[]>(values => [.. values.Select(v => $"#{v}")]);

        // Assert
        mapped.IsSuccess.Should().BeTrue();
        mapped.Data.Should().Equal("#1", "#2");
        mapped.Message.Should().Be("List retrieved");
        mapped.Metadata!.PageNumber.Should().Be(2);
        mapped.Metadata.PageSize.Should().Be(10);
        mapped.Metadata.TotalCount.Should().Be(25);
        mapped.Metadata.PageCount.Should().Be(3);
    }

    [Fact]
    public void MapTo_WhenPaginatedResultFailedWithoutData_ThrowsTemplateException()
    {
        // Arrange
        PaginatedResult<int[]> result = PaginatedResult<int[]>.Failure("Query failed", HttpStatusCode.InternalServerError);
        Func<PaginatedResult<string[]>> act = () => result.MapTo<int[], string[]>(values => [.. values.Select(v => $"#{v}")]);

        // Act & Assert
        act.Should().Throw<TemplateException>()
            .WithMessage("Cannot map a failed result.");
    }
}
