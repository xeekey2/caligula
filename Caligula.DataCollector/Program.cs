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

void ConfigureServices(IServiceCollection services)
{
    services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connectionString));

    services.AddHttpClient<DataCollectionService>(client =>
    {
        client.BaseAddress = new Uri(apiServiceBaseUrl.TrimEnd('/') + "/");
        client.Timeout = TimeSpan.FromMinutes(10);
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
    await collector.RunDailyMatchHistoryUpdateAsync();
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
