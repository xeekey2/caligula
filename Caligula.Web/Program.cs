using Caligula.Service;
using Caligula.Service.Entity;
using Caligula.Web.ApiClients;
using Caligula.Web.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();
var defaultConnection =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=(localdb)\\MSSQLLocalDB;Database=Caligula;Trusted_Connection=True;";
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(defaultConnection));

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddOutputCache();

// Configure HttpClient with base address
builder.Services.AddHttpClient("Sc2PulseClient", client =>
{
    client.BaseAddress = new Uri("https+http://apiservice");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Register your custom services and ensure they use the named HttpClient
builder.Services.AddScoped(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("Sc2PulseClient");
    var dbContext = sp.GetRequiredService<ApplicationDbContext>();
    return new DataCollectionService(dbContext, httpClient);
});

builder.Services.AddScoped(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("Sc2PulseClient");
    var dbContext = sp.GetRequiredService<ApplicationDbContext>();
    return new PlayerComparisonService(dbContext);
});

builder.Services.AddScoped(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("Sc2PulseClient");
    var dataCollectionService = sp.GetRequiredService<DataCollectionService>();
    var playerComparisonService = sp.GetRequiredService<PlayerComparisonService>();
    var logger = sp.GetRequiredService<ILogger<MatchHistoryApiClient>>();
    return new MatchHistoryApiClient(dataCollectionService, playerComparisonService, logger, httpClient);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.UseOutputCache();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.Run();
