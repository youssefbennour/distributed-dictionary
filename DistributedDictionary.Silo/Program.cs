using Orleans.Configuration;

var host = Host.CreateDefaultBuilder()
    .UseOrleans((ctx, silo) =>
    {
        silo.UseLocalhostClustering();
        silo.Configure<ClusterOptions>(options =>
        {  
            options.ClusterId = "dev";
            options.ServiceId = "distributed-dictionary";
        });
        silo.UseDashboard();
        silo.AddMemoryGrainStorage("DefinitionsStorage");
    })
    .Build();

await host.StartAsync();

Console.WriteLine("Press any key to exit...");
Console.ReadKey();

await host.StopAsync();