using EuskalIA.Server.Data;
using EuskalIA.Server.Services.Email;
using EuskalIA.Server.Services.AI;
using EuskalIA.Server.Services.Encryption;
using EuskalIA.Server.Services.Auth;
using EuskalIA.Server.Services;
using EuskalIA.Server.Services.Notifications;
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

// Always use SQLite — path is configured via ConnectionStrings:DefaultConnection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=euskalia.db";

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite(connectionString);
});

// Domain Services
builder.Services.AddScoped<IAIService, MockAIService>();
builder.Services.AddScoped<IEncryptionService, EncryptionService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<ISrsService, SrsService>();
builder.Services.AddHttpClient<ISocialAuthService, SocialAuthService>();

// Email Infrastructure
builder.Services.AddSingleton<IEmailQueue, EmailQueue>();
builder.Services.AddHostedService<EmailBackgroundSender>();
builder.Services.AddScoped<IEmailService, EmailService>();

// Notifications Infrastructure
builder.Services.AddHttpClient<INotificationService, NotificationService>();
builder.Services.AddHostedService<SrsReminderService>();

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
    db.Database.Migrate();

    if (!db.Users.Any())
    {
        var adminPassword = encryptionService.Encrypt("1234");
        var user = new EuskalIA.Server.Models.User
        {
            Username = "igoraresti",
            Nickname = encryptionService.Encrypt("adminigor"),
            Email = encryptionService.Encrypt("igor@euskalia.eus"),
            Password = adminPassword,
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
    
    // Seed AIGC Sample Exercise
    EuskalIA.Server.Utils.AigcSeeder.SeedAigcExercises(db);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    app.UseHsts();
}

app.Use(async (context, next) =>
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation($"=> Incoming Request: {context.Request.Method} {context.Request.Path}");
    
    try
    {
        await next();
        logger.LogInformation($"<= Outgoing Response: {context.Response.StatusCode}");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, $"!! Unhandled Exception during request to {context.Request.Path}");
        throw;
    }
});

app.UseCors("AllowAll");
app.UseHttpsRedirection(); 
app.UseAuthentication(); // Must be before UseAuthorization
app.UseAuthorization();
app.MapControllers();

app.Run();
