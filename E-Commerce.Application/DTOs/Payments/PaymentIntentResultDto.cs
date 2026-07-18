namespace ECommerce.Application.DTOs.Payments;

public class PaymentIntentResultDto
{
    public string Id { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
}