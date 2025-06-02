using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using CustomerApp.SharedModels.Entities;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

var app = await BuildAppAsync(builder, config);
app.Run();

static async Task<WebApplication> BuildAppAsync(WebApplicationBuilder builder, IConfiguration config)
{
    var cosmosClient = new CosmosClient(config["CosmosDb:ConnectionString"]);
    var dbResponse = await cosmosClient.CreateDatabaseIfNotExistsAsync(config["CosmosDb:DatabaseName"]);
    await dbResponse.Database.CreateContainerIfNotExistsAsync(
        config["CosmosDb:ContainerName"],
        "/id");

    builder.Services.AddSingleton(s =>
        cosmosClient.GetContainer(config["CosmosDb:DatabaseName"], config["CosmosDb:ContainerName"]));

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    app.UseSwagger();
    app.UseSwaggerUI();

    // POST
    app.MapPost("/customers", async (Customer customer, Container container) =>
    {
        customer.Id = Guid.NewGuid().ToString();
        await container.CreateItemAsync(customer, new PartitionKey(customer.Id));
        return Results.Created($"/customers/{customer.Id}", customer);
    });

    // GET all
    app.MapGet("/customers", async (Container container) =>
    {
        var query = container.GetItemLinqQueryable<Customer>();
        var iterator = query.ToFeedIterator();

        var results = new List<Customer>();
        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            results.AddRange(response);
        }
        return Results.Ok(results);
    });

    // PUT
    app.MapPut("/customers/{id}", async (string id, Customer updated, Container container) =>
    {
        updated.Id = id;
        await container.UpsertItemAsync(updated, new PartitionKey(id));
        return Results.Ok(updated);
    });

    // DELETE
    app.MapDelete("/customers/{id}", async (string id, Container container) =>
    {
        await container.DeleteItemAsync<Customer>(id, new PartitionKey(id));
        return Results.Ok();
    });

    // Search with LINQ
    app.MapGet("/search", async (string term, Container container) =>
    {
        var query = container.GetItemLinqQueryable<Customer>()
            .Where(c => c.Name.Contains(term) || c.ResponsibleSalesPerson.Name.Contains(term));

        var iterator = query.ToFeedIterator();
        var results = new List<Customer>();
        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            results.AddRange(response);
        }

        return Results.Ok(results);
    });

    return app;
}
