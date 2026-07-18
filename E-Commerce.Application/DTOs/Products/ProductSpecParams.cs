namespace ECommerce.Application.DTOs.Products
{
    public class ProductSpecParams
    {
        private const int MaxPageSize = 50; // √ﬁ’Ï ⁄œœ „‰ Ã«  „”„ÊÕ »ÿ·»Â ›Ì «·’›Õ…
        public int PageNumber { get; set; } = 1;

        private int _pageSize = 10; // «·«› —«÷Ì 10 „‰ Ã« 
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
        }

        public string? Search { get; set; } // ﬂ·„… «·»ÕÀ
        public int? CategoryId { get; set; }
    }
}
