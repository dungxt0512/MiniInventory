using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniInventory.Model.Entities
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage ="UserName khong duoc de trong")]
        [StringLength(50, MinimumLength = 3, ErrorMessage ="UserName phai tu 3-50 ki tu")]
        public string UserName { get; set; } = "";
        [Required]
        public string PasswordHash { get; set; } = "";
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Role { get; set; } = "Admin";
    }
}
