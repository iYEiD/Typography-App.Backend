namespace DTO.Response{
    public class ImageIndexResponse
{
    public required string imageName { get; set; }
    public required string description { get; set; }
    public required DateTime createdAt { get; set; }
    public required DateTime updatedAt { get; set; }
}
}