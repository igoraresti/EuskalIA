using EuskalIA.Server.Data;
using EuskalIA.Server.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// SQLite DB
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=euskalia.db"));

// AI Service
builder.Services.AddScoped<IAIService, MockAIService>();
builder.Services.AddScoped<IEncryptionService, EncryptionService>();
builder.Services.AddScoped<IEmailService, EmailService>();

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
            IsVerified = true, // Existing user is pre-verified
            Language = "es" // Default language
        };
        db.Users.Add(user);
        db.SaveChanges();
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowAll");
// app.UseHttpsRedirection(); // Disabled for initial public IP testing without SSL
app.UseAuthorization();
app.MapControllers();

// Bind to all interfaces (0.0.0.0) so it's accessible externally
app.Run("http://0.0.0.0:5235");
