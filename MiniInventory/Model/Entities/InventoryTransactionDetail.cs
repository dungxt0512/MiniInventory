using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniInventory.Model.Entities
{
    public class InventoryTransactionDetail
    {
        public int Id { get; set; }
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "So luong phai lon hon 0")]
        public int Quantity { get; set; }
        [Required]
        [Range(0, double.MaxValue)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }
        [Required]
        public int TransactionId { get; set; }
        [ForeignKey(nameof(TransactionId))]
        public InventoryTransaction Transaction { get; set; }
        [Required]
        public int ProductId { get; set; }
        [ForeignKey(nameof(ProductId))]
        public Product Product { get; set; }
    }
}
