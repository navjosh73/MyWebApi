using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);
// Explicitly configure console logging so app.Logger messages appear in the terminal.
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();


var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

// Existing endpoint
app.MapGet("/weatherforecast", () => "Token is Blank")
    .WithName("GetToken");

// New endpoint: accepts a token query parameter, authenticates it, then calls WebToAuthorise if valid.
app.MapGet("/get-weather", async (string? token) =>
{
    // Validate the token (hardcoded check)
    if (string.IsNullOrWhiteSpace(token) || token != "valid-token-123")
    {
        app.Logger.LogWarning("authentication fail");
        return Results.Unauthorized();
    }

    // Token is valid, call WebToAuthorise's /weatherforecast endpoint
    using var http = new HttpClient();
    var url = "http://localhost:5000/weatherforecast";
    app.Logger.LogInformation("authentication success");
    try
    {
        var response = await http.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        app.Logger.LogInformation("Received response from WebToAuthorise: {Length} bytes", content?.Length ?? 0);
        return Results.Ok(content);
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Error calling WebToAuthorise");
        return Results.Problem($"Error calling WebToAuthorise: {ex.Message}");
    }
  
});

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
