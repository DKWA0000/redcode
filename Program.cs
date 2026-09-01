using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.Sources.Clear();
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables(); 

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins( 
                "https://redcode-frontend-zev2.onrender.com" 
              ) 
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); 
    });
});

// Create Api
builder.Services.AddOpenApi();

// Add jwt
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

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            if (context.Request.Cookies.ContainsKey("Access-token"))
            {
                context.Token = context.Request.Cookies["Access-token"];
            }
            return Task.CompletedTask;
        }
    };
});

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// Create Database (PostgreSQL)
builder.Services.AddDbContext<BookListContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


// Add Services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<PersonService>();
builder.Services.AddScoped<BookService>();
builder.Services.AddScoped<QuoteService>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails(options => 
{
    options.CustomizeProblemDetails = ctx => ctx.ProblemDetails.Extensions.Remove("exception");
});

// Add Controllers
builder.Services.AddControllers();

var app = builder.Build();

// 🟢 FIX: Flyttad till absolut högsta toppen för att hantera preflight (OPTIONS) innan felhanteringen
app.UseCors("AllowAngular");

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// 🟢 NYTT: Automatisk databasuppdatering inifrån Render vid start
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<BookListContext>();
        // Denna rad kör alla väntande migrationer direkt mot databasen
        await context.Database.MigrateAsync();
        Console.WriteLine("Databasen har uppdaterats framgångsrikt via automatiska migrationer!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Ett fel uppstod vid automatisk databas-migrering: {ex.Message}");
    }
}

app.Run();
