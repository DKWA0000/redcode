using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;

// 🌟 FIXAT: Skapa en helt tom builder utan dolda Linux-filbevakare (inotify)
var builder = WebApplication.CreateEmptyBuilder(new WebApplicationOptions
{
    Args = args
});

// Lägg till baskonfiguration manuellt UTAN reloadOnChange
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables(); // Laddar dina Render-miljövariabler

// 🌟 Manuell registrering av nödvändiga grundtjänster (krävs när CreateEmptyBuilder används)
builder.Services.AddRouting();
builder.Services.AddLogging(logging => logging.AddConsole());

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins(
                "http://localhost:4200", 
                "https://onrender.com" // Byt ut mot din exakta Render-URL sen
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

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BookListContext>();
    db.Database.Migrate();
}

app.Run();
