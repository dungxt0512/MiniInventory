using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniInventory.Data;
using MiniInventory.Model.DTOs.Category;
using MiniInventory.Model.Entities;

namespace MiniInventory.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CategoryControllers : ControllerBase
    {
        private readonly AppDbContext _context;
        public CategoryControllers(AppDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _context.Categories.Where(c => c.IsActive).Select(c => new CategoryDtos
            {
                Id = c.CategoryId,
                Name = c.Name,
                Description = c.Description,
                IsActive = c.IsActive
            }).ToListAsync();
            return Ok(list);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return Ok(new CategoryDtos
            {
                Id = category.CategoryId,
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive
            });
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCategoryDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var category = new Category
            {
                Name = request.Name.Trim(),
                Description = request.Description,
                IsActive = true
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Tạo danh mục thành công.", id = category.CategoryId });
        }

        // PUT /api/category/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return NotFound(new { message = "Không tìm thấy danh mục." });

            category.Name = request.Name.Trim();
            category.Description = request.Description;
            category.IsActive = request.IsActive;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Cập nhật danh mục thành công." });
        }

        // DELETE /api/category/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
                return NotFound(new { message = "Không tìm thấy danh mục." });

            // Check có product không
            if (category.Products.Any())
                return BadRequest(new { message = "Không thể xóa danh mục đã có sản phẩm." });

            // Soft delete
            category.IsActive = false;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Xóa danh mục thành công." });
        }
    }
}
