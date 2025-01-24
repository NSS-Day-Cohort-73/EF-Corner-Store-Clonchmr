using System.ComponentModel.DataAnnotations;

namespace CornerStore.Models.DTOs;

public class CategoryDTO
{
    public int Id { get; set; }
    [Required]
    public string CategoryName { get; set; }
}