using System;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;

public class JwtUtil
{

    private String jwtKey = "EnSäkerOchVäldigtLångNyckelHärSomÄrMinst32Tecken!";

 public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        
        
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
        }
        
        return Convert.ToBase64String(randomNumber);
    }

    public string GenerateAccessToken(Person person)
{
    var tokenHandler = new JwtSecurityTokenHandler();
    var key = Encoding.ASCII.GetBytes(jwtKey);

    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, person.Id.ToString()),
            new Claim(ClaimTypes.Email, person.email)
        }),

        Expires = DateTime.UtcNow.AddMinutes(30), // Token är giltig i 30 minuter
        SigningCredentials = new SigningCredentials(
            new SymmetricSecurityKey(key),
             SecurityAlgorithms.HmacSha256Signature)
    };

    var token = tokenHandler.CreateToken(tokenDescriptor);
    return tokenHandler.WriteToken(token);
}

    public ClaimsPrincipal? getClaimsFromToken(string token) // Ändrat returtyp till nullable (?) för modern C#
{
    var tokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtKey)), // 💡 Dubbelkolla att du inte använde Encoding.UTF8 vid genereringen!
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = false, 
        ClockSkew = TimeSpan.Zero
    };

    var tokenHandler = new JwtSecurityTokenHandler();
    
    try
    {
        // ValidateToken läser ut alla claims och lägger dem i ett ClaimsPrincipal
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
        
        // 🌟 ÄNDRING 1: Vi mjukar upp algoritms-kollen lite eller tar bort den tillfälligt under testet, 
        // då utgångna tokens ibland kan ändra hur headern tolkas i .NET Core.
        if (securityToken is JwtSecurityToken jwtSecurityToken)
        {
            var alg = jwtSecurityToken.Header.Alg;
            if (!alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase) &&
                !alg.Equals("HS256", StringComparison.InvariantCultureIgnoreCase)) // .NET använder ibland "HS256" kortform
            {
                Console.WriteLine($"[JWT WARN] Ogiltig algoritm hittades: {alg}");
                return null;
            }
        }
        else
        {
            Console.WriteLine("[JWT WARN] securityToken var inte en giltig JwtSecurityToken.");
            return null;
        }

        return principal;
    }
    catch (Exception ex)
    {
        // 🌟 ÄNDRING 2: Skriv ut det exakta undantaget i terminalen! 
        // Detta avslöjar direkt om det är fel på nyckeln, signaturen eller formatet.
        Console.WriteLine($"[JWT ERROR] Sökning i getClaimsFromToken misslyckades: {ex.Message}");
        if (ex.InnerException != null)
        {
            Console.WriteLine($"[JWT INNER ERROR] {ex.InnerException.Message}");
        }
        return null; 
    }
}

}