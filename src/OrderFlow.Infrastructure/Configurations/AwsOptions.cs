using System.Diagnostics.CodeAnalysis;

namespace OrderFlow.Infrastructure.Configurations;

[ExcludeFromCodeCoverage]
public sealed class AwsOptions
{
    public const string SectionName = "AWS";

    public string ServiceUrl { get; set; } = string.Empty;
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;

    public static bool HasValidConfiguration(AwsOptions options)
    {
        return
            !string.IsNullOrWhiteSpace(options.ServiceUrl) &&
            Uri.TryCreate(options.ServiceUrl, UriKind.Absolute, out var uri) &&
            uri.Scheme is "http" or "https" &&
            !string.IsNullOrWhiteSpace(options.AccessKey) &&
            !string.IsNullOrWhiteSpace(options.SecretKey);
    }
}