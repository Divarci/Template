using AwesomeAssertions;
using Core.Library.Exceptions;
using Core.Library.ResultPattern;
using Xunit;

namespace Core.Library.Tests.ResultPattern;

public sealed class MetadataConstructorTests
{
    [Theory]
    [InlineData(25, 10, 3)]
    [InlineData(20, 10, 2)]
    [InlineData(0, 10, 0)]
    [InlineData(1, 20, 1)]
    public void Constructor_WithPositivePageSize_ComputesTotalPagesByCeiling(int totalCount, int pageSize, int expectedTotalPages)
    {
        // Arrange
        int pageNumber = 1;

        // Act
        Metadata metadata = new(
            pageNumber,
            pageSize,
            totalCount,
            pageCount: 0);

        // Assert
        metadata.TotalPages.Should().Be(expectedTotalPages);
    }

    [Fact]
    public void Constructor_WithPositivePageSize_KeepsSuppliedValues()
    {
        // Arrange
        int pageNumber = 2;

        // Act
        Metadata metadata = new(
            pageNumber,
            pageSize: 10,
            totalCount: 25,
            pageCount: 3);

        // Assert
        metadata.PageNumber.Should().Be(2);
        metadata.PageSize.Should().Be(10);
        metadata.TotalCount.Should().Be(25);
        metadata.PageCount.Should().Be(3);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_WithNonPositivePageSize_ThrowsTemplateException(int pageSize)
    {
        // Arrange
        Func<Metadata> act = () => new Metadata(
            1,
            pageSize,
            10,
            1);

        // Act & Assert
        act.Should().Throw<TemplateException>()
            .WithMessage("PageSize must be greater than zero.");
    }
}
