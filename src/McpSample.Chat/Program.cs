using McpSample.BlazorChat.Components;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using ModelContextProtocol.Configuration;
using ModelContextProtocol.Protocol.Transport;

var builder = WebApplication.CreateBuilder(args);

// add aspire service defaults
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add configuration service
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

// Add logging service
builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddConsole();
    loggingBuilder.AddDebug();
});

builder.Services.AddSingleton<ILogger>(sp => sp.GetRequiredService<ILogger<Program>>());

// add MCP client
builder.Services.AddSingleton<IMcpClient>(sp =>
{
    McpClientOptions mcpClientOptions = new() 
    { ClientInfo = new() { Name = "AspNetCoreSseClient", Version = "1.0.0" } };

    HttpClient httpClient = new()
    {
        BaseAddress = new("https://localhost:7133/sse")  //"https +http://aspnetsseserver" + "/sse")
    }; 

    McpServerConfig mcpServerConfig = new()
    {
        Id = "AspNetCoreSse",
        Name = "AspNetCoreSse",
        TransportType = TransportTypes.Sse,
        Location = httpClient.BaseAddress.ToString(),
    };

    var mcpClient = McpClientFactory.CreateAsync(mcpServerConfig, mcpClientOptions).GetAwaiter().GetResult();
    return mcpClient;
});

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
