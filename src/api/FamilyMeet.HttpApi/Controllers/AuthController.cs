using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using FamilyMeet.Application.Contracts.DTOs;
using FamilyMeet.Application.Contracts.Services;

namespace FamilyMeet.HttpApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;
    private readonly IChatAppService _chatAppService;

    public AuthController(
        IConfiguration configuration,
        ILogger<AuthController> logger,
        IChatAppService chatAppService)
    {
        _configuration = configuration;
        _logger = logger;
        _chatAppService = chatAppService;
    }

    [HttpPost("google")]
    public async Task<ActionResult<LoginResponseDto>> GoogleLogin([FromBody] GoogleLoginRequestDto request)
    {
        try
        {
            // Validate Google token
            var googleUser = await ValidateGoogleTokenAsync(request.IdToken);
            if (googleUser == null)
            {
                return BadRequest(new { error = "Invalid Google token" });
            }

            // Create or get user
            var user = await _chatAppService.GetOrCreateUserAsync(new CreateUserDto
            {
                Email = googleUser.Email,
                Name = googleUser.Name,
                Avatar = googleUser.Picture,
                Provider = "Google",
                ProviderId = googleUser.Sub
            });

            // Generate JWT token
            var token = GenerateJwtToken(user.Id, user.Email, user.Name);

            _logger.LogInformation("User {Email} logged in via Google", googleUser.Email);

            return Ok(new LoginResponseDto
            {
                Token = token,
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    Name = user.Name,
                    Avatar = user.Avatar
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Google login");
            return BadRequest(new { error = "Login failed" });
        }
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<LoginResponseDto>> RefreshToken([FromBody] RefreshTokenDto request)
    {
        try
        {
            // Validate and refresh token logic here
            // For now, just return a new token
            var principal = GetPrincipalFromExpiredToken(request.Token);
            if (principal == null)
            {
                return BadRequest(new { error = "Invalid token" });
            }

            var userIdClaim = principal.FindFirst("sub")?.Value;
            var emailClaim = principal.FindFirst("email")?.Value;
            var nameClaim = principal.FindFirst("name")?.Value;

            if (userIdClaim == null || emailClaim == null || nameClaim == null)
            {
                return BadRequest(new { error = "Invalid token claims" });
            }

            var userId = Guid.Parse(userIdClaim);
            var newToken = GenerateJwtToken(userId, emailClaim, nameClaim);

            return Ok(new LoginResponseDto
            {
                Token = newToken,
                User = new UserDto
                {
                    Id = userId,
                    Email = emailClaim,
                    Name = nameClaim
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return BadRequest(new { error = "Token refresh failed" });
        }
    }

    private async Task<GoogleUserDto> ValidateGoogleTokenAsync(string idToken)
    {
        try
        {
            // In production, validate with Google's API
            // For now, decode the token (simplified)
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(idToken);

            return new GoogleUserDto
            {
                Sub = jwt.Claims.FirstOrDefault(c => c.Type == "sub")?.Value ?? string.Empty,
                Email = jwt.Claims.FirstOrDefault(c => c.Type == "email")?.Value ?? string.Empty,
                Name = jwt.Claims.FirstOrDefault(c => c.Type == "name")?.Value ?? string.Empty,
                Picture = jwt.Claims.FirstOrDefault(c => c.Type == "picture")?.Value ?? string.Empty,
                EmailVerified = jwt.Claims.FirstOrDefault(c => c.Type == "email_verified")?.Value == "true"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating Google token");
            return null;
        }
    }

    private string GenerateJwtToken(Guid userId, string email, string name)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _configuration["Jwt:Key"] ?? "FamilyMeetSecretKey123456789"));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("sub", userId.ToString()),
            new Claim("email", email),
            new Claim("name", name),
            new Claim("user_id", userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(60),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        try
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration["Jwt:Key"] ?? "FamilyMeetSecretKey123456789"));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = false, // Don't validate lifetime for refresh
                ValidateIssuerSigningKey = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidAudience = _configuration["Jwt:Audience"],
                IssuerSigningKey = key,
                ClockSkew = TimeSpan.Zero
            };

            var handler = new JwtSecurityTokenHandler();
            var principal = handler.ValidateToken(token, tokenValidationParameters, out var securityToken);

            return principal;
        }
        catch
        {
            return null;
        }
    }
}

public class GoogleLoginRequestDto
{
    public string IdToken { get; set; } = string.Empty;
}

public class RefreshTokenDto
{
    public string Token { get; set; } = string.Empty;
}

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public UserDto User { get; set; } = new();
}

public class GoogleUserDto
{
    public string Sub { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Picture { get; set; } = string.Empty;
    public bool EmailVerified { get; set; }
}
