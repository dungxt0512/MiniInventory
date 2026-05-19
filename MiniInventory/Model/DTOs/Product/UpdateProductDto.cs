namespace MiniInventory.Model.DTOs.Product
{
    public class UpdateProductDto
    {
        public int Id { get; set; }
        public string ProductId { get; set; } = "";
        public string Name { get; set; } = "";
        public decimal CostPrice { get; set; }
        public decimal SalePrice { get; set; }
        public string Unit { get; set; } = "";
        public bool IsActive { get; set; }
        public int CategoryId { get; set; }
    }
}
