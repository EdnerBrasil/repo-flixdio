using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using System.Net;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Serialization.HybridRow;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace fnGetMovieDetail;

public class Function1
{
    private readonly ILogger<Function1> _logger;
    private readonly CosmosClient _cosmosClient;
    public Function1(ILogger<Function1> logger, CosmosClient cosmosClient)
    {
        _logger = logger;
        _cosmosClient = cosmosClient;
    }

    [Function("detail")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        // Parse query string for 'id'
        var queryParams = QueryHelpers.ParseQuery(req.Url.Query);
        if (!queryParams.TryGetValue("id", out StringValues idValues) || StringValues.IsNullOrEmpty(idValues))
        {
            var badResp = req.CreateResponse(HttpStatusCode.BadRequest);
            await badResp.WriteStringAsync("Missing required query parameter 'id'.");
            return badResp;
        }

        var id = idValues.ToString();

            try
            {
            var container = _cosmosClient.GetContainer("DioFlixDB01", "movies");

            // If your container's partition key is the id or another property, prefer ReadItemAsync for performance.
            var query = "SELECT * FROM c WHERE c.id = @id";
            var queryDefinition = new QueryDefinition(query).WithParameter("@id", id);
            var iterator = container.GetItemQueryIterator<MovieResult>(queryDefinition);

            MovieResult? found = null;
            while (iterator.HasMoreResults)
            {
                var page = await iterator.ReadNextAsync();
                found = page.FirstOrDefault();
                if (found != null) break;
            }

            if (found == null)
            {
                var notFound = req.CreateResponse(HttpStatusCode.NotFound);
                await notFound.WriteStringAsync($"Movie with id '{id}' not found.");
                return notFound;
            }

            var ok = req.CreateResponse(HttpStatusCode.OK);
            await ok.WriteAsJsonAsync(found);
            return ok;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching movie detail for id={Id}", id);
            var resp = req.CreateResponse(HttpStatusCode.InternalServerError);
            await resp.WriteStringAsync("An error occurred while fetching the movie details.");
            return resp;
        }
    }
}