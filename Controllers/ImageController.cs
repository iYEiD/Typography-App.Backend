using Microsoft.AspNetCore.Mvc;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;
using DotNetEnv;
using Services;
using DTO.Request;


[ApiController]
    [Route("api/[controller]")]
    public class ImageController : ControllerBase
    {
        private readonly MinioClient _minioClient;
        private readonly AIProcessingService _aiProcessingService;

        private readonly ElasticsearchService _elasticsearchService;
        private readonly UserService _userService;

        public ImageController(AIProcessingService aiProcessingService, ElasticsearchService elasticsearchService, UserService userService)
        {
            _minioClient = new MinioClient()
                .WithEndpoint(Env.GetString("MINIO_ADDRESS"))
                .WithCredentials(Env.GetString("MINIO_ROOT_ACCESS_KEY"), Env.GetString("MINIO_ROOT_SECRET_KEY"))
                .Build() as MinioClient;
            _aiProcessingService = aiProcessingService;
            _elasticsearchService = elasticsearchService;
            _userService = userService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var bucketName = "images";
            var uniqueIdentifier = Guid.NewGuid().ToString();
            var objectName = $"{uniqueIdentifier}_{file.FileName}";

            var jwtToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var user = await _userService.GetUserFromTokenAsync(jwtToken);
            try
            {
                using (var stream = file.OpenReadStream())
                {
                    await _minioClient.PutObjectAsync(new PutObjectArgs()
                        .WithBucket(bucketName)
                        .WithObject(objectName)
                        .WithStreamData(stream)
                        .WithObjectSize(stream.Length)
                        .WithContentType(file.ContentType));
                }
                // var result = await _aiProcessingService.ProcessImageAsync(file);
                var result = "The image features a graphic design representing a logo for a photography-related application named \"Snappi.\" The logo is characterized by a vibrant and colorful aesthetic, blending shades of blue, purple, and turquoise. \n\n**Description Elements:**\n\n1. **Shape and Structure**: \n   - The logo is enclosed within a rounded rectangular shape that has a slight gradient background.\n   - A prominent circular camera lens occupies the center of the logo, surrounded by a circular border.\n\n2. **Camera Lens Detail**: \n   - The lens is designed with multiple concentric circles, creating depth, and includes highlights that suggest a reflective quality.\n   - A white gleam at the top-left of the lens adds a realistic touch, mimicking light reflection.\n\n3. **Text Element**: \n   - The word \"Snappi\" is displayed prominently at the bottom of the logo in a bold, rounded font.\n   - The text is colored in a bright cyan hue, enhancing visibility and contrasting well with the darker background.\n\n4. **Color Scheme**:\n   - A vibrant palette featuring shades of blue, purple, and hints of turquoise creates a modern and engaging look.\n   - The overall combination of colors and shapes conveys a sense of creativity and technology suitable for a photography application.\n\n5. **Background**:\n   - The background has a soft gradient transitioning from light to darker shades, complementing the logo and increasing its visual appeal.\n\nThis detailed description provides a comprehensive overview of the logo's design and aesthetic, making it suitable for indexing and searching within an ElasticSearch database.";
                var request = new ImageIndexRequest
                {
                    description = result,
                    userId = user.id.ToString(),
                    imageName = objectName,
                    createdAt = DateTime.Now,
                    updatedAt = DateTime.Now
                };

                // Elastic search
                await _elasticsearchService.IndexImageDataAsync(request);
                return Ok("File uploaded successfully.");
            }
            catch (MinioException e)
            {
                return StatusCode(500, $"Error occurred: {e.Message}");
            }
        }
    }
