using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniInventory.Data;
using MiniInventory.Model.DTOs.Product;
using MiniInventory.Model.Entities;

namespace MiniInventory.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductControllers : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductControllers(AppDbContext context)
        {
            _context = context;
        }

        // GET /api/product
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var products = await _context.Products
                .Where(p => p.IsActive)
                .Include(p => p.Category)
                .Select(p => new ProductsDtos
                {
                    Id = p.Id,
                    ProductId = p.ProductId,
                    Name = p.Name,
                    Unit = p.Unit,
                    StockQuantity = p.StockQuantity,
                    CostPrice = p.CostPrice,
                    SalePrice = p.SalePrice,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    IsActive = p.IsActive
                })
                .OrderBy(p => p.Name)
                .ToListAsync();

            if (products.Count == 0)
                return Ok(new { message = "Chưa có sản phẩm nào.", data = products });

            return Ok(products);
        }

        // GET /api/product/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _context.Products
                .Where(p => p.Id == id && p.IsActive)
                .Include(p => p.Category)
                .FirstOrDefaultAsync();

            if (product == null)
                return NotFound(new { message = "Không tìm thấy sản phẩm." });

            return Ok(new ProductsDtos
            {
                Id = product.Id,
                ProductId = product.ProductId,
                Name = product.Name,
                Unit = product.Unit,
                StockQuantity = product.StockQuantity,
                CostPrice = product.CostPrice,
                SalePrice = product.SalePrice,
                CategoryId = product.CategoryId,
                CategoryName = product.Category.Name,
                IsActive = product.IsActive
            });
        }

        // GET /api/product/available-for-outbound
        [HttpGet("available-for-outbound")]
        public async Task<IActionResult> GetAvailableForOutbound()
        {
            var products = await _context.Products
                .Where(p => p.IsActive && p.StockQuantity > 0)
                .Include(p => p.Category)
                .Select(p => new ProductsDtos
                {
                    Id = p.Id,
                    ProductId = p.ProductId,
                    Name = p.Name,
                    Unit = p.Unit,
                    StockQuantity = p.StockQuantity,
                    CostPrice = p.CostPrice,
                    SalePrice = p.SalePrice,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    IsActive = p.IsActive
                })
                .OrderBy(p => p.Name)
                .ToListAsync();

            return Ok(products);
        }

        // POST /api/product
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProductDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check mã SP trùng
            var exists = await _context.Products.AnyAsync(p => p.ProductId == request.ProductId);
            if (exists)
                return BadRequest(new { message = $"Mã sản phẩm '{request.ProductId}' đã tồn tại." });

            // Check category có tồn tại
            var category = await _context.Categories.FindAsync(request.CategoryId);
            if (category == null)
                return BadRequest(new { message = "Danh mục không tồn tại." });

            var product = new Product
            {
                ProductId = request.ProductId.Trim(),
                Name = request.Name.Trim(),
                CategoryId = request.CategoryId,
                CostPrice = request.CostPrice,
                SalePrice = request.SalePrice,
                Unit = request.Unit.Trim(),
                StockQuantity = 0,  // Mặc định 0
                IsActive = true
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Tạo sản phẩm thành công.", id = product.Id });
        }

        // PUT /api/product/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound(new { message = "Không tìm thấy sản phẩm." });

            // Check category có tồn tại
            var category = await _context.Categories.FindAsync(request.CategoryId);
            if (category == null)
                return BadRequest(new { message = "Danh mục không tồn tại." });

            product.Name = request.Name.Trim();
            product.CategoryId = request.CategoryId;
            product.CostPrice = request.CostPrice;
            product.SalePrice = request.SalePrice;
            product.Unit = request.Unit.Trim();
            product.IsActive = request.IsActive;
            // KHÔNG cập nhật StockQuantity - chỉ thay đổi qua phiếu nhập/xuất!

            await _context.SaveChangesAsync();

            return Ok(new { message = "Cập nhật sản phẩm thành công." });
        }

        // DELETE /api/product/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound(new { message = "Không tìm thấy sản phẩm." });

            // Check có trong phiếu không
            var inTransaction = await _context.InventoryTransactionDetails
                .AnyAsync(d => d.ProductId == id);

            if (inTransaction)
                return BadRequest(new { message = "Không thể xóa sản phẩm đã có trong phiếu." });

            // Soft delete
            product.IsActive = false;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Xóa sản phẩm thành công." });
        }
    }
}
