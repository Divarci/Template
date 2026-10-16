using AwesomeAssertions;
using Core.Library.ResultPattern;
using System.Net;
using Xunit;

namespace Core.Library.Tests.ResultPattern.ResultExtensions;

public sealed partial class ResultExtensionsTests
{
    [Fact]
    public void MapTo_WithSuccessfulResult_ReturnsMappedDataWithMessageAndStatus()
    {
        // Arrange
        Result<string> result = Result<string>.Success(Message, Title, HttpStatusCode.Created);

        // Act
        Result<int> mappedResult = result.MapTo(MapToLength);

        // Assert
        mappedResult.IsSuccess.Should().BeTrue();
        mappedResult.StatusCode.Should().Be(HttpStatusCode.Created);
        mappedResult.Message.Should().Be(Message);
        mappedResult.Data.Should().Be(Title.Length);
    }

    [Fact]
    public void MapTo_WithSuccessfulPaginatedResult_ReturnsMappedDataWithMetadata()
    {
        // Arrange
        string[] titles = ["First", "Second"];
        PaginatedResult<string[]> result = PaginatedResult<string[]>.Success(
            titles,
            Message,
            PageNumber,
            PageSize,
            TotalCount,
            PageCount);

        // Act
        PaginatedResult<int[]> mappedResult = result.MapTo(MapToLengths);

        // Assert
        mappedResult.IsSuccess.Should().BeTrue();
        mappedResult.StatusCode.Should().Be(HttpStatusCode.OK);
        mappedResult.Message.Should().Be(Message);
        mappedResult.Data.Should().Equal(titles[0].Length, titles[1].Length);
        mappedResult.Metadata!.PageNumber.Should().Be(PageNumber);
        mappedResult.Metadata.PageSize.Should().Be(PageSize);
        mappedResult.Metadata.TotalCount.Should().Be(TotalCount);
        mappedResult.Metadata.PageCount.Should().Be(PageCount);
    }
}
