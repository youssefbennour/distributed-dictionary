using DistributedDictionary;
using DistributedDictionary.ActorAbstractions.Terms;
using DistributedDictionary.SearchDefinitionsByQuery;
using DistributedDictionary.Terms.GetDefinitionByTerm;
using DistributedDictionary.Terms.UpdateDefinitionByTerm;
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
app.MapGetDefinitionsByQuery();
app.MapGetDefinitionByTerm();
app.MapUpdateDefinitionByTerm();

app.Run();