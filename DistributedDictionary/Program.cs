using DistributedDictionary;
using DistributedDictionary.ActorAbstractions.Terms;
using DistributedDictionary.Terms.GetDefinitionByTerm;
using Orleans.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.UseOrleansClient(clientBuilder =>
{
    clientBuilder.UseLocalhostClustering();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapGetDefinitionByTerm();

app.Run();

namespace DistributedDictionary
{
    record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
    {
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    }
}