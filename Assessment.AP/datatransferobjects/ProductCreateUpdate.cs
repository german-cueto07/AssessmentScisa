namespace Assessment.AP.dtos;

public class ProductCreateUpdateDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<int> CategoryIds { get; set; } = new();
}