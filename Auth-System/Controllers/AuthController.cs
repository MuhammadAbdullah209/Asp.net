using Auth_System.DTOs;
using Auth_System.IService;
using Auth_System.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Auth_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuth _authService;
        private readonly IEmailService _emailService;

        public AuthController(IAuth authService, IEmailService emailService)
        {
            _authService = authService;
            _emailService = emailService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] ResgisterReqDto registerReqDto)
        {
            var result = await _authService.Register(registerReqDto);

            if (result.Item1 == 0)
            {
                return Ok(Response_Model<object>.Success(null, "User registered successfully.", true));
            }
            else if (result.Item1 == 2)
            {
                return Conflict(Response_Model<object>.Failure("Email address is already registered.", false));
            }
            else if (result.Item1 == 4)
            {
                return BadRequest(Response_Model<object>.Failure("Request body cannot be empty.", false));
            }
            else if (result.Item1 == 5)
            {
                return BadRequest(Response_Model<object>.Failure("First name, email address and password are required.", false));
            }
            else if (result.Item1 == 6)
            {
                return BadRequest(Response_Model<object>.Failure("Invalid email address format.", false));
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    Response_Model<object>.Failure("Something went wrong while registering. Please try again later.", false));
            }
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginReqDto dto)
        {
            try
            {
                var existingUser = await _authService.Login(dto);

                if (existingUser.Item1 == 0)
                {
                    return NotFound(Response_Model<string>.Failure("User not found", false));
                }

                if (existingUser.Item1 == 1)
                {
                    return BadRequest(Response_Model<string>.Failure("Invalid credentials", false));
                }

                TokenUserDto user = new();

                user.Id = existingUser.Item2.Id;
                user.Name = existingUser.Item2.FirstName;
                user.Email = existingUser.Item2.Email;
                user.Role = existingUser.Item2.Role;

                var token = GenerateJwtToken(user);

                return Ok(LoginResDto.Success(user.Name, user.Email, token));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                var ErrResult = new Tuple<int, string>(3, "Internal Server Error!");
                return StatusCode(500, ErrResult);
            }
        }

        
       

        private string GenerateJwtToken(TokenUserDto dto)
        {
            var claims = new Claim[]
            {
                new Claim(ClaimTypes.Email, dto.Email),
                new Claim(ClaimTypes.Name, dto.Name),
                new Claim(ClaimTypes.NameIdentifier, dto.Id.ToString()),
                new Claim(ClaimTypes.Role, dto.Role)
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("fiwe09i90849igerubg93u9bv9rue9t93th8gr")
            );

            var creds = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256
            );

            var tokens = new JwtSecurityToken(
                issuer: "Auth System",
                audience: "Auth System Backend",
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(tokens);
        }
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordReqDto dto)
        {
            var result = await _authService.ForgotPassword(dto);

            if (result == 0)
            {
                return Ok(Response_Model<object>.Success(null,
                    "If this email is registered, a reset link has been sent.", true));
            }
            else if (result == 4)
            {
                return BadRequest(Response_Model<object>.Failure("Email is required.", false));
            }
            else
            {
                return StatusCode(500, Response_Model<object>.Failure(
                    "Something went wrong. Please try again later.", false));
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordReqDto dto)
        {
            var result = await _authService.ResetPassword(dto);

            if (result == 0)
            {
                return Ok(Response_Model<object>.Success(null, "Password reset successfully.", true));
            }
            else if (result == 1)
            {
                return BadRequest(Response_Model<object>.Failure("Invalid or expired reset link.", false));
            }
            else if (result == 4)
            {
                return BadRequest(Response_Model<object>.Failure(
                    "Email, token and new password are required.", false));
            }
            else
            {
                return StatusCode(500, Response_Model<object>.Failure(
                    "Something went wrong. Please try again later.", false));
            }
        }
        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailReqDto dto)
        {
            var result = await _authService.VerifyEmail(dto);

            if (result == 0)
                return Ok(Response_Model<object>.Success(null, "Email verified. Registration complete.", true));
            else if (result == 1)
                return BadRequest(Response_Model<object>.Failure("Invalid or expired token.", false));
            else if (result == 2)
                return Conflict(Response_Model<object>.Failure("Email address is already registered.", false));
            else if (result == 4)
                return BadRequest(Response_Model<object>.Failure("Email and token are required.", false));
            else
                return StatusCode(500, Response_Model<object>.Failure("Something went wrong. Please try again later.", false));
        }
    }
}
