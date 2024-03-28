using System.Text.Json;
using System.Text.Json.Serialization;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Infrastructure.Serialization;

/// <inheritdoc />
public sealed class DefaultJsonSerializer : IJsonSerializer
{
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultJsonSerializer" /> class.
    /// </summary>
    public DefaultJsonSerializer()
        => _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters =
            {
                new JsonStringEnumConverter(),
                new JsonUdiConverter(),
                // We may need to add JsonObjectConverter at some point, but for the time being things work fine without
                //new JsonObjectConverter()
            }
        };

    /// <inheritdoc />
    public string Serialize(object? input) => JsonSerializer.Serialize(input, _jsonSerializerOptions);

    /// <inheritdoc />
    public T? Deserialize<T>(string input) => JsonSerializer.Deserialize<T>(input, _jsonSerializerOptions);
}
