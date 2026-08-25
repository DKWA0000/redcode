public class LoginResponseDto
{
    public String accessToken{get; set;}

    public LoginResponseDto(String token)
    {
        this.accessToken = token;
    }
}