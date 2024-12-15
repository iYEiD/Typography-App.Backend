using Elastic.Clients.Elasticsearch;
using DotNetEnv;
using Elastic.Transport;

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

    public async Task IndexImageDataAsync(string description, string userId, string imageName)
    {
        var imageData = new
        {
            description = description,
            userId = userId,
            imageName = imageName
        };

        var response = await _client.IndexAsync(imageData);
        if (!response.IsValidResponse)
        {
            throw new Exception("Failed to index document in Elasticsearch");
        }
    }
}