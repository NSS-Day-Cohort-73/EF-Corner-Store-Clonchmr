
namespace CornerStore.Models.DTOs;

public class OrderDTO
{
    public int Id { get; set; }
    public int CashierId { get; set; }
    public CashierDTO Cashier { get; set; }
    public List<OrderProductDTO> OrderProducts { get; set; } = new List<OrderProductDTO>();
    public List<ProductDTO> Products { get; set; } = new List<ProductDTO>();
    public decimal Total 
    {
        get
        {
            return OrderProducts.Sum(op => op.Product.Price * op.Quantity);
        }
    }
    public DateTime? PaidOnDate { get; set; }
}