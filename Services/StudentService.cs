using Azure.Core;
using Azure;
using dotInstrukcijeBackend.DataTransferObjects;
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

        public async Task<ServiceResult> RegisterStudentAsync(StudentRegistrationModel model)
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
                ProfilePicture = await _profilePhotoSaver.SaveProfilePictureAsync(model.ProfilePicture)
            };

            await _studentRepository.AddStudentAsync(student);

            return ServiceResult.Success();
        }

        public async Task<ServiceResult<(StudentDTO, string, string)>> LoginStudentAsync(LoginModel model)
        {
            var student = await _studentRepository.GetStudentByEmailAsync(model.Email);
            if (student == null)
            {
                return ServiceResult<(StudentDTO, string, string)>.Failure("Student not found.", 400);
            }

            if (!_passwordHasher.VerifyPassword(student.Password, model.Password))
            {
                return ServiceResult<(StudentDTO, string, string)>.Failure("Invalid password.", 401);
            }

            var accessToken = _tokenService.GenerateAccessToken(student);
            var refreshToken = _tokenService.GenerateRefreshToken(student);

            

            string? profilePhotoBase64String = student.ProfilePicture != null
                ? Convert.ToBase64String(student.ProfilePicture)
                : null;

            var studentDTO = new StudentDTO(student.Id, student.Name, student.Surname, profilePhotoBase64String);

            return ServiceResult<(StudentDTO, string, string)>.Success((studentDTO, accessToken, refreshToken));
        }

        public async Task<ServiceResult<StudentDTO>> FindStudentByEmailAsync(string email)
        {
            var student = await _studentRepository.GetStudentByEmailAsync(email);

            if (student == null)
            {
                return ServiceResult<StudentDTO>.Failure("Student not found.", 404);
            }

            string? profilePhotoBase64String = student.ProfilePicture != null
            ? Convert.ToBase64String(student.ProfilePicture)
            : null;

            var studentDTO = new StudentDTO(student.Id, student.Name, student.Surname, profilePhotoBase64String);

            return ServiceResult<StudentDTO>.Success(studentDTO);
        }

        public async Task<ServiceResult<IEnumerable<StudentDTO>>> FindAllStudentsAsync()
        {
            var listOfStudents = await _studentRepository.GetAllStudentsAsync();

            var listOfStudentsDTO = new List<StudentDTO>();

            foreach (var student in listOfStudents)
            {
                String profilePhotoBase64String = student.ProfilePicture is not null ? Convert.ToBase64String(student.ProfilePicture) : null;
                var studentDTO = new StudentDTO(student.Id, student.Name, student.Surname, profilePhotoBase64String);
                listOfStudentsDTO.Add(studentDTO);
            }

            return ServiceResult<IEnumerable<StudentDTO>>.Success(listOfStudentsDTO);
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


    }
}
    

