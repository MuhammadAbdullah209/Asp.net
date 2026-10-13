using Auth_System.data;
using Auth_System.DTOs;
using Auth_System.Helper;
using Auth_System.IService;
using Auth_System.Models;
using Auth_System.Models.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.ComponentModel.DataAnnotations;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace Auth_System.Services
{
    public class AuthService : IAuth
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IMemoryCache _cache;


        public AuthService(AppDbContext context, IEmailService emailService, IMemoryCache cache)
        {
            _context = context;
            _emailService = emailService;
            _cache = cache;



        }
        public async Task<Tuple<int, User?>> Register(ResgisterReqDto registerReqDto)
        {
            try
            {
                if (registerReqDto == null)
                {
                    return new Tuple<int, User?>(4, null);
                }
                if (string.IsNullOrWhiteSpace(registerReqDto.FirstName) ||
                   string.IsNullOrWhiteSpace(registerReqDto.Email) ||
                   string.IsNullOrWhiteSpace(registerReqDto.Password))
                {
                    return new Tuple<int, User?>(5, null);
                }
                if (registerReqDto.Email?.EndsWith("@gmail.com", StringComparison.OrdinalIgnoreCase) != true)
                    return new Tuple<int, User?>(1, null);
                var emailValidator = new EmailAddressAttribute();

                if (!emailValidator.IsValid(registerReqDto.Email))
                {
                    return new Tuple<int, User?>(6, null);
                }

                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == registerReqDto.Email);

                if (existingUser != null)
                {
                    return new Tuple<int, User?>(2, null);
                }

                var newUser = new User
                {
                    Id = Guid.NewGuid(),
                    FirstName = registerReqDto.FirstName,
                    LastName = registerReqDto.LastName,
                    DOB = registerReqDto.DOB,
                    Gender = registerReqDto.Gender,
                    Email = registerReqDto.Email,
                    Password = BCrypt.Net.BCrypt.HashPassword(registerReqDto.Password),
                    PhoneNumber = registerReqDto.PhoneNumber,
                    Role = "user",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                
                var token = TokenHelper.GenerateToken();

                var pending = new PendingRegistration
                {
                    User = newUser,
                    TokenHash = TokenHelper.Hash(token)
                };

                _cache.Set($"pending:{newUser.Email.ToLowerInvariant()}", pending,
                    TimeSpan.FromMinutes(15));  

                await _emailService.SendAsync(
                    newUser.Email,
                    "Verify your email",
                    $"<p>Hello {newUser.FirstName},</p>" +
                    $"<p>Your verification token is: <b>{token}</b></p>" +
                    $"<p>It is valid for 15 minutes.</p>");

                return new Tuple<int, User?>(0, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine("REGISTER ERROR: " + ex.Message);
                Console.WriteLine(ex.StackTrace);
                return new Tuple<int, User?>(3, null);
            }
        }
        public async Task<Tuple<int, User?>> Login(LoginReqDto dto)
        {
            try
            {
                if (dto == null)
                {
                    return new Tuple<int, User?>(4, null);

                }

                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
                if (existingUser == null)
                {
                    return new Tuple<int, User?>(2, null);
                }
                if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                {
                    return new Tuple<int, User?>(5, null);
                }
                var emailValidator = new EmailAddressAttribute();

                if (!emailValidator.IsValid(dto.Email))
                {
                    return new Tuple<int, User?>(6, null);
                }
                if (!BCrypt.Net.BCrypt.Verify(dto.Password, existingUser.Password))
                {
                    return new Tuple<int, User?>(1, null);
                }
                return new Tuple<int, User?>(2, existingUser);


            }
            catch (Exception)
            {
                return new Tuple<int, User?>(3, null);
            }

        }
        public async Task<int> ForgotPassword(ForgotPasswordReqDto dto)
        {
            try
            {
                if (dto == null || string.IsNullOrWhiteSpace(dto.Email))
                {
                    return 4;
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
                if (user == null)
                {
                    return 0; 
                }

                var token = TokenHelper.GenerateToken();
                user.PasswordResetTokenHash = TokenHelper.Hash(token);
                user.PasswordResetTokenExpiry = DateTime.UtcNow.AddMinutes(30);
                user.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                

                await _emailService.SendAsync(
                    user.Email,
                    "Reset your password",
                    $"<p>Hello {user.FirstName},</p>" +
                    $"<p> It is valid for 30 minutes.</p>" +
                    $"<p>Your reset token: <b>{token}</b></p>" +
                    $"<p>If you did not request this, you can ignore this email.</p>");

                return 0;
            }
            catch (Exception)
            {
                return 3;
            }
        }

        public async Task<int> ResetPassword(ResetPasswordReqDto dto)
        {
            try
            {
                if (dto == null ||
                    string.IsNullOrWhiteSpace(dto.Email) ||
                    string.IsNullOrWhiteSpace(dto.Token) ||
                    string.IsNullOrWhiteSpace(dto.NewPassword))
                {
                    return 4;
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

                if (user == null ||
                    user.PasswordResetTokenHash == null ||
                    user.PasswordResetTokenExpiry == null ||
                    user.PasswordResetTokenExpiry < DateTime.UtcNow ||
                    user.PasswordResetTokenHash != TokenHelper.Hash(dto.Token))
                {
                    return 1; 
                }

                user.Password = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
                user.PasswordResetTokenHash = null;   // token works only once
                user.PasswordResetTokenExpiry = null;
                user.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return 0;
            }
            catch (Exception)
            {
                return 3;
            }
        }

        
        public async Task<int> VerifyEmail(VerifyEmailReqDto dto)
        {
            try
            {
                if (dto == null || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Token))
                    return 4;

                var key = $"pending:{dto.Email.ToLowerInvariant()}";

                if (!_cache.TryGetValue(key, out PendingRegistration? pending) || pending == null)
                    return 1;   // never registered, or expired

                if (pending.Attempts >= 5)
                {
                    _cache.Remove(key);   // too many wrong guesses, must register again
                    return 1;
                }

                if (pending.TokenHash != TokenHelper.Hash(dto.Token))
                {
                    pending.Attempts++;
                    return 1;
                }

                // check again in case someone registered this email in the meantime
                var exists = await _context.Users.AnyAsync(u => u.Email == pending.User.Email);
                if (exists)
                {
                    _cache.Remove(key);
                    return 2;
                }

                pending.User.CreatedAt = DateTime.UtcNow;
                pending.User.UpdatedAt = DateTime.UtcNow;

                await _context.Users.AddAsync(pending.User);
                await _context.SaveChangesAsync();

                _cache.Remove(key);   // token can only be used once
                return 0;
            }
            catch (Exception)
            {
                return 3;
            }
        }
    }
}