namespace norviguet_control_fletes_api.Models.User
{
    public class UserDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = "Pending"; 
        public string Status { get; set; } = "Inactive"; 
    }
}
