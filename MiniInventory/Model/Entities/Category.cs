using System.ComponentModel.DataAnnotations;

namespace MiniInventory.Model.Entities
{
    public class Category
    {
        public int CategoryId { get; set; }
        [Required(ErrorMessage ="Ten danh muc khong duoc de trong")]
        [StringLength(100, MinimumLength = 1, ErrorMessage ="Ten danh muc phai tu 1-100 ki tu")]
        public string Name { get; set; } = "";
        [StringLength(500)]
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        [System.ComponentModel.DataAnnotations.Schema.InverseProperty("Category")]
        public ICollection<Product> Products { get; set; } = [];
    }
}
