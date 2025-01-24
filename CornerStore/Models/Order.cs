
namespace CornerStore.Models;

public class Order
{
    public int Id { get; set; }
    public int CashierId { get; set; }
    public Cashier Cashier { get; set; }
    public List<OrderProduct> OrderProducts { get; set; } = new List<OrderProduct>();
    public List<Product> Products { get; set; } = new List<Product>();
    public decimal Total 
    {
        get
        {
            return OrderProducts?.Sum(op => op.Product?.Price * op.Quantity) ?? 0;
        }
    }
    public DateTime? PaidOnDate { get; set; }
}