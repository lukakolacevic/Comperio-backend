using Azure.Core;
using Azure;
using dotInstrukcijeBackend.Interfaces.RepositoryInterfaces;
using dotInstrukcijeBackend.Interfaces.ServiceInterfaces;
using dotInstrukcijeBackend.Interfaces.Utility;
using dotInstrukcijeBackend.JWTTokenUtility;
using dotInstrukcijeBackend.Models;
using dotInstrukcijeBackend.PasswordHashingUtilities;
using dotInstrukcijeBackend.ProfilePictureSavingUtility;
using dotInstrukcijeBackend.ServiceResultUtility;
using dotInstrukcijeBackend.ViewModels;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using dotInstrukcijeBackend.DataTransferObjects;

namespace dotInstrukcijeBackend.Services
{
    public class StudentService : IStudentService
    {
        private readonly IStudentRepository _studentRepository;
        private readonly ISubjectRepository _subjectRepository;
        private readonly IProfessorRepository _professorRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IProfilePhotoSaver _profilePhotoSaver;
        private readonly ITokenService _tokenService;

        public StudentService(
            IStudentRepository studentRepository,
            ISubjectRepository subjectRepository,
            IPasswordHasher passwordHasher,
            IProfilePhotoSaver profilePhotoSaver,
            ITokenService tokenService,
            IProfessorRepository professorRepository)
        {
            _studentRepository = studentRepository;
            _subjectRepository = subjectRepository;
            _passwordHasher = passwordHasher;
            _profilePhotoSaver = profilePhotoSaver;
            _tokenService = tokenService;
            _professorRepository = professorRepository;
        }

        public async Task<ServiceResult> RegisterStudentAsync(RegistrationModel model)
        {
            if (await _studentRepository.GetStudentByEmailAsync(model.Email) != null)
            {
                return ServiceResult.Failure("Email is already in use.", 400);
            }

            var student = new Student
            {
                Email = model.Email,
                Name = model.Name,
                Surname = model.Surname,
                Password = _passwordHasher.HashPassword(model.Password),
                ProfilePicture = await _profilePhotoSaver.SaveProfilePictureAsync(model.ProfilePicture),
                OAuthId = null,
                CreatedAt = DateTime.UtcNow,
                IsVerified = false
            };

            await _studentRepository.AddStudentAsync(student);

            return ServiceResult.Success();
        }

        public async Task<ServiceResult<(Student, string, string)>> LoginStudentAsync(LoginModel model)
        {
            var student = await _studentRepository.GetStudentByEmailAsync(model.Email);
            if (student == null)
            {
                return ServiceResult<(Student, string, string)>.Failure("Student not found.", 400);
            }

            if (!_passwordHasher.VerifyPassword(student.Password, model.Password))
            {
                return ServiceResult<(Student, string, string)>.Failure("Invalid password.", 401);
            }

            var accessToken = _tokenService.GenerateAccessToken(student);
            var refreshToken = _tokenService.GenerateRefreshToken(student);

            // Previously we used StudentDTO and a base64 string for the profile picture. Now we directly return the Student model.
            // We'll no longer use the base64 string, as we're not changing logic beyond class replacement.
            var studentObj = new Student
            {
                Id = student.Id,
                Email = student.Email,
                Name = student.Name,
                Surname = student.Surname,
                Password = student.Password,
                ProfilePicture = student.ProfilePicture,
                OAuthId = student.OAuthId,
                CreatedAt = student.CreatedAt,
                IsVerified = student.IsVerified
            };

            return ServiceResult<(Student, string, string)>.Success((studentObj, accessToken, refreshToken));
        }

        public async Task<ServiceResult<Student>> FindStudentByEmailAsync(string email)
        {
            var student = await _studentRepository.GetStudentByEmailAsync(email);

            if (student == null)
            {
                return ServiceResult<Student>.Failure("Student not found.", 404);
            }

            // Previously returned a StudentDTO with a base64 string. Now we return the Student model directly.
            var studentObj = new Student
            {
                Id = student.Id,
                Email = student.Email,
                Name = student.Name,
                Surname = student.Surname,
                Password = student.Password,
                ProfilePicture = student.ProfilePicture
            };

            return ServiceResult<Student>.Success(studentObj);
        }

        public async Task<ServiceResult<IEnumerable<Student>>> FindAllStudentsAsync()
        {
            var listOfStudents = await _studentRepository.GetAllStudentsAsync();

            var listOfStudentsModels = new List<Student>();

            foreach (var student in listOfStudents)
            {
                var studentObj = new Student
                {
                    Id = student.Id,
                    Email = student.Email,
                    Name = student.Name,
                    Surname = student.Surname,
                    Password = student.Password,
                    ProfilePicture = student.ProfilePicture
                };
                listOfStudentsModels.Add(studentObj);
            }

            return ServiceResult<IEnumerable<Student>>.Success(listOfStudentsModels);
        }

        public async Task<ServiceResult<IEnumerable<SubjectFrequencyDTO>>> FindTopFiveRequestedSubjectsAsync(int studentId)
        {
            var listOfMostChosenSubjects = await _subjectRepository.GetTopFiveRequestedSubjectsAsync(studentId);
            return ServiceResult<IEnumerable<SubjectFrequencyDTO>>.Success(listOfMostChosenSubjects);
        }

        public async Task<ServiceResult<IEnumerable<ProfessorFrequencyDTO>>> FindTopFiveRequestedProfessorsAsync(int studentId)
        {
            var listOfMostChosenProfessors = await _professorRepository.GetTopFiveRequestedProfessorsAsync(studentId);
            return ServiceResult<IEnumerable<ProfessorFrequencyDTO>>.Success(listOfMostChosenProfessors);
        }

        public async Task<ServiceResult> ConfirmEmailAsync(int id)
        {
            await _studentRepository.SetEmailVerifiedAsync(id);
            return ServiceResult.Success();
        }
    }
}
