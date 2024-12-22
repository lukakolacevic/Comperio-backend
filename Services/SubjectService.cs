using dotInstrukcijeBackend.Models;
//using dotInstrukcijeBackend.HelperFunctions;
using dotInstrukcijeBackend.Interfaces.RepositoryInterfaces;
using dotInstrukcijeBackend.Interfaces.Service;
using dotInstrukcijeBackend.ServiceResultUtility;
using dotInstrukcijeBackend.ViewModels;
using System.Linq;

namespace dotInstrukcijeBackend.Services
{
    public class SubjectService : ISubjectService
    {
        private readonly ISubjectRepository _subjectRepository;
        private readonly IInstructorRepository _instructorRepository;

        public SubjectService(ISubjectRepository subjectRepository, IInstructorRepository instructorRepository)
        {
            _subjectRepository = subjectRepository;
            _instructorRepository = instructorRepository;
        }

        public async Task<ServiceResult> CreateSubjectAsync(SubjectRegistrationModel request, int professorId)
        {
            if (await _subjectRepository.GetSubjectByTitleAsync(request.Title) != null)
            {
                return ServiceResult.Failure("Subject with the given title already exists.", 400);
            }

            var existingSubjectByURL = await _subjectRepository.GetSubjectByURLAsync(request.Url);
            if (existingSubjectByURL.Subject != null)
            {
                return ServiceResult.Failure("Subject with the given URL already exists.", 400);
            }

            var subject = new Subject
            {
                Title = request.Title,
                Url = request.Url,
                Description = request.Description
            };

            var subjectId = await _subjectRepository.AddSubjectAsync(subject);
            await _instructorRepository.AssociateInstructorWithSubjectAsync(professorId, subjectId);

            return ServiceResult.Success();
        }

        public async Task<ServiceResult<(Subject subject, IEnumerable<User> instructors)>> FindSubjectByURLAsync(string url)
        {
            var subjectDetails = await _subjectRepository.GetSubjectByURLAsync(url);

            if (subjectDetails.Subject == null)
            {
                return ServiceResult<(Subject, IEnumerable<User>)>.Failure("Subject not found.", 404);
            }

            // Previously mapped to ProfessorDTO. Now we directly construct Professor objects.
            var instructors = subjectDetails.SubjectInstructors.Select(professor => new User
            {
                Id = professor.Id,
                Email = professor.Email,
                Name = professor.Name,
                Surname = professor.Surname,
                PasswordHash = professor.PasswordHash,
                ProfilePicture = professor.ProfilePicture,
                OAuthId = professor.OAuthId,
                CreatedAt = professor.CreatedAt,
                IsVerified = professor.IsVerified
            }).ToList();

            return ServiceResult<(Subject, IEnumerable<User>)>.Success((subjectDetails.Subject, instructors));
        }

        public async Task<ServiceResult<IEnumerable<Subject>>> FindAllSubjectsAsync()
        {
            var subjects = await _subjectRepository.GetAllSubjectsAsnyc();
            return ServiceResult<IEnumerable<Subject>>.Success(subjects);
        }

        public async Task<ServiceResult<IEnumerable<Subject>>> FindAllSubjectsForInstructorAsync(int instructorId)
        {
            var subjects = await _subjectRepository.GetAllSubjectsForInstructorAsync(instructorId);
            return ServiceResult<IEnumerable<Subject>>.Success(subjects);
        }
    }
}
