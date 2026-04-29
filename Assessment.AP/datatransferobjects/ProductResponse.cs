namespace Assessment.AP.dtos;

public class ProductResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<CategoryInfoDto> Categories { get; set; } = new();
}