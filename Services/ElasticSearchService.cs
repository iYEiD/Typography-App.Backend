using Elastic.Clients.Elasticsearch;
using DotNetEnv;
using Elastic.Transport;
using DTO.Request;
using DTO.Response;

public class ElasticsearchService
{
    private readonly ElasticsearchClient _client;

    public ElasticsearchService()
    {
        var settings = new ElasticsearchClientSettings(new Uri(Env.GetString("ELASTIC_CLOUD_URL")))
            .Authentication(new BasicAuthentication(Env.GetString("ELASTIC_CLOUD_NAME"), Env.GetString("ELASTIC_CLOUD_PASSWORD")))
            .DefaultIndex("images");
        _client = new ElasticsearchClient(settings);
    }

    public async Task IndexImageDataAsync(ImageIndexRequest request)
    {

        var response = await _client.IndexAsync(request);
        if (!response.IsValidResponse)
        {
            throw new Exception("Failed to index document in Elasticsearch");
        }
    }

    public async Task<List<ImageIndexResponse>> SearchImageDataAsync(string query)
    {
        var searchResponse = await _client.SearchAsync<ImageIndexRequest>(s => s
            .Query(q => q
                .Match(m => m
                    .Field(f => f.description)
                    .Query(query)
                )
            )
        );

        if (!searchResponse.IsValidResponse)
        {
            throw new Exception("Failed to search documents in Elasticsearch");
        }

        return searchResponse.Documents.Select(doc => new ImageIndexResponse
        {
            imageName = doc.imageName,
            description = doc.description,
            createdAt = doc.createdAt.Value,
            updatedAt = doc.updatedAt
        }).ToList();
    }
}