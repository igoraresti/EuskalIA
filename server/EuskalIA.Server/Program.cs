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

// Connection String: Prefer environment variable DB_CONNECTION_STRING (Production)
// Fallback to ConnectionStrings:DefaultConnection (Development/Docker)
var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING") 
    ?? builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Server=localhost,1433;Database=EuskalIA;User Id=sa;Password=YourStrong!Pass123;Encrypt=False";

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});

// AI & Knowledge Infrastructure
builder.Services.Configure<GeminiSettings>(builder.Configuration.GetSection("GeminiSettings"));
builder.Services.AddScoped<IKnowledgeService, KnowledgeService>();
builder.Services.AddHttpClient<GeminiAIService>();

var geminiApiKey = builder.Configuration["GeminiSettings:ApiKey"];
if (!string.IsNullOrEmpty(geminiApiKey))
{
    builder.Services.AddScoped<IAIService, GeminiAIService>();
}
else
{
    builder.Services.AddScoped<IAIService, MockAIService>();
}

builder.Services.AddScoped<IEncryptionService, EncryptionService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<ISrsService, SrsService>();
builder.Services.AddScoped<IGamificationService, GamificationService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
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

    // Seed AIGC Sample Exercise
    var seederLogger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    EuskalIA.Server.Utils.AigcSeeder.SeedAigcExercises(db, seederLogger);
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

// Automatic migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseCors("AllowAll");
app.UseHttpsRedirection(); 
app.UseAuthentication(); // Must be before UseAuthorization
app.UseAuthorization();
app.MapControllers();

app.Run();
