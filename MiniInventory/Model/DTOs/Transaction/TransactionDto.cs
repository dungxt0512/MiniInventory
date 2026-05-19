namespace MiniInventory.Model.DTOs.Transaction
{
    public class TransactionDto
    {
        public int Id { get; set; }
        public string TransactionType { get; set; } = "";
        public DateTime TransactionDate { get; set; }
        public string? PartnerName { get; set; }
        public string Note { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public List<TransactionDetailDto> Details { get; set; } = [];
    }
}
