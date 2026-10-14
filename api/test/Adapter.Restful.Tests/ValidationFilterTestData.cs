using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;

namespace Adapter.Restful.Tests;

internal static class ValidationFilterTestData
{
    public static ActionExecutingContext CreateContext(ModelStateDictionary modelState) =>
        new(
            new ActionContext(
                new DefaultHttpContext(),
                new RouteData(),
                new ActionDescriptor(),
                modelState),
            [],
            new Dictionary<string, object?>(),
            new object());
}
