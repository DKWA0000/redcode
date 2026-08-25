public class CreateUserDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public CreateUserDto(string email, string password)
    {
        Email = email;
        Password = password;
    }
}