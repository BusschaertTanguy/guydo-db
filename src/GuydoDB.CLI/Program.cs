using GuydoDB.CLI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var configuration = new ConfigurationBuilder()
    .AddCommandLine(args)
    .AddEnvironmentVariables()
    .Build();

var services = new ServiceCollection()
    .Configure<AppConfiguration>(configuration)
    .AddSingleton<Startup>();

var serviceProvider = services.BuildServiceProvider();
var startup = serviceProvider.GetRequiredService<Startup>();
await startup.StartAsync();