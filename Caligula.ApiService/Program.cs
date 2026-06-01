using Caligula.Model.SC2Pulse;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Caligula.Model.Caligula;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();

var httpClient = new HttpClient { BaseAddress = new Uri("https://sc2pulse.nephest.com/sc2/") };

app.MapGet("/playerid/{playerName}", async (string playerName) =>
{
    var response = await httpClient.GetAsync($"api/character/search?term={playerName}");
    if (response.IsSuccessStatusCode)
    {
        var content = await response.Content.ReadAsStringAsync();
        return Results.Content(content, "application/json");
    }
    return Results.BadRequest("Failed to retrieve player ID.");
});



app.MapGet("/proplayer/matchhistory/{proPlayerId}/{date?}", async (int proPlayerId, string? date) =>
{
    var dateCursor = string.IsNullOrWhiteSpace(date)
        ? DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.ffffff'Z'")
        : date;
    var response = await httpClient.GetAsync(
        $"api/group/match?proPlayerId={proPlayerId}&dateCursor={Uri.EscapeDataString(dateCursor)}&typeCursor=_1V1");
    if (response.IsSuccessStatusCode)
    {
        var content = await response.Content.ReadAsStringAsync();
        return Results.Content(content, "application/json");
    }
    return Results.BadRequest("Failed to retrieve match history.");
});

app.MapGet("/proplayer/ids/{proPlayerId}", async (int proPlayerId) =>
{
    var response = await httpClient.GetAsync($"api/group/character/full?proPlayerId={proPlayerId}");
    if (response.IsSuccessStatusCode)
    {
        var content = await response.Content.ReadAsStringAsync();
        return Results.Content(content, "application/json");
    }
    return Results.BadRequest("Failed to retrieve all IDs from pro player ID.");
});

app.MapGet("/playername/{id}", async (int id) =>
{
    var response = await httpClient.GetAsync($"api/character/{id}");
    if (response.IsSuccessStatusCode)
    {
        var content = await response.Content.ReadAsStringAsync();
        return Results.Content(content, "application/json");
    }
    return Results.BadRequest("Failed to retrieve name from ID.");
});

app.MapGet("/GetPlayerInfo/{charId}", async (int charId) =>
{
    var response = await httpClient.GetAsync($"api/character/{charId}/common");
    if (response.IsSuccessStatusCode)
    {
        var content = await response.Content.ReadAsStringAsync();
        var playerData = JsonConvert.DeserializeObject<PlayerInfo>(content);
        return Results.Content(content, "application/json");
    }
    return Results.BadRequest("Failed to retrieve pro player ID.");
});



app.MapDefaultEndpoints();

app.Run();
