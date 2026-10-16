using AwesomeAssertions;
using Core.Library.Exceptions;
using Xunit;

namespace Core.Library.Tests.ResultPattern.Metadata;

using Metadata = Core.Library.ResultPattern.Metadata;

public sealed partial class MetadataTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_WithNonPositivePageSize_ThrowsTemplateException(int pageSize)
    {
        // Act
        Action act = () => new Metadata(PageNumber, pageSize, TotalCount, PageCount);

        // Assert
        act.Should().Throw<TemplateException>();
    }
}
