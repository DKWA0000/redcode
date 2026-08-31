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
        //Check if the provided email is already in use
        if(await context.persons.AnyAsync(e => e.email.ToLower() == dto.Email.ToLower()))
        {
            throw new EmailAlreadyInUseException("The email provided is aldready in use, please try again");
        }

        // Create new user
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
        
        if (!string.IsNullOrEmpty(refreshToken))
        {
            
            // Get the current refresh-token
            var storedToken = await context.refreshTokens
                .FirstOrDefaultAsync(t => t.token == refreshToken);

            if (storedToken != null)
            {
                // Check if token is valid(has not expired )
                double hoursLeft = await getRefreshTokenHoursLeft(refreshToken, storedToken.personId);
                
                if (hoursLeft > 0)
                {
                    // Get the personId from the stored row in the refreshtoken table
                    Person? tmp = await context.persons.FindAsync(storedToken.personId);
                    if (tmp != null)
                    {
                        // Generate a new access-token
                        string newAccessToken = util.GenerateAccessToken(tmp);
                        appendAccessCookie(newAccessToken);
                        
                        // Renew refresh-token if it expires in less than 24 hours
                        if (hoursLeft < 24)
                        {
                            await appendRefreshCookie(util.GenerateRefreshToken(), storedToken.personId);
                        }
                        return;
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
        // Get the current refresh-token from db
        RefreshToken? refreshTokenFromDb = await context.refreshTokens
            .FirstOrDefaultAsync(r => r.personId == personId && r.token == refreshToken);

        if (refreshTokenFromDb != null)
        {
            // Calculate hours left on the token
             return (refreshTokenFromDb.expires - DateTime.Now).TotalHours;    
        }
    
        throw new NoValidTokenException("Token invalid, please login again");
    }

    private async Task appendRefreshCookie(string refreshToken, int personId)
    {
        // Get the current http-context
        HttpContext? currentContext = httpContext.HttpContext;
        if (currentContext == null) return;
        await addRefreshToken(refreshToken, personId);

        // Add cookie-options for the http-only cookie
        CookieOptions refreshCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTime.UtcNow.AddDays(7)
        };

        // Append the cookie to the response
        currentContext.Response.Cookies.Append("Refresh-token", refreshToken, refreshCookieOptions);
    }

    private void appendAccessCookie(string accessToken)
    {
        // Get the current http-context
        HttpContext? currentContext = httpContext.HttpContext;
        if (currentContext == null) return;

        // Add cookie-options for the http-only cookie
        CookieOptions accessCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTime.UtcNow.AddMinutes(30) 
        };

        // Append the cookie to the response
        currentContext.Response.Cookies.Append("Access-token", accessToken, accessCookieOptions);
    }

    // Method to add the refresh-token to the database
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
