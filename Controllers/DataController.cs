using Microsoft.AspNetCore.Mvc;
using MetadataExtractor;
using MetadataExtractor.Formats.Jpeg;
using System.Collections.Generic;
using System.IO;

namespace YourNamespace.Controllers
{
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
    }
}
