namespace MiniInventory.Model.DTOs.Product
{
    public class CreateProductDto
    {
        public string ProductId { get; set; } = "";
        public string Name { get; set; } = "";
        public decimal CostPrice { get; set; }
        public decimal SalePrice { get; set; }
        public string Unit { get; set; } = "";
        public int CategoryId { get; set; }
    }
}
