using Microsoft.AspNetCore.Mvc;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;
using DotNetEnv;


    [ApiController]
    [Route("api/[controller]")]
    public class ImageController : ControllerBase
    {
        private readonly MinioClient _minioClient;

        public ImageController()
        {
            _minioClient = new MinioClient()
                .WithEndpoint(Env.GetString("MINIO_ADDRESS"))
                .WithCredentials(Env.GetString("MINIO_ROOT_ACCESS_KEY"), Env.GetString("MINIO_ROOT_SECRET_KEY"))
                .Build() as MinioClient;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var bucketName = "images";
            var uniqueIdentifier = Guid.NewGuid().ToString();
            var objectName = $"{uniqueIdentifier}_{file.FileName}";

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

                return Ok("File uploaded successfully.");
            }
            catch (MinioException e)
            {
                return StatusCode(500, $"Error occurred: {e.Message}");
            }
        }
    }
