namespace MiniInventory.Model.DTOs.Transaction
{
    public class CreateTransactionDetail
    {
        public int ProductId { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
    }
}
