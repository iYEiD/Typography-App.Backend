using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using DotNetEnv;


[ApiController]
[Route("api/[controller]")]
    public class AIController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private const string OpenAIApiUrl = "https://api.openai.com/v1/chat/completions";
        private readonly string OpenAIApiKey = Env.GetString("OPENAI_API_KEY");

        public AIController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpPost("upload-image")]
        public async Task<IActionResult> UploadImage([FromForm] IFormFile image)
        {
            if (image == null || image.Length == 0)
            {
                return BadRequest("No image uploaded.");
            }

            using (var memoryStream = new MemoryStream())
            {
                await image.CopyToAsync(memoryStream);
                var imageBytes = memoryStream.ToArray();
                var base64Image = Convert.ToBase64String(imageBytes);

                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", OpenAIApiKey);

                var jsonContent = new StringContent($"{{ \"model\": \"gpt-4o-mini\", \"messages\": [{{ \"role\": \"user\", \"content\": [{{ \"type\": \"text\", \"text\": \"What is in this image?\" }}, {{ \"type\": \"image_url\", \"image_url\": {{ \"url\": \"data:image/jpeg;base64,{base64Image}\" }} }}] }}] }}", System.Text.Encoding.UTF8, "application/json");

                var response = await client.PostAsync(OpenAIApiUrl, jsonContent);

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, "Error processing image with OpenAI API.");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseContent);
                // Extract the description from the responseContent

                return Ok(responseContent);
            }
        }
}
