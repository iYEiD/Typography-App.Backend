using Elastic.Clients.Elasticsearch;
using DotNetEnv;
using Elastic.Transport;
using DTO.Request;

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
}