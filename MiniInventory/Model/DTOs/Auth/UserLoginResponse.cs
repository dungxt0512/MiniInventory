namespace MiniInventory.Model.DTOs.Auth
{
    public class UserLoginResponse
    {
        public string Token { get; set; } = "";
        public string UserName { get; set; } = "";
        public string Role { get; set; } = "";
    }
}
