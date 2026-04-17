using System.CommandLine;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;

var templateTypeArg = new Option<string>("--template-type")
{
    Description = "The type of Velaptor template being used (e.g. VelaptorRenderTexture, VelaptorEmpty)",
    Required = true
};

var templateVersionArg = new Option<string>("--template-version")
{
    Description = "The version of the Velaptor template being used (e.g. v1.0.0)",
    Required = true
};

var serverHostArg = new Option<string>("--server-host")
{
    Description = "The telemetry server host (e.g. telemetry.example.com)",
    Required = true
};

var endpointArg = new Option<string>("--endpoint")
{
    Description = "The telemetry endpoint path (e.g. /velaptor-template-telemetry)",
    Required = true
};

var rootCommand = new RootCommand("Velaptor Templates Telemetry Tool")
{
    templateTypeArg,
    templateVersionArg,
    serverHostArg,
    endpointArg
};

rootCommand.SetAction(async parseResult =>
{
    var templateType = parseResult.GetValue(templateTypeArg)?.Trim() ?? string.Empty;
    var templateVersion = parseResult.GetValue(templateVersionArg)?.Trim() ?? string.Empty;
    var serverHost = parseResult.GetValue(serverHostArg)?.Trim() ?? "localhost:8000";
    var endpoint = parseResult.GetValue(endpointArg)?.Trim() ?? "/velaptor-template-telemetry";

    // If the required arguments are not provided.
    if (string.IsNullOrEmpty(templateType) || string.IsNullOrEmpty(templateVersion) ||
        string.IsNullOrEmpty(serverHost) || string.IsNullOrEmpty(endpoint))
    {
        return;
    }

    templateVersion = templateVersion.StartsWith('v')
        ? templateVersion
        : $"v{templateVersion}";

    serverHost = serverHost.StartsWith("http")
        ? serverHost.Replace("http", string.Empty)
        : serverHost;

    serverHost = serverHost.StartsWith("s://")
        ? serverHost.Replace("s://", string.Empty)
        : serverHost;

    serverHost = serverHost.StartsWith("://")
        ? serverHost.Replace("://", string.Empty)
        : serverHost;

    endpoint = endpoint.TrimStart('/');

    var isProduction = (Environment.GetEnvironmentVariable("VTT_ENV") ?? "production").Equals("production", StringComparison.CurrentCultureIgnoreCase);

    using var client = new HttpClient();
    client.Timeout = TimeSpan.FromSeconds(6);

    // Verify that the template type is valid
    if (templateType != "VelaptorRenderTexture" && templateType != "VelaptorEmpty")
    {
        return;
    }

    try
    {
        var toolVersion = typeof(Program).Assembly.GetName().Version?.ToString() ?? "unknown";

        var payload = new
        {
            templateType,
            templateVersion,
            toolVersion,
            dotnetVersion = Environment.Version.ToString(),
            locale = CultureInfo.CurrentUICulture.Name,
            rid = RuntimeInformation.RuntimeIdentifier,
            osArchitecture = RuntimeInformation.OSArchitecture.ToString().ToLower(),
            timestamp = DateTime.UtcNow.ToString("o"),
        };

        var json = JsonSerializer.Serialize(payload);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        await client.PostAsync($"{(isProduction ? "https" : "http")}://{serverHost}/{endpoint}", content);
    }
    catch
    {
        // Silently swallow - telemetry must never crash the user's workflow
    }
});

await rootCommand.Parse(args).InvokeAsync();
