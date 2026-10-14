using AwesomeAssertions;
using Core.Library.ResultPattern;
using System.Net;
using Xunit;

namespace Core.Library.Tests.ResultPattern;

public sealed class PaginatedResultOfTSuccessTests
{
    [Fact]
    public void Success_WithPage_ReturnsOkWithDataAndMetadata()
    {
        // Arrange
        string[] data = ["first", "second"];

        // Act
        PaginatedResult<string[]> result = PaginatedResult<string[]>.Success(
            data,
            "List retrieved",
            2,
            10,
            25,
            3);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Message.Should().Be("List retrieved");
        result.Data.Should().BeSameAs(data);
        result.Metadata!.PageNumber.Should().Be(2);
        result.Metadata.PageSize.Should().Be(10);
        result.Metadata.TotalCount.Should().Be(25);
        result.Metadata.PageCount.Should().Be(3);
    }
}
