using Azure;
using dotInstrukcijeBackend.Interfaces.RepositoryInterfaces;
using dotInstrukcijeBackend.Interfaces.User;
using dotInstrukcijeBackend.Interfaces.Utility;
using dotInstrukcijeBackend.Models;
using dotInstrukcijeBackend.ServiceResultUtility;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace dotInstrukcijeBackend.JWTTokenUtility
{
    public class TokenService : ITokenService
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IProfessorRepository _professorRepository;
        private readonly IConfiguration _configuration;

        public TokenService(IStudentRepository studentRepository, IProfessorRepository professorRepository, IConfiguration configuration)
        {
            _studentRepository = studentRepository;
            _professorRepository = professorRepository;
            _configuration = configuration;
        }

        public string GenerateAccessToken(IUser user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:AccessTokenSecretKey"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim("id", user.Id.ToString()),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddMinutes(15),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public string GenerateRefreshToken(IUser user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:RefreshTokenSecretKey"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("id", user.Id.ToString()),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        public async Task <ServiceResult<(string, string)>> RefreshToken(string oldRefreshToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:RefreshTokenSecretKey"]);

            var principal = tokenHandler.ValidateToken(oldRefreshToken, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true
            }, out var validatedToken);

            var userId = principal.FindFirst("id")?.Value;
            var role = principal.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(role))
            {
                return ServiceResult<(string, string)>.Failure("Invalid token claims.", 400);
            }

            IUser user = null;
            if(role == "Professor")
            {
                var professor = await _professorRepository.GetProfessorByIdAsync(int.Parse(userId));
                if(professor == null)
                {
                    return ServiceResult<(string, string)>.Failure("Professor from claim not found.", 404);
                }
                user = (IUser?)professor;
            }

            else
            {
                var student = await _studentRepository.GetStudentByIdAsync(int.Parse(userId));
                if (student == null)
                {
                    return ServiceResult<(string, string)>.Failure("Student from claim not found.", 404);
                }
                user = (IUser?)student;
            }

            if (user == null)
            {
                return ServiceResult<(string, string)>.Failure("User not found.", 404);
            }

            var newAccessToken = GenerateAccessToken(user);
            var newRefreshToken = GenerateRefreshToken(user);

            return ServiceResult<(string, string)>.Success((newAccessToken, newRefreshToken));
        }
    }
}
