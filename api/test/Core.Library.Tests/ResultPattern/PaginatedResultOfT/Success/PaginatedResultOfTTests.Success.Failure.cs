using AwesomeAssertions;
using Core.Library.Exceptions;
using Core.Library.ResultPattern;
using Xunit;

namespace Core.Library.Tests.ResultPattern.PaginatedResultOfT;

public sealed partial class PaginatedResultOfTTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Success_WithNonPositivePageSize_ThrowsTemplateException(int pageSize)
    {
        // Arrange
        string[] data = CreatePageData();

        // Act
        Action act = () => PaginatedResult<string[]>.Success(
            data,
            Message,
            PageNumber,
            pageSize,
            TotalCount,
            PageCount);

        // Assert
        act.Should().Throw<TemplateException>();
    }
}
