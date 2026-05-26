using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniInventory.Data;
using MiniInventory.Model.Entities;

namespace MiniInventory.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DashboardControllers : ControllerBase
    {
        private readonly AppDbContext _context;

        public DashboardControllers(AppDbContext context)
        {
            _context = context;
        }

        // GET /api/dashboard/stats
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var now = DateTime.UtcNow;
            var firstOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            var totalStock = await _context.Products
                .Where(p => p.IsActive)
                .SumAsync(p => p.StockQuantity);

            var importThisMonth = await _context.InventoryTransactions
                .CountAsync(t => t.TransactionType == "IMPORT" && t.CreatedAt >= firstOfMonth);

            var exportThisMonth = await _context.InventoryTransactions
                .CountAsync(t => t.TransactionType == "EXPORT" && t.CreatedAt >= firstOfMonth);

            var lowStockCount = await _context.Products
                .CountAsync(p => p.IsActive && p.StockQuantity < 10);

            return Ok(new
            {
                totalStock,
                importThisMonth,
                exportThisMonth,
                lowStockCount
            });
        }

        // GET /api/dashboard/low-stock
        [HttpGet("low-stock")]
        public async Task<IActionResult> GetLowStock()
        {
            var products = await _context.Products
                .Where(p => p.IsActive && p.StockQuantity < 10)
                .Include(p => p.Category)
                .OrderBy(p => p.StockQuantity)
                .Select(p => new
                {
                    p.Id,
                    p.ProductId,
                    p.Name,
                    p.Unit,
                    p.StockQuantity,
                    CategoryName = p.Category.Name
                })
                .ToListAsync();

            return Ok(products);
        }

        // GET /api/dashboard/recent-transactions
        [HttpGet("recent-transactions")]
        public async Task<IActionResult> GetRecentTransactions()
        {
            var transactions = await _context.InventoryTransactions
                .OrderByDescending(t => t.CreatedAt)
                .Take(10)
                .Select(t => new
                {
                    t.Id,
                    t.TransactionCode,
                    t.TransactionType,
                    t.TransactionDate,
                    t.PartnerName,
                    t.CreatedAt
                })
                .ToListAsync();

            return Ok(transactions);
        }
    }
}
