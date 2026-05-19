using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniInventory.Model.Entities
{
    public class InventoryTransaction
    {
        public int Id { get; set; }
        [Required]
        [StringLength(20)]
        public string TransactionType { get; set; } = "";
        [Required]
        public DateTime TransactionDate { get; set; }
        [StringLength(100)]
        public string? PartnerName { get; set; }
        [StringLength(200)]
        public string Note { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<InventoryTransactionDetail> Details { get; set; } = [];
    }
}
