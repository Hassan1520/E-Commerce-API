namespace ECommerce.Application.DTOs.Payments;

public class WebhookEventResultDto
{
    public string Type { get; set; } = string.Empty;
    public string? PaymentIntentId { get; set; }
}