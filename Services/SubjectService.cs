using dotInstrukcijeBackend.Models;
using dotInstrukcijeBackend.DataTransferObjects;
using dotInstrukcijeBackend.HelperFunctions;
using dotInstrukcijeBackend.Interfaces.RepositoryInterfaces;
using dotInstrukcijeBackend.Interfaces.Service;
using dotInstrukcijeBackend.ServiceResultUtility;
using dotInstrukcijeBackend.ViewModels;
namespace dotInstrukcijeBackend.Services
{
    public class SubjectService : ISubjectService
    {
        private readonly ISubjectRepository _subjectRepository;
        private readonly IProfessorRepository _professorRepository;

        public SubjectService(ISubjectRepository subjectRepository, IProfessorRepository professorRepository)
        {
            _subjectRepository = subjectRepository;
            _professorRepository = professorRepository;
        }

        public async Task<ServiceResult> CreateSubjectAsync(SubjectRegistrationModel request, int professorId)
        {
            // Check if a subject with the given title or URL already exists
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

            // Create the subject and associate it with the professor
            var subjectId = await _subjectRepository.AddSubjectAsync(subject);
            await _professorRepository.AssociateProfessorWithSubjectAsync(professorId, subjectId);

            return ServiceResult.Success();
        }

        public async Task<ServiceResult<(Subject subject, IEnumerable<ProfessorDTO> professors)>> FindSubjectByURLAsync(string url)
        {
            var subjectDetails = await _subjectRepository.GetSubjectByURLAsync(url);

            if (subjectDetails.Subject == null)
            {
                return ServiceResult<(Subject, IEnumerable<ProfessorDTO>)>.Failure("Subject not found.", 404);
            }

            var professors = subjectDetails.SubjectProfessors.Select(professor => MapToProfessorDTO.mapToProfessorDTO(professor)).ToList();
            return ServiceResult<(Subject, IEnumerable<ProfessorDTO>)>.Success((subjectDetails.Subject, professors));
        }

        public async Task<ServiceResult<IEnumerable<Subject>>> FindAllSubjectsAsync()
        {
            var subjects = await _subjectRepository.GetAllSubjectsAsnyc();
            return ServiceResult<IEnumerable<Subject>>.Success(subjects);
        }

        public async Task<ServiceResult<IEnumerable<Subject>>> FindAllSubjectsForProfessorAsync(int professorId)
        {
            var subjects = await _subjectRepository.GetAllSubjectsForProfessorAsync(professorId);
            return ServiceResult<IEnumerable<Subject>>.Success(subjects);
        }
    }
}

