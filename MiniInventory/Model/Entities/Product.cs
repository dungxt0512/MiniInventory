using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MiniInventory.Model.Entities
{
    public class Product
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "ProductId khong duoc de trong")]
        [StringLength(20, MinimumLength = 1)]
        public string ProductId { get; set; } = "";
        [Required(ErrorMessage = "Name khong duoc de trong")]
        [StringLength(200, MinimumLength = 1)]
        public string Name { get; set; } = "";
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Gia nhap phai lon hon 0")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal CostPrice { get; set; }
        [Required]
        [Range(0, double.MaxValue)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal SalePrice { get; set; }
        [Required]
        public string Unit { get; set; } = "";
        [Range(0, int.MaxValue, ErrorMessage = "Ton Kho khong duoc am")]
        public int StockQuantity { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public int CategoryId { get; set; }
        [ForeignKey(nameof(CategoryId))]
        public Category Category { get; set; }
    }
}
