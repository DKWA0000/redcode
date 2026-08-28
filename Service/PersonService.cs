using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

public class PersonService
{
    private readonly BookListContext context;
    private readonly JwtUtil util;
    private readonly IHttpContextAccessor httpContext;

    public PersonService(BookListContext context, IHttpContextAccessor httpContextAccessor)
    {
        this.context = context;
        this.util = new JwtUtil();
        this.httpContext = httpContextAccessor;
    }

    public async Task createUser(CreateUserDto dto)
    {
        if (dto.Email == null)
        {
            throw new EmailAlreadyInUseException("The email provided is empty");
        }

        Person tmp = new Person();
        tmp.email = dto.Email;       
        tmp.password = dto.Password; 
        
        context.persons.Add(tmp);
        await context.SaveChangesAsync(); 
    }

    public async Task loginUser(LoginUserDto dto)
{
    Person? userToLogin = await context.persons
        .FirstOrDefaultAsync(p => p.email.ToLower() == dto.Email.ToLower());

    if (userToLogin == null)
    {
        throw new PersonNotFoundException("Invalid email, try again");
    }
    if (userToLogin.password != dto.Password)
    {
        throw new InvalidPasswordException("Invalid password, try again");
    }

        string refreshToken = util.GenerateRefreshToken();
        string accessToken = util.GenerateAccessToken(userToLogin);
    
        appendAccessCookie(accessToken);
        await appendRefreshCookie(refreshToken, userToLogin.Id);
    }   

     public async Task refreshTokens()
{
    HttpContext? currentContext = httpContext.HttpContext;

    if (currentContext != null)
    {
        String? refreshToken = currentContext.Request.Cookies["Refresh-token"];
        // 💡 Vi behöver inte längre tvinga fram Access-token!
        
        if (!string.IsNullOrEmpty(refreshToken))
        {
            // 🌟 1. Slå upp din refresh-token i databasen för att hitta vem den tillhör
            // (Justera namnet på tabellen 'refreshTokens' och fälten så de matchar din DB-kontext)
            var storedToken = await context.refreshTokens
                .FirstOrDefaultAsync(t => t.token == refreshToken);

            if (storedToken != null)
            {
                // 🌟 2. Kontrollera om din refresh-token fortfarande är giltig tidsmässigt
                double hoursLeft = await getRefreshTokenHoursLeft(refreshToken, storedToken.personId);
                
                if (hoursLeft > 0)
                {
                    // 🌟 3. Hämta personen direkt via det sparade PersonId:t i token-tabellen
                    Person? tmp = await context.persons.FindAsync(storedToken.personId);
                    if (tmp != null)
                    {
                        // Generera en helt ny fungerande Access-token
                        string newAccessToken = util.GenerateAccessToken(tmp);
                        appendAccessCookie(newAccessToken);
                        
                        // Förnya din refresh-token om den håller på att gå ut (mindre än 24 timmar kvar)
                        if (hoursLeft < 24)
                        {
                            await appendRefreshCookie(util.GenerateRefreshToken(), storedToken.personId);
                        }
                        return; // 🚀 Succé! Allt klart, avbryt metoden.
                    }
                }
                throw new TokenExpiredException("Refresh token has expired, please login again");
            }
        }
        throw new NoValidTokenException("Token is not valid, please login again");
    }
}
          

    private async Task<double> getRefreshTokenHoursLeft(String refreshToken, int personId)
    {
        RefreshToken? refreshTokenFromDb = await context.refreshTokens
            .FirstOrDefaultAsync(r => r.personId == personId && r.token == refreshToken);

        if (refreshTokenFromDb != null)
        {
             return (refreshTokenFromDb.expires - DateTime.Now).TotalHours;    
        }
    
        throw new NoValidTokenException("Token invalid, please login again");
    }

    private async Task appendRefreshCookie(string refreshToken, int personId)
    {
        HttpContext? currentContext = httpContext.HttpContext;
        if (currentContext == null) return;
        await addRefreshToken(refreshToken, personId);

        CookieOptions refreshCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTime.UtcNow.AddDays(7)
        };
        currentContext.Response.Cookies.Append("Refresh-token", refreshToken, refreshCookieOptions);
    }

    private void appendAccessCookie(string accessToken)
    {
        HttpContext? currentContext = httpContext.HttpContext;
        if (currentContext == null) return;

        CookieOptions accessCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTime.UtcNow.AddMinutes(30) 
        };

        currentContext.Response.Cookies.Append("Access-token", accessToken, accessCookieOptions);
    }

    private async Task addRefreshToken(String refreshtoken, int personId)
    {
        context.refreshTokens.Add(new RefreshToken(
             0, 
            refreshtoken,
            DateTime.UtcNow.AddDays(7),
            personId
        ));
    
        await context.SaveChangesAsync();
    }
}
