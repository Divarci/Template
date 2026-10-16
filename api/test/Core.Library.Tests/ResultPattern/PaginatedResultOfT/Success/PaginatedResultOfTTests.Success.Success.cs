using AwesomeAssertions;
using Core.Library.ResultPattern;
using Xunit;

namespace Core.Library.Tests.ResultPattern.PaginatedResultOfT;

public sealed partial class PaginatedResultOfTTests
{
    [Fact]
    public void Success_WithPageData_ReturnsSuccessfulResultCarryingData()
    {
        // Arrange
        string[] data = CreatePageData();

        // Act
        PaginatedResult<string[]> result = PaginatedResult<string[]>.Success(
            data,
            Message,
            PageNumber,
            PageSize,
            TotalCount,
            PageCount);

        // Assert
        AssertSuccessResult(result);
        result.Message.Should().Be(Message);
        result.Data.Should().BeSameAs(data);
        result.ErrorDetails.Should().BeNull();
    }

    [Fact]
    public void Success_WithPageData_ReturnsPaginationMetadata()
    {
        // Arrange
        string[] data = CreatePageData();

        // Act
        PaginatedResult<string[]> result = PaginatedResult<string[]>.Success(
            data,
            Message,
            PageNumber,
            PageSize,
            TotalCount,
            PageCount);

        // Assert
        AssertSuccessResult(result);
        result.Metadata.Should().NotBeNull();
        result.Metadata!.PageNumber.Should().Be(PageNumber);
        result.Metadata.PageSize.Should().Be(PageSize);
        result.Metadata.TotalCount.Should().Be(TotalCount);
        result.Metadata.PageCount.Should().Be(PageCount);
    }

    [Theory]
    [InlineData(0, 10, 0)]
    [InlineData(1, 10, 1)]
    [InlineData(10, 10, 1)]
    [InlineData(11, 10, 2)]
    public void Success_WithTotalCount_CalculatesTotalPages(int totalCount, int pageSize, int expectedTotalPages)
    {
        // Arrange
        string[] data = CreatePageData();

        // Act
        PaginatedResult<string[]> result = PaginatedResult<string[]>.Success(
            data,
            Message,
            PageNumber,
            pageSize,
            totalCount,
            expectedTotalPages);

        // Assert
        AssertSuccessResult(result);
        result.Metadata!.TotalPages.Should().Be(expectedTotalPages);
    }
}
