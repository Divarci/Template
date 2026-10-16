using AwesomeAssertions;

namespace Core.Library.Tests.ResultPattern.ErrorDetails;

public sealed partial class ErrorDetailsTests
{
    private const string FirstError = "Title cannot be null or empty.";
    private const string SecondError = "Content must be 5000 characters or less.";
    private const string TraceId = "0HN7S2V1QK0J4:00000001";

    private static void AssertGeneratedTraceId(string traceId)
    {
        Guid.TryParse(traceId, out _).Should().BeTrue();
    }
}
