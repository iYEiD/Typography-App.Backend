using DotNetEnv;
using System.Text.Json;

namespace Services
{
    public class AIProcessingService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private const string OpenAIApiUrl = "https://api.openai.com/v1/chat/completions";
        private readonly string OpenAIApiKey = Env.GetString("OPENAI_API_KEY");

        public AIProcessingService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<string> ProcessImageAsync(IFormFile image)
        {
            if (image == null || image.Length == 0)
            {
                throw new ArgumentException("No image uploaded.");
            }

            using (var memoryStream = new MemoryStream())
            {
                await image.CopyToAsync(memoryStream);
                var imageBytes = memoryStream.ToArray();
                var base64Image = Convert.ToBase64String(imageBytes);

                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", OpenAIApiKey);
                // To do : limit the tokens 1000
                var prompt = "You are an advanced image analyzer. Your task is to provide a detailed description of the image provided. " +
                            "1. If there are people in the image, identify and mention their names if they are famous or political figures. " +
                            "2. If the image contains a recognizable place, location, or landmark, describe it and mention its name if identifiable. " +
                            "3. Provide thorough details about the objects, actions, expressions, and the overall setting of the image. " +
                            "4. Do not include any extra commentary, opinions, or information outside of the image description. " +
                            "Write the description in one paragraph, nothing else.";

                var jsonContent = new StringContent($@"
                {{
                    ""model"": ""gpt-4o-mini"",
                    ""messages"": [
                        {{
                            ""role"": ""user"",
                            ""content"": [
                                {{ ""type"": ""text"", ""text"": ""{prompt}"" }},
                                {{ ""type"": ""image_url"", ""image_url"": {{ ""url"": ""data:image/jpeg;base64,{base64Image}"" }} }}
                            ]
                        }}
                    ],
                    ""max_tokens"": 1000
                }}", System.Text.Encoding.UTF8, "application/json");

                var response = await client.PostAsync(OpenAIApiUrl, jsonContent);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Error processing image with OpenAI API.");
                }
                
                var responseContent = await response.Content.ReadAsStringAsync();
                var jsonResponse = JsonDocument.Parse(responseContent);
                var content = jsonResponse.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();
                
                Console.WriteLine("Description of image: " + content);
                return content;
            }
        }
    }
}