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

    public ClaimsPrincipal getClaimsFromToken(String token)
    {
          var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtKey)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false 
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        
        try
        {
            // ValidateToken läser ut alla claims och lägger dem i ett ClaimsPrincipal (user)
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
            
            // En extra säkerhetskoll: Säkerställ att det är en äkta JWT (HMAC SHA256) och inte en fejkad sträng
            if (securityToken is not JwtSecurityToken jwtSecurityToken || 
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            return principal;
        }
        catch
        {
            return null; 
        }
    }
}