namespace ECommerce.Application.DTOs.Cart;

public class CartDto
{
    public List<CartItemDto> Items { get; set; } = [];
    public decimal TotalPrice { get; set; }
    public int TotalItems { get; set; }
}
