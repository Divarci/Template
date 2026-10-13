using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;

namespace Adapter.Restful.AspNetCore.Authentication;

internal sealed class JwtBearerConfigurationOptions(IConfiguration configuration)
    : IConfigureNamedOptions<JwtBearerOptions>
{
    private const string ConfigurationSectionName = "Authentication";

    private readonly IConfiguration _configuration = configuration;

    public void Configure(JwtBearerOptions options)
    {
        _configuration.GetSection(ConfigurationSectionName).Bind(options);

        options.MapInboundClaims = false;

        options.TokenValidationParameters.NameClaimType = "preferred_username";
    }

    public void Configure(string? name, JwtBearerOptions options)
    {
        Configure(options);
    }
}
