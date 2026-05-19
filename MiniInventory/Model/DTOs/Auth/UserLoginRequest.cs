namespace MiniInventory.Model.DTOs.Auth
{
    public class UserLoginRequest
    {
        public string UserName { get; set; } = "";
        public string Password { get; set; } = "";
    }
}
