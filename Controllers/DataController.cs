using Microsoft.AspNetCore.Mvc;
using MetadataExtractor;
using System.Text.Json;
using System.Text;


    [ApiController]
    [Route("api/[controller]")]
    public class DataController : ControllerBase
    {
        [HttpPost("meta-data")]
        public IActionResult UploadImage([FromForm] IFormFile image)
        {
            if (image == null || image.Length == 0)
            {
                return BadRequest("No image uploaded.");
            }

            using (var stream = image.OpenReadStream())
            {
                var directories = ImageMetadataReader.ReadMetadata(stream);
                var metadata = new Dictionary<string, string>();

                foreach (var directory in directories)
                {
                    foreach (var tag in directory.Tags)
                    {
                        metadata[tag.Name] = tag.Description;
                    }
                }

                // Extract additional metadata like Date/Time Original if available
                var subIfdDirectory = directories.OfType<MetadataExtractor.Formats.Exif.ExifSubIfdDirectory>().FirstOrDefault();
                if (subIfdDirectory != null)
                {
                    var dateTimeOriginal = subIfdDirectory.GetDescription(MetadataExtractor.Formats.Exif.ExifDirectoryBase.TagDateTimeOriginal);
                    if (!string.IsNullOrEmpty(dateTimeOriginal))
                    {
                        metadata["Date/Time Original"] = dateTimeOriginal;
                    }
                }

                return Ok(metadata);
            }
        }

        [HttpPost("analyze-image")]
        public async Task<IActionResult> AnalyzeImage([FromForm] IFormFile image)
        {
            if (image == null || image.Length == 0)
            {
                return BadRequest("No image uploaded.");
            }

            // Convert image to byte array
            byte[] imageBytes;
            using (var ms = new MemoryStream())
            {
                await image.CopyToAsync(ms);
                imageBytes = ms.ToArray();
            }

            // Convert image to base64 string
            var base64Image = Convert.ToBase64String(imageBytes);

            // Call an AI service to analyze the image
            var analysisResult = await AnalyzeImageWithAI(base64Image);

            return Ok(analysisResult);
        }

        private async Task<string> AnalyzeImageWithAI(string base64Image)
        {
            var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
            if (string.IsNullOrEmpty(apiKey))
            {
                return "API key not found.";
            }

            using (var client = new HttpClient())
            {
                var requestContent = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new object[]
                            {
                                new
                                {
                                    text = "Mention any people you recognize in this image. Or if you find a historical place / context for the image make sure to describe it and specify the date. please make sure you only response with the description and nothing else like chattings"
                                },
                                new
                                {
                                    inline_data = new
                                    {
                                        mime_type = "image/jpeg",
                                        data = base64Image
                                    }
                                }
                            }
                        }
                    }
                };

                var jsonContent = new StringContent(JsonSerializer.Serialize(requestContent), Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={apiKey}", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var jsonResponse = JsonDocument.Parse(responseContent);
                    var text = jsonResponse.RootElement
                        .GetProperty("candidates")[0]
                        .GetProperty("content")
                        .GetProperty("parts")[0]
                        .GetProperty("text")
                        .GetString();
                    return text;
                }
                else
                {
                    return $"Error: {response.StatusCode}";
                }
            }
        }
    }
