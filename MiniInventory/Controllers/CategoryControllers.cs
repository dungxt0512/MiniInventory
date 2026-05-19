using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniInventory.Data;
using MiniInventory.Model.DTOs.Category;

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
    }
}
