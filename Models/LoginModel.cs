namespace Feast.Models
{
    public class LoginModel
    {
        public string Identifier { get; set; } 
        public string Password { get; set; }
        public bool RememberMe { get; set; }
    }
}