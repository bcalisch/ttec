using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Backend.Api.Contracts.Auth;
using Backend.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Backend.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ICurrentUserService _currentUser;

    private static readonly Dictionary<string, (string Password, Guid Id, string DisplayName)> DevUsers = new(StringComparer.OrdinalIgnoreCase)
    {
        ["admin@test.com"] = ("password", Guid.Parse("00000000-0000-0000-0000-000000000001"), "Admin User"),
        ["tech1@test.com"] = ("password", Guid.Parse("00000000-0000-0000-0000-000000000002"), "Tech One"),
        ["tech2@test.com"] = ("password", Guid.Parse("00000000-0000-0000-0000-000000000003"), "Tech Two"),
    };

    public AuthController(IConfiguration configuration, ICurrentUserService currentUser)
    {
        _configuration = configuration;
        _currentUser = currentUser;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public ActionResult<LoginResponse> Login([FromBody] LoginRequest request)
    {
        if (!DevUsers.TryGetValue(request.Email, out var user) || user.Password != request.Password)
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        var expiresAt = DateTimeOffset.UtcNow.AddHours(8);
        var token = GenerateToken(user.Id, request.Email, user.DisplayName, expiresAt);

        return Ok(new LoginResponse(
            token,
            expiresAt,
            new UserInfo(user.Id, request.Email, user.DisplayName)
        ));
    }

    [HttpGet("me")]
    [Authorize]
    public ActionResult<UserInfo> Me()
    {
        return Ok(new UserInfo(
            _currentUser.UserId,
            _currentUser.Email,
            _currentUser.DisplayName
        ));
    }

    private string GenerateToken(Guid userId, string email, string displayName, DateTimeOffset expiresAt)
    {
        var signingKey = _configuration["Jwt:SigningKey"]!;
        var issuer = _configuration["Jwt:Issuer"];
        var audience = _configuration["Jwt:Audience"];

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim("name", displayName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
