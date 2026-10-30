using System.Diagnostics.CodeAnalysis;

namespace OrderFlow.Infrastructure.Configurations;

[ExcludeFromCodeCoverage]
public sealed class AlertServiceOptions
{
    public const string SectionName = "AlertService";

    public string Host { get; set; } = string.Empty;

    public static bool HasValidConfiguration(AlertServiceOptions options)
    {
        return
            Uri.TryCreate(options.Host, UriKind.Absolute, out var uri) &&
            uri.Scheme is "http" or "https";
    }
}