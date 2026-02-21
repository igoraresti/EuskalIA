using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EuskalIA.Server.Data;
using EuskalIA.Server.Models;
using EuskalIA.Server.DTOs;
using EuskalIA.Server.Services;
using System.Linq;

namespace EuskalIA.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IEncryptionService _encryptionService;
        private readonly IEmailService _emailService;

        public UsersController(AppDbContext context, IEncryptionService encryptionService, IEmailService emailService)
        {
            _context = context;
            _encryptionService = encryptionService;
            _emailService = emailService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            // Decrypt for response
            return new User 
            {
                Id = user.Id,
                Username = user.Username,
                Nickname = _encryptionService.Decrypt(user.Nickname),
                Email = _encryptionService.Decrypt(user.Email),
                JoinedAt = user.JoinedAt,
                Language = user.Language
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<User>> Login([FromBody] LoginDto loginDto)
        {
            // Naive implementation: fetch all users and check (since we need to decrypt to check)
            // In a real app, we would hash passwords one-way and query by hash or username.
            // Given the current encryption service, we can encrypt the input username to find the user?
            // No, the username is plain text in the model, but password/email/nickname are encrypted.
            
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == loginDto.Username);
            if (user == null) return Unauthorized(new { message = "Usuario o contraseña incorrectos" });

            var decryptedPassword = _encryptionService.Decrypt(user.Password);
            if (decryptedPassword != loginDto.Password) 
                return Unauthorized(new { message = "Usuario o contraseña incorrectos" });

            // Check if user is verified
            if (!user.IsVerified)
                return Unauthorized(new { message = "Por favor verifica tu correo electrónico antes de iniciar sesión" });

            // Check if user is active
            if (!user.IsActive)
                return Unauthorized(new { message = "Esta cuenta ha sido desactivada." });

            // Return user details similar to GetUser
            return Ok(new User 
            {
                Id = user.Id,
                Username = user.Username,
                Nickname = _encryptionService.Decrypt(user.Nickname),
                Email = _encryptionService.Decrypt(user.Email),
                JoinedAt = user.JoinedAt,
                Language = user.Language
            });
        }

        [HttpGet("{id}/progress")]
        public async Task<ActionResult<object>> GetProgress(int id)
        {
            var user = await _context.Users.Include(u => u.Progress).FirstOrDefaultAsync(u => u.Id == id);
            
            // Auto-create dummy user 1 if missing (legacy logic preserved)
            if (user == null && id == 1)
            {
                user = new User 
                { 
                    Username = "Igor Aresti", 
                    Nickname = _encryptionService.Encrypt("igoraresti"),
                    Email = _encryptionService.Encrypt("igor@euskalia.eus"),
                    Password = _encryptionService.Encrypt("1234"),
                    JoinedAt = DateTime.UtcNow.AddMonths(-2)
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }
            else if (user == null)
            {
                return NotFound();
            }

            if (user.Progress == null)
            {
                var progress = new Progress 
                { 
                    UserId = user.Id, 
                    XP = 0, 
                    Level = 1, 
                    Streak = 0, 
                    Txanponak = 100,
                    LastLessonDate = DateTime.Now.AddDays(-1),
                    LastLessonTitle = "Saludos"
                };
                _context.Progresses.Add(progress);
                await _context.SaveChangesAsync();
                user.Progress = progress;
            }

            // Get Lesson Progresses
            var lessonProgresses = await _context.LessonProgresses
                .Where(lp => lp.UserId == user.Id)
                .Select(lp => new 
                { 
                    lp.LessonId,
                    LessonTitle = _context.Lessons.Where(l => l.Id == lp.LessonId).Select(l => l.Title).FirstOrDefault() ?? "Lección",
                    lp.CorrectAnswers,
                    lp.TotalQuestions
                })
                .ToListAsync();

            return Ok(new 
            {
                user.Progress,
                LessonScores = lessonProgresses
            });
        }

        [HttpPost("{id}/xp")]
        public async Task<IActionResult> AddXP(int id, [FromBody] XPUpdateDto update)
        {
            var progress = await _context.Progresses.FirstOrDefaultAsync(p => p.UserId == id);
            if (progress == null) return NotFound();

            progress.XP += update.XP;
            progress.WeeklyXP += update.XP;
            progress.MonthlyXP += update.XP;
            progress.LastLessonDate = DateTime.Now;
            progress.LastLessonTitle = update.LessonTitle ?? progress.LastLessonTitle;

            // Simple level logic
            progress.Level = (progress.XP / 1000) + 1;
            
            await _context.SaveChangesAsync();
            return Ok(progress);
        }

        [HttpPut("{id}/profile")]
        public async Task<IActionResult> UpdateProfile(int id, [FromBody] ProfileUpdateDto profile)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            if (!string.IsNullOrEmpty(profile.Username)) user.Username = profile.Username;
            if (!string.IsNullOrEmpty(profile.Email)) user.Email = _encryptionService.Encrypt(profile.Email);
            if (!string.IsNullOrEmpty(profile.Nickname)) user.Nickname = _encryptionService.Encrypt(profile.Nickname);
            if (!string.IsNullOrEmpty(profile.Password)) user.Password = _encryptionService.Encrypt(profile.Password);

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Perfil actualizado con éxito" });
        }

        [HttpPost("{id}/request-deletion")]
        public async Task<IActionResult> RequestDeletion(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            var random = new Random();
            var code = random.Next(100000, 999999).ToString();
            user.DeletionCode = code;
            user.CodeExpiration = DateTime.UtcNow.AddMinutes(15);
            await _context.SaveChangesAsync();

            // Simulate sending email by logging to console
            Console.WriteLine("**************************************************");
            Console.WriteLine($"[EMAIL SIMULATION] To: {user.Email}");
            Console.WriteLine($"[EMAIL SIMULATION] Subject: Confirm Account Deletion");
            Console.WriteLine($"[EMAIL SIMULATION] Your verification code is: {code}");
            Console.WriteLine("**************************************************");

            return Ok(new { message = "Verification code generated and sent." });
        }

        [HttpPost("{id}/request-deactivation")]
        public async Task<IActionResult> RequestDeactivation(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            var token = Guid.NewGuid().ToString();
            user.DeactivationToken = token;
            user.DeactivationTokenExpiration = DateTime.UtcNow.AddHours(24);
            await _context.SaveChangesAsync();

            var decryptedEmail = _encryptionService.Decrypt(user.Email);
            await _emailService.SendDeactivationEmailAsync(decryptedEmail, user.Username, token);

            return Ok(new { message = "Solicitud de desactivación enviada. Revisa tu correo electrónico." });
        }

        [HttpGet("confirm-deactivation")]
        public async Task<IActionResult> ConfirmDeactivation([FromQuery] string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return BadRequest("Token inválido");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.DeactivationToken == token);
            
            if (user == null || user.DeactivationTokenExpiration < DateTime.UtcNow)
                return BadRequest("Token inválido o expirado");

            // Logical deletion: deactivate account
            user.IsActive = false;
            user.DeactivationToken = null;
            user.DeactivationTokenExpiration = null;
            
            await _context.SaveChangesAsync();

            // Redirect to a success page or login
            return Redirect("http://localhost:8081/login?deactivated=true");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAccount(int id, [FromQuery] string? code)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            if (string.IsNullOrEmpty(code) || user.DeletionCode != code || user.CodeExpiration < DateTime.UtcNow)
            {
                return BadRequest(new { message = "Invalid or expired verification code." });
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Account deleted successfully." });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(registerDto.Username) || 
                string.IsNullOrWhiteSpace(registerDto.Email) || 
                string.IsNullOrWhiteSpace(registerDto.Password))
            {
                return BadRequest(new { message = "Todos los campos son obligatorios" });
            }

            // Validate email format
            if (!registerDto.Email.Contains("@") || !registerDto.Email.Contains("."))
            {
                return BadRequest(new { message = "Formato de email inválido" });
            }

            // Validate password length
            if (registerDto.Password.Length < 6)
            {
                return BadRequest(new { message = "La contraseña debe tener al menos 6 caracteres" });
            }

            // Check if username already exists
            var existingUsername = await _context.Users.FirstOrDefaultAsync(u => u.Username == registerDto.Username);
            if (existingUsername != null)
            {
                return BadRequest(new { message = "El nombre de usuario ya está en uso" });
            }

            // Check if email already exists (need to check encrypted emails)
            var allUsers = await _context.Users.ToListAsync();
            var emailExists = allUsers.Any(u => _encryptionService.Decrypt(u.Email) == registerDto.Email);
            if (emailExists)
            {
                return BadRequest(new { message = "El correo electrónico ya está registrado" });
            }

            // Generate verification token
            var verificationToken = Guid.NewGuid().ToString();

            // Create new user
            var newUser = new User
            {
                Username = registerDto.Username,
                Nickname = _encryptionService.Encrypt(registerDto.Username), // Default nickname to username
                Email = _encryptionService.Encrypt(registerDto.Email),
                Password = _encryptionService.Encrypt(registerDto.Password),
                JoinedAt = DateTime.UtcNow,
                IsVerified = false,
                VerificationToken = verificationToken,
                TokenExpiration = DateTime.UtcNow.AddHours(24),
                Language = registerDto.Language ?? "es" // Use provided language or default to Spanish
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            // Send verification email
            await _emailService.SendVerificationEmailAsync(registerDto.Email, registerDto.Username, verificationToken, registerDto.Language ?? "es");

            return Ok(new { message = "Registro exitoso. Por favor verifica tu correo electrónico." });
        }

        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromQuery] string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return BadRequest("Token inválido");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.VerificationToken == token);
            
            if (user == null)
            {
                return BadRequest("Token inválido");
            }

            if (user.TokenExpiration < DateTime.UtcNow)
            {
                return BadRequest("El token ha expirado");
            }

            // Mark user as verified
            user.IsVerified = true;
            user.VerificationToken = null;
            user.TokenExpiration = null;
            
            await _context.SaveChangesAsync();

            // Redirect to frontend success page
            return Redirect($"http://localhost:8081/registro-exitoso?lng={user.Language}");
        }

        [HttpPut("{id}/language")]
        public async Task<IActionResult> UpdateLanguage(int id, [FromBody] UpdateLanguageDto updateLanguageDto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "Usuario no encontrado" });
            }

            // Validate language
            var validLanguages = new[] { "es", "en", "pl", "eu", "fr" };
            if (!validLanguages.Contains(updateLanguageDto.Language))
            {
                return BadRequest(new { message = "Idioma no válido" });
            }

            user.Language = updateLanguageDto.Language;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Idioma actualizado correctamente", language = user.Language });
        }
    }
}
