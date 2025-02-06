namespace DTO.Request{
public class ImageIndexRequest
{
    public string? _id { get; set; }
    public required string userId { get; set; }
    public required string description { get; set; }
    public required string imageName { get; set; }
    public DateTime? createdAt { get; set; }
    public required DateTime updatedAt { get; set; }
}
}