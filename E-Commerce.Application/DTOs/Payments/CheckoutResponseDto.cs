namespace ECommerce.Application.DTOs.Payments;

public class CheckoutResponseDto
{
    public int OrderId { get; set; }
    public string ClientSecret { get; set; } = string.Empty;
    public string PublishableKey { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "usd";
}