//using dotInstrukcijeBackend.HelperFunctions;
using dotInstrukcijeBackend.Interfaces.Repository;
using dotInstrukcijeBackend.Interfaces.RepositoryInterfaces;
using dotInstrukcijeBackend.Interfaces.Service;
using dotInstrukcijeBackend.Interfaces.Utility;
using dotInstrukcijeBackend.Models;
using dotInstrukcijeBackend.ProfilePictureSavingUtility;
using dotInstrukcijeBackend.ServiceResultUtility;
using dotInstrukcijeBackend.ViewModels;

namespace dotInstrukcijeBackend.Services
{
    public class InstructorService : IInstructorService
    {
        private readonly IInstructorRepository _instructorRepository;
        private readonly ISubjectRepository _subjectRepository;
       
        private readonly IUserRepository _userRepository;

        public InstructorService(
            IInstructorRepository instructorRepository,
            ISubjectRepository subjectRepository,
            IUserRepository userRepository)
        {
            _instructorRepository = instructorRepository;
            _subjectRepository = subjectRepository;
            _userRepository = userRepository;
        }

       
        public async Task<ServiceResult<IEnumerable<Subject>>> FindAllSubjectsForInstructorAsync(int instructorId)
        {
            var subjects = await _subjectRepository.GetAllSubjectsForInstructorAsync(instructorId);
            return ServiceResult<IEnumerable<Subject>>.Success(subjects);
        }

        
        public async Task<ServiceResult<IEnumerable<User>>> FindTopFiveInstructorsBySessionCountAsync()
        {
            var listOfProfessors = await _instructorRepository.GetTopFiveInstructorsBySessionCountAsync();
            // Return the professors list directly.
            return ServiceResult<IEnumerable<User>>.Success(listOfProfessors);
        }

        public async Task<ServiceResult> RemoveInstructorFromSubjectAsync(int instructorId, int subjectId)
        {
            var professor = await _userRepository.GetUserByIdAsync(instructorId);
            if (professor == null)
            {
                return ServiceResult.Failure("Instructor not found.", 404);
            }

            var subject = await _subjectRepository.GetSubjectByIdAsync(subjectId);
            if (subject == null)
            {
                return ServiceResult.Failure("Subject not found.", 404);
            }

            if (!await _instructorRepository.IsInstructorTeachingSubject(instructorId, subjectId))
            {
                return ServiceResult.Failure("The instructor is not teaching this subject.", 400);
            }

            await _instructorRepository.DeleteInstructorFromSubjectAsync(instructorId, subjectId);
            return ServiceResult.Success();
        }

        public async Task<ServiceResult> JoinInstructorToSubject(int instructorId, int subjectId)
        {
            var professor = await _userRepository.GetUserByIdAsync(instructorId);
            if (professor == null)
            {
                return ServiceResult.Failure("Instructor not found.", 404);
            }

            var subject = await _subjectRepository.GetSubjectByIdAsync(subjectId);
            if (subject == null)
            {
                return ServiceResult.Failure("Subject not found.", 404);
            }

            if (await _instructorRepository.IsInstructorTeachingSubject(instructorId, subjectId))
            {
                return ServiceResult.Failure("The instructor is already teaching this subject.", 400);
            }

            await _instructorRepository.AssociateInstructorWithSubjectAsync(instructorId, subjectId);
            return ServiceResult.Success();
        }

        
    }
}
