using System.ComponentModel.DataAnnotations;

namespace Common.Contracts.Configuration;

public sealed class ServiceEndpointOptions
{
    [Required]
    [Url]
    public string BaseUrl { get; init; } = string.Empty;
}
