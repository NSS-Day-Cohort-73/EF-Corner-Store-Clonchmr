using System.ComponentModel.DataAnnotations;

namespace CornerStore.Models.DTOs;

public class ProductDTO
{
    public int Id { get; set; }
    [Required]
    public string ProductName { get; set; }
    public decimal Price { get; set; }
    [Required]
    public string Brand { get; set; }
    public int CategoryId { get; set; }
    public CategoryDTO Category { get; set; }
    
}