using AwesomeAssertions;
using Xunit;

namespace Core.Library.Tests.ResultPattern.Metadata;

using Metadata = Core.Library.ResultPattern.Metadata;

public sealed partial class MetadataTests
{
    [Fact]
    public void Constructor_WithValidValues_CarriesValues()
    {
        // Act
        Metadata metadata = new(PageNumber, PageSize, TotalCount, PageCount);

        // Assert
        metadata.PageNumber.Should().Be(PageNumber);
        metadata.PageSize.Should().Be(PageSize);
        metadata.TotalCount.Should().Be(TotalCount);
        metadata.PageCount.Should().Be(PageCount);
    }

    [Theory]
    [InlineData(0, 10, 0)]
    [InlineData(1, 10, 1)]
    [InlineData(10, 10, 1)]
    [InlineData(11, 10, 2)]
    [InlineData(25, 1, 25)]
    public void Constructor_WithTotalCount_CalculatesTotalPages(int totalCount, int pageSize, int expectedTotalPages)
    {
        // Act
        Metadata metadata = new(PageNumber, pageSize, totalCount, PageCount);

        // Assert
        metadata.TotalPages.Should().Be(expectedTotalPages);
    }
}
