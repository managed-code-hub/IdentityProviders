using System.Text.Json.Serialization;

namespace ManagedCode.Orleans.Identity;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DevicePlatform
{
    Unknown,
    Android,
    iOS,
    macOS,
    Windows,
    WebBrowser,
    Api,
}

