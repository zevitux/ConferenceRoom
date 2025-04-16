using ConferenceRoomApi.DTOs.Auth;
using ConferenceRoomApi.Models;
using ConferenceRoomApi.Repositories;
using ConferenceRoomApi.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace ConferenceRoomApi.Services;

public class AuthService(
    IUserRepository userRepository,
    ILogger<AuthService> logger,
    IConfiguration configuration)
    : IAuthService
{
    public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
    {
        try
        {
            var existingUser = await userRepository.GetUserByEmailAsync(registerDto.Email);
            if (existingUser != null)
            {
                logger.LogWarning("Registration attempt with existing email: {Email}", registerDto.Email);
                throw new Exception("Email already exists");
            }

            var newUser = new User
            {
                Email = registerDto.Email,
                Name = registerDto.Name,
                Role = registerDto.Role
            };

            newUser.PasswordHash = new PasswordHasher<User>().HashPassword(newUser, registerDto.Password);
            newUser.RefreshToken = GenerateRefreshToken();
            newUser.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            var createdUser = await userRepository.AddAsync(newUser);
            logger.LogInformation("Registration successful: {Email}", registerDto.Email);

            return await GenerateTokens(createdUser);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Registration failed: {Email}", registerDto.Email);
            throw;
        }
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
    {
        try
        {
            logger.LogInformation("Login attempt with login: {Login}", loginDto.Email);

            var user = await userRepository.GetUserByEmailAsync(loginDto.Email);
            if (user == null)
            {
                logger.LogWarning("Login attempt with invalid email: {Email}", loginDto.Email);
                throw new Exception("Invalid credentials");
            }

            var passwordHasher = new PasswordHasher<User>();
            var passwordVerificationResult = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, loginDto.Password);

            if (passwordVerificationResult == PasswordVerificationResult.Failed)
            {
                logger.LogWarning("Login attempt with invalid password: {Email}", loginDto.Email);
                throw new Exception("Invalid credentials");
            }
            
            logger.LogInformation("Successful login for: {Email}", loginDto.Email);
            return await GenerateTokens(user);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Login failed: {Email}", loginDto.Email);
            throw;
        }
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto refreshTokenDto)
    {
        try
        {
            var principal = GetPrincipalFromExpiredToken(refreshTokenDto.AccessToken);
            var userId = int.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var user = await ValidateRefreshToken(userId, refreshTokenDto.RefreshToken);
            if (user == null)
            {
                logger.LogWarning("Refresh token attempt with invalid refresh token: {RefreshToken}", refreshTokenDto.RefreshToken);
                throw new Exception("Invalid refresh token");
            }

            return await GenerateTokens(user);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Refresh token failed: {RefreshToken}", refreshTokenDto.RefreshToken);
            throw;
        }
    }

    private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidatorParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]!)),
            ValidateLifetime = false
        };
        
        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.ValidateToken(token, tokenValidatorParameters, out _);
    }

    private async Task<User?> ValidateRefreshToken(int userId, string refreshToken)
    {
        var user = await userRepository.GetUserById(userId);
        
        if(user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime < DateTime.Now)
            return null;
        
        return user;
    }

    private async Task<AuthResponseDto> GenerateTokens(User user)
    {
        var accessToken = GenerateJwtToken(user);
        var refreshToken = GenerateRefreshToken();
        
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(
            configuration.GetValue("Jwt:RefreshTokenExpiryDays", 7));
        
        await userRepository.UpdateAsync(user);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
        };
    }

    private string GenerateJwtToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(configuration["Jwt:Secret"]!);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
            }),
            Expires = DateTime.UtcNow.AddMinutes(
                configuration.GetValue("Jwt:AccessTokenExpiryMinutes", 15)),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256)
        };
        
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
    }
}