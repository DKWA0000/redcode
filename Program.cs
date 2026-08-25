using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Create Api
builder.Services.AddOpenApi();

// Lägg till jwt
var jwtKey = "EnSäkerOchVäldigtLångNyckelHärSomÄrMinst32Tecken!";
var key = Encoding.ASCII.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; 
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,   
        ValidateAudience = false, 
        ClockSkew = TimeSpan.Zero 
    };

    // ---- HÄR ÄR FIXEN: Säg till .NET att hämta token från din Cookie ----
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // Kontrollera om cookien "Access-token" finns i anropet
            if (context.Request.Cookies.ContainsKey("Access-token"))
            {
                context.Token = context.Request.Cookies["Access-token"];
            }
            return Task.CompletedTask;
        }
    };
    // --------------------------------------------------------------------
});

// Create Database
builder.Services.AddDbContext<BookListContext>(options =>
    options.UseInMemoryDatabase("BookListDatabase"));

// Add Services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<PersonService>();
builder.Services.AddScoped<BookService>();
builder.Services.AddScoped<QuoteService>();

// 🟢 1. REGISTRERA FELHANTERAREN HÄR
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
// Dölj stacktrace i ProblemDetails även om vi kör lokalt (Development)
builder.Services.AddProblemDetails(options => 
{
    options.CustomizeProblemDetails = ctx => ctx.ProblemDetails.Extensions.Remove("exception");
});

// Add Controllers
builder.Services.AddControllers();

var app = builder.Build();

// 🟢 2. AKTIVERA DIN GLOBAL EXCEPTION HANDLER HÄR (Ska ligga först i pipelinen!)
app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
