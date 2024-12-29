using Azure.Core;
using dotInstrukcijeBackend.Interfaces.Repository;
using dotInstrukcijeBackend.Interfaces.RepositoryInterfaces;
using dotInstrukcijeBackend.Interfaces.Service;
using dotInstrukcijeBackend.Interfaces.Utility;
using dotInstrukcijeBackend.JWTTokenUtility;
using dotInstrukcijeBackend.Models;
using dotInstrukcijeBackend.ServiceResultUtility;
using dotInstrukcijeBackend.ViewModels;

namespace dotInstrukcijeBackend.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IProfilePhotoSaver _profilePhotoSaver;
        private readonly ITokenService _tokenService;
        private readonly IInstructorRepository _instructorRepository;

        public UserService(IInstructorRepository instructorRepository,IUserRepository userRepository, IPasswordHasher passwordHasher, IProfilePhotoSaver profilePhotoSaver, ITokenService tokenService)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _profilePhotoSaver = profilePhotoSaver;
            _tokenService = tokenService;
            _instructorRepository = instructorRepository;
        }

        public async Task<ServiceResult<User>> FindUserByEmailAsync(int roleId, string email)
        {
            var user = await _userRepository.GetUserByEmailAsync(roleId, email);
            if (user == null)
            {
                return ServiceResult<User>.Failure("User not found", 404);
            }
            return ServiceResult<User>.Success(user);
        }

        public async Task<ServiceResult<IEnumerable<User>>> FindAllUsersByRoleIdAsync(int roleId)
        {
            var listOfStudents = await _userRepository.GetAllUsersByRoleAsync(roleId);
            return ServiceResult<IEnumerable<User>>.Success(listOfStudents);
        }

        public async Task<ServiceResult> ConfirmEmailAsync(int id)
        {
            await _userRepository.SetEmailVerifiedAsync(id);
            return ServiceResult.Success();
        }

        public async Task<ServiceResult> RegisterUserAsync(int roleId, RegistrationModel model)
        {
            if (await _userRepository.GetUserByEmailAsync(roleId, model.Email) != null)
            {
                return ServiceResult.Failure("Email is already in use.", 400);
            }

            var user = new User
            {
                RoleId = roleId,
                Email = model.Email,
                Name = model.Name,
                Surname = model.Surname,
                PasswordHash = _passwordHasher.HashPassword(model.Password),
                ProfilePicture = null,
                OAuthId = null,
                CreatedAt = DateTime.UtcNow,
                IsVerified = false
            };

            var userId = await _userRepository.AddUserAsync(user);
            Console.WriteLine("Id:" + userId);
            if(roleId == 2)
            {
                await _instructorRepository.InitializeInstructorStats(userId);
            }
            return ServiceResult.Success();
        }

        public async Task<ServiceResult<(User, string, string)>> LoginUserAsync(int roleId, LoginModel model)
        {
            var user = await _userRepository.GetUserByEmailAsync(roleId, model.Email);
            if (user == null)
            {
                return ServiceResult<(User, string, string)>.Failure("User not found.", 400);
            }

            if (!_passwordHasher.VerifyPassword(user.PasswordHash, model.Password))
            {
                return ServiceResult<(User, string, string)>.Failure("Wrong password.", 401);
            }
            if (!user.IsVerified)
            {
                return ServiceResult<(User, string, string)>.Failure("User not verified.", 401);
            }
            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken(user);

            return ServiceResult<(User, string, string)>.Success((user, accessToken, refreshToken));
        }

        public async Task<ServiceResult<User>> FindUserByIdAsync(int id)
        {
            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null)
            {
                return ServiceResult<User>.Failure("User not found.", 400);
            }

            return ServiceResult<User>.Success(user);
        }

        public async Task<ServiceResult<User>> RegisterGoogleUserAsync(int roleId, User googleUser)
        {
            // Check if the email is already in use for this role
            if (await _userRepository.GetUserByEmailAsync(roleId, googleUser.Email) != null)
            {
                return ServiceResult<User>.Failure("Email is already in use.", 400);
            }

            // Create the user entity
            var user = new User
            {
                RoleId = roleId,
                Email = googleUser.Email,
                Name = googleUser.Name,
                Surname = googleUser.Surname,
                PasswordHash = null, // No password since it's Google login
                ProfilePicture = googleUser.ProfilePicture, // Can be set to null or Google's profile picture URL
                OAuthId = googleUser.OAuthId, // This should be set to the unique Google user ID
                CreatedAt = DateTime.UtcNow,
                IsVerified = true // Users logging in with Google are assumed verified
            };

            // Save the user in the database
            var userId = await _userRepository.AddUserAsync(user);

            // If the role is instructor, initialize instructor stats
            if (roleId == 2)
            {
                await _instructorRepository.InitializeInstructorStats(userId);
            }

            // Retrieve the saved user
            var registeredUser = await _userRepository.GetUserByIdAsync(userId);
            if (registeredUser == null)
            {
                return ServiceResult<User>.Failure("Failed to retrieve registered user.", 500);
            }

            return ServiceResult<User>.Success(registeredUser);
        }
    }
}
