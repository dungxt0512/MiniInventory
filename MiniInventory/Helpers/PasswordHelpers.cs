namespace MiniInventory.Helpers
{
    public interface PasswordHelpers
    {
            string HashPassword(string password);
            bool VerifyPassword(string password, string passwordHash);
    }
    public class PasswordHelpersImpl : PasswordHelpers
    {
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
        public bool VerifyPassword(string password, string passwordHash)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, passwordHash);
            }
            catch
            {
                return false;
            }
        }
    }
}
