namespace ECommerce.Domain.Entities;

public class ProductImage
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string BlobName { get; set; } = string.Empty; 
    public bool IsMain { get; set; } // «·’Ê—… «·—∆Ì”Ì… «··Ì  ŸÂ— ›Ì «·ﬂ—Ê /listing
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Product Product { get; set; } = null!;
}