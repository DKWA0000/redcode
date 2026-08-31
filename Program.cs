using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;

// 🌟 Skapa buildern med standardinställningar
var builder = WebApplication.CreateBuilder(args);

// Stäng av live-bevakning på konfigurationsfilerna efteråt för att spara Linux-resurser
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
                "http://localhost:4200", 
                "https://redcode-frontend-zev2.onrender.com" // 🌟 Tillåt din skarpa frontend på Render
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

// 🌟 Tvingar PostgreSQL-drivrutinen att automatiskt acceptera och konvertera DateTime till UTC
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

// Activate global exception handler
app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowAngular");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// 🌟 Kör dina nya, rensade och synkade PostgreSQL-migreringar vid uppstart
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BookListContext>();
    db.Database.Migrate();
}

app.Run();
