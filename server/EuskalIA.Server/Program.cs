using EuskalIA.Server.Data;
using EuskalIA.Server.Services.Email;
using EuskalIA.Server.Services.AI;
using EuskalIA.Server.Services.Encryption;
using EuskalIA.Server.Services.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
    });
builder.Services.AddOpenApi();

// SQLite DB
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=euskalia.db"));

// Domain Services
builder.Services.AddScoped<IAIService, MockAIService>();
builder.Services.AddScoped<IEncryptionService, EncryptionService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddHttpClient<ISocialAuthService, SocialAuthService>();

// Email Infrastructure
builder.Services.AddSingleton<IEmailQueue, EmailQueue>();
builder.Services.AddHostedService<EmailBackgroundSender>();
builder.Services.AddScoped<IEmailService, EmailService>();

// Localization
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

// JWT Authentication
var jwtSecret = builder.Configuration["JwtSettings:Secret"]!;
var jwtIssuer = builder.Configuration["JwtSettings:Issuer"]!;
var jwtAudience = builder.Configuration["JwtSettings:Audience"]!;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
    };
});

builder.Services.AddAuthorization();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Localization Middleware
var supportedCultures = new[] { "es", "en", "eu", "pl", "fr" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture("es")
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

app.UseRequestLocalization(localizationOptions);

// Auto-migrate on startup (for easy deployment)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var encryptionService = scope.ServiceProvider.GetRequiredService<IEncryptionService>();
    db.Database.EnsureCreated();

    if (!db.Users.Any())
    {
        var user = new EuskalIA.Server.Models.User
        {
            Username = "igoraresti",
            Nickname = encryptionService.Encrypt("igoraresti"),
            Email = encryptionService.Encrypt("igor@euskalia.eus"),
            Password = encryptionService.Encrypt("1234"),
            JoinedAt = DateTime.UtcNow.AddMonths(-2),
            IsVerified = true,
            Language = "es",
            Role = "Admin"
        };
        db.Users.Add(user);
        db.SaveChanges();
    }
    else
    {
        // Migration: ensure igoraresti has Admin role
        var adminUser = db.Users.FirstOrDefault(u => u.Username == "igoraresti");
        if (adminUser != null && adminUser.Role != "Admin")
        {
            adminUser.Role = "Admin";
            db.SaveChanges();
        }
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowAll");
// app.UseHttpsRedirection(); // Disabled for initial public IP testing without SSL
app.UseAuthentication(); // Must be before UseAuthorization
app.UseAuthorization();
app.MapControllers();

// Bind to all interfaces (0.0.0.0) so it's accessible externally
app.Run("http://0.0.0.0:5235");
