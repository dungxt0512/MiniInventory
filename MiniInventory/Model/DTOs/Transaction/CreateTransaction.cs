namespace MiniInventory.Model.DTOs.Transaction
{
    public class CreateTransaction
    {
        public string TransactionType { get; set; } = "";
        public DateTime TransactionDate { get; set; }
        public string PartnerName { get; set; }
        public string Note { get; set; }
        public List<CreateTransactionDetail> Details { get; set; } = [];
    }
}
