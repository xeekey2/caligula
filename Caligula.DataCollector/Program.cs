using Caligula.Service;
using Caligula.Service.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using Caligula.DataCollector;

const string defaultConnection =
    "Server=(localdb)\\MSSQLLocalDB;Database=Caligula;Trusted_Connection=True;";
var connectionString =
    Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
    ?? defaultConnection;
var apiServiceBaseUrl =
    Environment.GetEnvironmentVariable("CALIGULA_APISERVICE_URL")
    ?? "http://localhost:5483";

var runOnce = args.Contains("--run-once", StringComparer.OrdinalIgnoreCase);
int? maxPros = null;
var maxProsIndex = Array.FindIndex(args, a => a.Equals("--max-pros", StringComparison.OrdinalIgnoreCase));
if (maxProsIndex >= 0 && maxProsIndex + 1 < args.Length && int.TryParse(args[maxProsIndex + 1], out var parsed))
    maxPros = parsed;

int? onlyProId = null;
var proIdIndex = Array.FindIndex(args, a => a.Equals("--pro-id", StringComparison.OrdinalIgnoreCase));
if (proIdIndex >= 0 && proIdIndex + 1 < args.Length && int.TryParse(args[proIdIndex + 1], out var proIdParsed))
    onlyProId = proIdParsed;

void ConfigureServices(IServiceCollection services)
{
    services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connectionString));

    services.AddHttpClient<DataCollectionService>(client =>
    {
        client.BaseAddress = new Uri(apiServiceBaseUrl.TrimEnd('/') + "/");
        client.Timeout = TimeSpan.FromMinutes(30);
    });
}

if (runOnce)
{
    var host = Host.CreateDefaultBuilder(args)
        .ConfigureServices((_, services) => ConfigureServices(services))
        .Build();

    using var scope = host.Services.CreateScope();
    var collector = scope.ServiceProvider.GetRequiredService<DataCollectionService>();
    Console.WriteLine($"Importing ladder matches via {apiServiceBaseUrl} ...");
    await collector.RunFullProMatchImportAsync(maxPros: maxPros, onlyProPlayerId: onlyProId);
    Console.WriteLine("Import finished.");
    return;
}

var schedulerHost = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
    {
        ConfigureServices(services);

        services.AddSingleton<IJobFactory, SingletonJobFactory>();
        services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
        services.AddSingleton<MatchHistoryJob>();
        services.AddSingleton(new JobSchedule(
            jobType: typeof(MatchHistoryJob),
            cronExpression: "0 0 0 * * ?"));
        services.AddHostedService<CaligulaQuartzHostedService>();
    })
    .Build();

await schedulerHost.RunAsync();
