using Microsoft.AspNetCore.Diagnostics; 
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc; 

public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, 
        Exception exception, 
        CancellationToken cancellationToken)
    {
        // 1. Mappa dina exceptions till Statuskod och Titel med en switch-expression
        var (statusCode, title) = exception switch
        {
            BookNotFoundException => (StatusCodes.Status404NotFound, "Book Not Found"),
            PersonNotFoundException => (StatusCodes.Status404NotFound, "User is not found"),
            QuoteNotFoundException => (StatusCodes.Status404NotFound, "Quote not found"),
            NoValidTokenException => (StatusCodes.Status404NotFound, "No valid token found"),
            
            InvalidQuoteOperationException => (StatusCodes.Status401Unauthorized, "Unauthorized quote operation"),
            
            ToManyQuotesException => (StatusCodes.Status400BadRequest, "To many quotes"),
            EmailAlreadyInUseException => (StatusCodes.Status400BadRequest, "Email already in use"),
            InvalidPasswordException => (StatusCodes.Status400BadRequest, "Invalid password"),
            TokenExpiredException => (StatusCodes.Status400BadRequest, "Token expired"),
            
            _ => (0, null) // Om det är ett okänt fel som vi inte vill hantera här
        };

        // 2. Om felet inte matchade något ovan, låt .NET hantera det (returnera false)
        if (statusCode == 0)
        {
            return false;
        }

        // 3. Bygg upp och skicka svaret en enda gång gemensamt för alla fel
        httpContext.Response.StatusCode = statusCode;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = exception.Message
        };

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        
        return true;
    }
}
