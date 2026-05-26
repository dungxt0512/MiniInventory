using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniInventory.Data;
using MiniInventory.Model.DTOs.Transaction;
using MiniInventory.Model.Entities;

namespace MiniInventory.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class InventoryTransactionControllers : ControllerBase
    {
        private readonly AppDbContext _context;

        public InventoryTransactionControllers(AppDbContext context)
        {
            _context = context;
        }

        // GET /api/inventory-transaction
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var transactions = await _context.InventoryTransactions
                .Include(t => t.Details)
                .ThenInclude(d => d.Product)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new TransactionDto
                {
                    Id = t.Id,
                    TransactionCode = t.TransactionCode ?? "",
                    TransactionType = t.TransactionType,
                    TransactionDate = t.TransactionDate,
                    PartnerName = t.PartnerName,
                    Note = t.Note,
                    CreatedAt = t.CreatedAt,
                    Details = t.Details.Select(d => new TransactionDetailDto
                    {
                        Id = d.Id,
                        ProductId = d.ProductId,
                        ProductName = d.Product.Name,
                        Quantity = d.Quantity,
                        UnitPrice = d.UnitPrice
                    }).ToList()
                })
                .ToListAsync();

            return Ok(transactions);
        }

        // GET /api/inventory-transaction/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var transaction = await _context.InventoryTransactions
                .Where(t => t.Id == id)
                .Include(t => t.Details)
                .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync();

            if (transaction == null)
                return NotFound(new { message = "Không tìm thấy phiếu." });

            return Ok(new TransactionDto
            {
                Id = transaction.Id,
                TransactionCode = transaction.TransactionCode ?? "",
                TransactionType = transaction.TransactionType,
                TransactionDate = transaction.TransactionDate,
                PartnerName = transaction.PartnerName,
                Note = transaction.Note,
                CreatedAt = transaction.CreatedAt,
                Details = transaction.Details.Select(d => new TransactionDetailDto
                {
                    Id = d.Id,
                    ProductId = d.ProductId,
                    ProductName = d.Product.Name,
                    Quantity = d.Quantity,
                    UnitPrice = d.UnitPrice
                }).ToList()
            });
        }

        // POST /api/inventory-transaction
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTransaction request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Validate phiếu phải có ít nhất 1 dòng chi tiết
            if (request.Details == null || request.Details.Count == 0)
                return BadRequest(new { message = "Phiếu phải có ít nhất 1 sản phẩm." });

            // Validate tồn kho nếu là phiếu xuất
            if (request.TransactionType == "EXPORT")
            {
                foreach (var detail in request.Details)
                {
                    var product = await _context.Products.FindAsync(detail.ProductId);
                    if (product == null)
                        return BadRequest(new { message = $"Sản phẩm {detail.ProductId} không tồn tại." });

                    if (product.StockQuantity < detail.Quantity)
                        return BadRequest(new
                        {
                            message = $"'{product.Name}' chỉ còn {product.StockQuantity}, " +
                                    $"không đủ để xuất {detail.Quantity}."
                        });
                }
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Sinh TransactionCode
                    var transactionCode = await GenerateTransactionCode(request.TransactionType);

                    var inventoryTx = new InventoryTransaction
                    {
                        TransactionCode = transactionCode,
                        TransactionType = request.TransactionType,
                        TransactionDate = request.TransactionDate,
                        PartnerName = request.PartnerName,
                        Note = request.Note,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.InventoryTransactions.Add(inventoryTx);
                    await _context.SaveChangesAsync();

                    // Thêm chi tiết và cộng/trừ tồn kho
                    foreach (var detail in request.Details)
                    {
                        var product = await _context.Products.FindAsync(detail.ProductId);

                        if (request.TransactionType == "EXPORT")
                            product.StockQuantity -= detail.Quantity;
                        else // IMPORT
                            product.StockQuantity += detail.Quantity;

                        var txDetail = new InventoryTransactionDetail
                        {
                            TransactionId = inventoryTx.Id,
                            ProductId = detail.ProductId,
                            Quantity = detail.Quantity,
                            UnitPrice = detail.UnitPrice
                        };

                        _context.InventoryTransactionDetails.Add(txDetail);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Ok(new
                    {
                        message = "Tạo phiếu thành công.",
                        transactionCode = transactionCode,
                        id = inventoryTx.Id
                    });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new { message = $"Lỗi: {ex.Message}" });
                }
            }
        }

        // DELETE /api/inventory-transaction/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var transaction = await _context.InventoryTransactions
                .Include(t => t.Details)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (transaction == null)
                return NotFound(new { message = "Không tìm thấy phiếu." });

            using (var tx = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Hoàn lại tồn kho nếu có details
                    foreach (var detail in transaction.Details)
                    {
                        var product = await _context.Products.FindAsync(detail.ProductId);

                        if (transaction.TransactionType == "EXPORT")
                            product.StockQuantity += detail.Quantity;  // Hoàn lại
                        else // IMPORT
                            product.StockQuantity -= detail.Quantity;  // Hoàn lại
                    }

                    // Xóa phiếu (cascade sẽ xóa details)
                    _context.InventoryTransactions.Remove(transaction);
                    await _context.SaveChangesAsync();
                    await tx.CommitAsync();

                    return Ok(new { message = "Xóa phiếu thành công." });
                }
                catch (Exception ex)
                {
                    await tx.RollbackAsync();
                    return BadRequest(new { message = $"Lỗi: {ex.Message}" });
                }
            }
        }

        // Helper: Sinh TransactionCode
        private async Task<string> GenerateTransactionCode(string type)
        {
            var prefix = type == "IMPORT" ? "IN" : "OUT";
            var dateString = DateTime.Now.ToString("yyyyMMdd");

            var todayCount = await _context.InventoryTransactions
                .Where(x => x.CreatedAt.Date == DateTime.Now.Date && x.TransactionType == type)
                .CountAsync();

            var sequence = (todayCount + 1).ToString("D3");
            return $"{prefix}-{dateString}-{sequence}";
        }
    }
}
