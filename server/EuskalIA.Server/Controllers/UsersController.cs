using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EuskalIA.Server.Data;
using EuskalIA.Server.Models;
using EuskalIA.Server.DTOs.Auth;
using EuskalIA.Server.DTOs.Users;
using EuskalIA.Server.DTOs.Gamification;
using EuskalIA.Server.Services.Email;
using EuskalIA.Server.Services.Encryption;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Authorization;
using EuskalIA.Server.Services.Auth;
using EuskalIA.Server.Services;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace EuskalIA.Server.Controllers
{
    /// <summary>
    /// Controller for managing user profiles, authentication, progress, and account lifecycle.
    /// </summary>
    [ApiController]
    [Route("api/euskalia/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IEncryptionService _encryptionService;
        private readonly IEmailService _emailService;
        private readonly IJwtService _jwtService;
        private readonly IStringLocalizer<UsersController> _localizer;

        private readonly IGamificationService _gamificationService;
        private readonly ILogger<UsersController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="UsersController"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="encryptionService">The service for encrypting/decrypting user data.</param>
        /// <param name="emailService">The service for sending system emails.</param>
        /// <param name="jwtService">The service for generating authentication tokens.</param>
        /// <param name="localizer">The localizer for UI strings.</param>
        /// <param name="gamificationService">The service for progress and achievement updates.</param>
        /// <param name="logger">The controller logger.</param>
        public UsersController(
            AppDbContext context, 
            IEncryptionService encryptionService, 
            IEmailService emailService, 
            IJwtService jwtService, 
            IStringLocalizer<UsersController> localizer, 
            IGamificationService gamificationService,
            ILogger<UsersController> logger)
        {
            _context = context;
            _encryptionService = encryptionService;
            _emailService = emailService;
            _jwtService = jwtService;
            _localizer = localizer;
            _gamificationService = gamificationService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves basic profile information for a specific user.
        /// Sensitve data is decrypted before being returned.
        /// </summary>
        /// <param name="id">The unique identifier of the user.</param>
        /// <returns>A <see cref="User"/> object if found; otherwise, NotFound.</returns>
        [Authorize]
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

        /// <summary>
        /// Authenticates a user with a username and password.
        /// </summary>
        /// <param name="loginDto">The login credentials.</param>
        /// <returns>An <see cref="ActionResult"/> containing the JWT token and user details if successful.</returns>
        [HttpPost("login")]
        public async Task<ActionResult<User>> Login([FromBody] LoginDto loginDto)
        {
            _logger.LogInformation("Login attempt for user {Username}.", loginDto.Username);
            // Naive implementation: fetch all users and check (since we need to decrypt to check)
            // In a real app, we would hash passwords one-way and query by hash or username.
            // Given the current encryption service, we can encrypt the input username to find the user?
            // No, the username is plain text in the model, but password/email/nickname are encrypted.
            
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == loginDto.Username);
            if (user == null) return Unauthorized(new { message = _localizer["InvalidCredentials"].Value });

            var decryptedPassword = _encryptionService.Decrypt(user.Password);
            if (decryptedPassword != loginDto.Password) 
            {
                _logger.LogWarning("Failed login for user {Username}: Invalid password.", loginDto.Username);
                return Unauthorized(new { message = _localizer["InvalidCredentials"].Value });
            }

            // Check if user is verified
            if (!user.IsVerified)
                return Unauthorized(new { message = _localizer["EmailNotVerified"].Value });

            // Check if user is active
            if (!user.IsActive)
            {
                _logger.LogWarning("Failed login for user {Username}: Account deactivated.", loginDto.Username);
                return Unauthorized(new { message = _localizer["AccountDeactivated"].Value });
            }

            _logger.LogInformation("User {Username} logged in successfully.", loginDto.Username);

            var token = _jwtService.GenerateToken(user);

            // Return user details including token and role
            return Ok(new 
            {
                Token = token,
                User = new 
                {
                    user.Id,
                    user.Username,
                    Nickname = _encryptionService.Decrypt(user.Nickname),
                    Email = _encryptionService.Decrypt(user.Email),
                    user.JoinedAt,
                    user.Language,
                    user.Role
                }
            });
        }

        /// <summary>
        /// Retrieves the comprehensive learning progress for a user, including XP, level, and lesson scores.
        /// Automatically initializes progress if it's the user's first time.
        /// </summary>
        /// <param name="id">The unique identifier of the user.</param>
        /// <returns>An object containing user progress and detailed lesson scores.</returns>
        [Authorize]
        [HttpGet("{id}/progress")]
        public async Task<ActionResult<object>> GetProgress(int id)
        {
            var user = await _context.Users.Include(u => u.Progress).FirstOrDefaultAsync(u => u.Id == id);
            
            if (user == null) return NotFound();

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

        /// <summary>
        /// Adds Experience Points (XP) to the user's progress and updates streaks and achievements.
        /// </summary>
        /// <param name="id">The unique identifier of the user.</param>
        /// <param name="update">The XP update details.</param>
        /// <returns>An <see cref="IActionResult"/> with the updated progress and newly earned achievements.</returns>
        [Authorize]
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

            // Trigger gamification updates: streak and achievement checks
            await _gamificationService.UpdateStreakAsync(id);
            var newlyEarned = await _gamificationService.CheckAchievementsAsync(id);

            return Ok(new { Progress = progress, NewlyEarned = newlyEarned });
        }

        /// <summary>
        /// Updates the user's profile information such as username, email, or password.
        /// </summary>
        /// <param name="id">The unique identifier of the user.</param>
        /// <param name="profile">The updated profile details.</param>
        /// <returns>An <see cref="IActionResult"/> indicating success.</returns>
        [Authorize]
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
            _logger.LogInformation("Profile updated for user {Username} (ID: {UserId}).", user.Username, id);
            return Ok(new { Message = _localizer["ProfileUpdated"].Value });
        }

        /// <summary>
        /// Initiates the account deletion process by generating a verification code.
        /// </summary>
        /// <param name="id">The unique identifier of the user.</param>
        /// <returns>An <see cref="IActionResult"/> indicating that the code has been sent.</returns>
        [Authorize]
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

        /// <summary>
        /// Sends a deactivation email to the user with a confirmation link.
        /// </summary>
        /// <param name="id">The unique identifier of the user.</param>
        /// <returns>An <see cref="IActionResult"/> indicating that the email has been sent.</returns>
        [Authorize]
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

            return Ok(new { message = _localizer["DeactivationEmailSent"].Value });
        }

        /// <summary>
        /// Confirms account deactivation via a secure token.
        /// This performs a logical deletion (inactivation).
        /// </summary>
        /// <param name="token">The deactivation token received via email.</param>
        /// <returns>A redirect to the frontend login or success page.</returns>
        [HttpGet("confirm-deactivation")]
        public async Task<IActionResult> ConfirmDeactivation([FromQuery] string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return BadRequest(_localizer["TokenInvalid"].Value);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.DeactivationToken == token);
            
            if (user == null || user.DeactivationTokenExpiration < DateTime.UtcNow)
                return BadRequest(_localizer["TokenInvalidOrExpired"].Value);

            // Logical deletion: deactivate account
            user.IsActive = false;
            user.DeactivationToken = null;
            user.DeactivationTokenExpiration = null;
            
            await _context.SaveChangesAsync();

            // Redirect to a success page or login
            return Redirect("http://localhost:8081/login?deactivated=true");
        }

        /// <summary>
        /// Permanently deletes a user's account after verifying a code sent via email.
        /// </summary>
        /// <param name="id">The unique identifier of the user.</param>
        /// <param name="code">The verification code.</param>
        /// <returns>An <see cref="IActionResult"/> indicating success.</returns>
        [Authorize]
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

        /// <summary>
        /// Registers a new user account and sends a verification email.
        /// </summary>
        /// <param name="registerDto">The registration details.</param>
        /// <returns>An <see cref="IActionResult"/> indicating registration success.</returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(registerDto.Username) || 
                string.IsNullOrWhiteSpace(registerDto.Email) || 
                string.IsNullOrWhiteSpace(registerDto.Password))
            {
                return BadRequest(new { message = _localizer["AllFieldsRequired"].Value });
            }

            // Validate email format
            if (!registerDto.Email.Contains("@") || !registerDto.Email.Contains("."))
            {
                return BadRequest(new { message = _localizer["InvalidEmailFormat"].Value });
            }

            // Validate password length
            if (registerDto.Password.Length < 6)
            {
                return BadRequest(new { message = _localizer["PasswordTooShort"].Value });
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
            _logger.LogInformation("New user registered: {Username} (ID: {UserId}).", newUser.Username, newUser.Id);
 
            // Send verification email
            await _emailService.SendVerificationEmailAsync(registerDto.Email, registerDto.Username, verificationToken, registerDto.Language ?? "es");
            _logger.LogInformation("Verification email sent for user {Username} (ID: {UserId}).", newUser.Username, newUser.Id);
 
            return Ok(new { message = _localizer["RegistrationSuccess"].Value });
        }

        /// <summary>
        /// Verifies a user's email address using a token.
        /// </summary>
        /// <param name="token">The verification token.</param>
        /// <returns>A redirect to the registration success page.</returns>
        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromQuery] string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return BadRequest(_localizer["TokenInvalid"].Value);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.VerificationToken == token);
            
            if (user == null)
            {
                return BadRequest(_localizer["TokenInvalid"].Value);
            }

            if (user.TokenExpiration < DateTime.UtcNow)
            {
                return BadRequest(_localizer["TokenExpired"].Value);
            }

            // Mark user as verified
            user.IsVerified = true;
            user.VerificationToken = null;
            user.TokenExpiration = null;
            
            await _context.SaveChangesAsync();

            // Redirect to frontend success page
            return Redirect($"http://localhost:8081/registro-exitoso?lng={user.Language}");
        }

        /// <summary>
        /// Updates the user's preferred language for the application interface.
        /// </summary>
        /// <param name="id">The unique identifier of the user.</param>
        /// <param name="updateLanguageDto">The new language code.</param>
        /// <returns>An <see cref="IActionResult"/> with the updated language.</returns>
        [Authorize]
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
                return BadRequest(new { message = _localizer["InvalidLanguage"].Value });
            }

            user.Language = updateLanguageDto.Language;
            await _context.SaveChangesAsync();

            return Ok(new { message = _localizer["LanguageUpdated"].Value, language = user.Language });
        }

        /// <summary>
        /// Updates the Expo Push Token for the user to enable mobile notifications.
        /// </summary>
        /// <param name="id">The unique identifier of the user.</param>
        /// <param name="pushTokenDto">The push token payload.</param>
        /// <returns>An <see cref="IActionResult"/> signaling success.</returns>
        [Authorize]
        [HttpPost("{id}/push-token")]
        public async Task<IActionResult> UpdatePushToken(int id, [FromBody] PushTokenDto pushTokenDto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.ExpoPushToken = pushTokenDto.Token;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Push token updated successfully" });
        }
    }
}
