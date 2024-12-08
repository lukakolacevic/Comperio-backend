using dotInstrukcijeBackend.HelperFunctions;
using dotInstrukcijeBackend.Interfaces.RepositoryInterfaces;
using dotInstrukcijeBackend.Interfaces.Service;
using dotInstrukcijeBackend.Interfaces.Utility;
using dotInstrukcijeBackend.Models;
using dotInstrukcijeBackend.ProfilePictureSavingUtility;
using dotInstrukcijeBackend.ServiceResultUtility;
using dotInstrukcijeBackend.ViewModels;

namespace dotInstrukcijeBackend.Services
{
    public class ProfessorService : IProfessorService
    {
        private readonly IProfessorRepository _professorRepository;
        private readonly ISubjectRepository _subjectRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IProfilePhotoSaver _profilePhotoSaver;
        private readonly ITokenService _tokenService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProfessorService(
            IProfessorRepository professorRepository,
            ISubjectRepository subjectRepository,
            IConfiguration configuration,
            IPasswordHasher passwordHasher,
            IProfilePhotoSaver profilePhotoSaver,
            ITokenService tokenService,
            IHttpContextAccessor httpContextAccessor)
        {
            _professorRepository = professorRepository;
            _subjectRepository = subjectRepository;
            _passwordHasher = passwordHasher;
            _profilePhotoSaver = profilePhotoSaver;
            _tokenService = tokenService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ServiceResult> RegisterProfessorAsync(RegistrationModel model)
        {
            if (await _professorRepository.GetProfessorByEmailAsync(model.Email) != null)
            {
                return ServiceResult.Failure("Email is already in use.", 400);
            }

            var professor = new Professor
            {
                Email = model.Email,
                Name = model.Name,
                Surname = model.Surname,
                Password = _passwordHasher.HashPassword(model.Password),
                ProfilePicture = await _profilePhotoSaver.SaveProfilePictureAsync(model.ProfilePicture)
            };

            await _professorRepository.AddProfessorAsync(professor);
            return ServiceResult.Success();
        }

        public async Task<ServiceResult<(Professor, string, string)>> LoginProfessorAsync(LoginModel model)
        {
            var professor = await _professorRepository.GetProfessorByEmailAsync(model.Email);
            if (professor == null)
            {
                return ServiceResult<(Professor, string, string)>.Failure("Professor not found.", 404);
            }

            if (!_passwordHasher.VerifyPassword(professor.Password, model.Password))
            {
                return ServiceResult<(Professor, string, string)>.Failure("Invalid password.", 401);
            }

            var accessToken = _tokenService.GenerateAccessToken(professor);
            var refreshToken = _tokenService.GenerateRefreshToken(professor);

            // Return the professor object directly.
            return ServiceResult<(Professor, string, string)>.Success((professor, accessToken, refreshToken));
        }

        public async Task<ServiceResult<Professor>> FindProfessorByEmailAsync(string email)
        {
            var professor = await _professorRepository.GetProfessorByEmailAsync(email);
            if (professor == null)
            {
                return ServiceResult<Professor>.Failure("Professor not found.", 404);
            }

            // Return the professor object directly.
            return ServiceResult<Professor>.Success(professor);
        }

        public async Task<ServiceResult<IEnumerable<Subject>>> FindAllSubjectsForProfessorAsync(int professorId)
        {
            var subjects = await _subjectRepository.GetAllSubjectsForProfessorAsync(professorId);
            return ServiceResult<IEnumerable<Subject>>.Success(subjects);
        }

        public async Task<ServiceResult<IEnumerable<Professor>>> FindAllProfessorsAsync()
        {
            var professors = await _professorRepository.GetAllProfessorsAsync();
            // Return the professors list directly.
            return ServiceResult<IEnumerable<Professor>>.Success(professors);
        }

        public async Task<ServiceResult<IEnumerable<Professor>>> FindTopFiveProfessorsByInstructionsCountAsync()
        {
            var listOfProfessors = await _professorRepository.GetTopFiveProfessorsByInstructionsCountAsync();
            // Return the professors list directly.
            return ServiceResult<IEnumerable<Professor>>.Success(listOfProfessors);
        }

        public async Task<ServiceResult> RemoveProfessorFromSubjectAsync(int professorid, int subjectId)
        {
            var professor = await _professorRepository.GetProfessorByIdAsync(professorid);
            if (professor == null)
            {
                return ServiceResult.Failure("Professor not found.", 404);
            }

            var subject = await _subjectRepository.GetSubjectByIdAsync(subjectId);
            if (subject == null)
            {
                return ServiceResult.Failure("Subject not found.", 404);
            }

            if (!await _professorRepository.IsProfessorTeachingSubject(professorid, subjectId))
            {
                return ServiceResult.Failure("The professor is not teaching this subject.", 400);
            }

            await _professorRepository.DeleteProfessorFromSubjectAsync(professorid, subjectId);
            return ServiceResult.Success();
        }

        public async Task<ServiceResult> JoinProfessorToSubject(int professorId, int subjectId)
        {
            var professor = await _professorRepository.GetProfessorByIdAsync(professorId);
            if (professor == null)
            {
                return ServiceResult.Failure("Professor not found.", 404);
            }

            var subject = await _subjectRepository.GetSubjectByIdAsync(subjectId);
            if (subject == null)
            {
                return ServiceResult.Failure("Subject not found.", 404);
            }

            if (await _professorRepository.IsProfessorTeachingSubject(professorId, subjectId))
            {
                return ServiceResult.Failure("The professor is already teaching this subject.", 400);
            }

            await _professorRepository.AssociateProfessorWithSubjectAsync(professorId, subjectId);
            return ServiceResult.Success();
        }

        public async Task<ServiceResult> ConfirmEmailAsync(int id)
        {
            await _professorRepository.SetEmailVerifiedAsync(id);
            return ServiceResult.Success();
        }
    }
}
