namespace LibraryCoreApi.DTOs;

/// <summary>
/// CategoryDto class to represent a category
/// </summary>
public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int BookCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// CreateCategoryDto class to represent a category to be created
/// </summary>
public class CreateCategoryDto
{
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// UpdateCategoryDto class to represent a category to be updated
/// </summary>
public class UpdateCategoryDto
{
   public string Name { get; set; } = string.Empty;
}
