using DistributedDictionary.Actors.Data;
using Orleans.Configuration;

var host = Host.CreateDefaultBuilder()
    .UseOrleans((ctx, silo) =>
    {
        silo.UseLocalhostClustering();
        silo.UseDashboard();
        silo.AddMemoryGrainStorage("DefinitionsStorage");
    }).ConfigureServices(services =>
    {
        services.AddSingleton<ReferenceDataService>();
    })
    .Build();

await host.StartAsync();

Console.WriteLine("Press any key to exit...");
Console.ReadKey();

await host.StopAsync();