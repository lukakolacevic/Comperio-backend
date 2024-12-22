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
using dotInstrukcijeBackend.Interfaces.Repository;

namespace dotInstrukcijeBackend.Services
{
    public class StudentService : IStudentService
    {
        private readonly ISubjectRepository _subjectRepository;
        private readonly IInstructorRepository _instructorRepository;
       
        public StudentService(
            ISubjectRepository subjectRepository,
            IInstructorRepository instructorRepository)
        {
            _subjectRepository = subjectRepository;
            _instructorRepository = instructorRepository;
        }

        public async Task<ServiceResult<IEnumerable<SubjectFrequencyDTO>>> FindTopFiveRequestedSubjectsAsync(int studentId)
        {
            var listOfMostChosenSubjects = await _subjectRepository.GetTopFiveRequestedSubjectsAsync(studentId);
            return ServiceResult<IEnumerable<SubjectFrequencyDTO>>.Success(listOfMostChosenSubjects);
        }

        public async Task<ServiceResult<IEnumerable<InstructorFrequencyDTO>>> FindTopFiveRequestedProfessorsAsync(int studentId)
        {
            var listOfMostChosenProfessors = await _instructorRepository.GetTopFiveRequestedProfessorsAsync(studentId);
            return ServiceResult<IEnumerable<InstructorFrequencyDTO>>.Success(listOfMostChosenProfessors);
        }

      
    }
}
