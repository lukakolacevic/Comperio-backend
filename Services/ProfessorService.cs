using dotInstrukcijeBackend.DataTransferObjects;
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
        private readonly IConfiguration _configuration;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IProfilePhotoSaver _profilePhotoSaver;
        private readonly IJWTTokenGenerator _jwtTokenGenerator;


        public ProfessorService(
            IProfessorRepository professorRepository,
            ISubjectRepository subjectRepository,
            IConfiguration configuration,
            IPasswordHasher passwordHasher,
            IProfilePhotoSaver profilePhotoSaver,
            IJWTTokenGenerator jwtTokenGenerator)
        {
            _professorRepository = professorRepository;
            _subjectRepository = subjectRepository;
            _configuration = configuration;
            _passwordHasher = passwordHasher;
            _profilePhotoSaver = profilePhotoSaver;
            _jwtTokenGenerator = jwtTokenGenerator;
        }


        public async Task<ServiceResult> RegisterProfessorAsync(ProfessorRegistrationModel model)
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

            var prof = await _professorRepository.GetProfessorByEmailAsync(professor.Email);
            int profId = prof.Id;

            if (model.Subjects is not null)
            {
                foreach (var subjectURL in model.Subjects)
                {
                    var subjectDetails = await _subjectRepository.GetSubjectByURLAsync(subjectURL);

                    int subjectId = subjectDetails.Subject.Id;

                    await _professorRepository.AssociateProfessorWithSubjectAsync(profId, subjectId);
                }
            }

            return ServiceResult.Success();
        }


        public async Task<ServiceResult<(ProfessorDTO professor, string token)>> LoginProfessorAsync(LoginModel model)
        {
            var professor = await _professorRepository.GetProfessorByEmailAsync(model.Email);
            if (professor == null)
            {
                return ServiceResult<(ProfessorDTO professor, string token)>.Failure("Professor not found.", 404);
            }

            if (!_passwordHasher.VerifyPassword(professor.Password, model.Password))
            {
                return ServiceResult<(ProfessorDTO professor, string token)>.Failure("Invalid password.", 401);
            }

            var token = _jwtTokenGenerator.GenerateJwtToken(professor, _configuration);
            string? profilePhotoBase64String = professor.ProfilePicture != null
                ? Convert.ToBase64String(professor.ProfilePicture)
                : null;

            var professorDTO = new ProfessorDTO
            {
                ProfessorId = professor.Id,
                Name = professor.Name,
                Surname = professor.Surname,
                ProfilePictureBase64String = profilePhotoBase64String,
                InstructionsCount = professor.InstructionsCount,
            };

            return ServiceResult<(ProfessorDTO professor, string token)>.Success((professorDTO, token));
        }

        public async Task<ServiceResult<ProfessorDTO>> FindProfessorByEmailAsync(string email)
        {
            var professor = await _professorRepository.GetProfessorByEmailAsync(email);
            if (professor == null)
            {
                return ServiceResult<ProfessorDTO>.Failure("Professor not found.", 404);
            }

            string? profilePhotoBase64String = professor.ProfilePicture != null
                ? Convert.ToBase64String(professor.ProfilePicture)
                : null;

            var professorDTO = new ProfessorDTO
            {
                ProfessorId = professor.Id,
                Name = professor.Name,
                Surname = professor.Surname,
                ProfilePictureBase64String = profilePhotoBase64String,
                InstructionsCount = professor.InstructionsCount,
            };

            return ServiceResult<ProfessorDTO>.Success(professorDTO);
        }


        public async Task<ServiceResult<IEnumerable<Subject>>> FindAllSubjectsForProfessorAsync(int professorId)
        {
            var subjects = await _subjectRepository.GetAllSubjectsForProfessorAsync(professorId);
            return ServiceResult<IEnumerable<Subject>>.Success(subjects);
        }

        public async Task<ServiceResult<IEnumerable<ProfessorDTO>>> FindAllProfessorsAsync()
        {
            var professors = await _professorRepository.GetAllProfessorsAsync();
            var professorDTOs = professors.Select(professor => new ProfessorDTO
            {
                ProfessorId = professor.Id,
                Name = professor.Name,
                Surname = professor.Surname,
                ProfilePictureBase64String = professor.ProfilePicture != null
                ? Convert.ToBase64String(professor.ProfilePicture)
                : null,
                InstructionsCount = professor.InstructionsCount
            }
            );

            return ServiceResult<IEnumerable<ProfessorDTO>>.Success(professorDTOs);
        }


        public async Task<ServiceResult<IEnumerable<ProfessorDTO>>> FindTopFiveProfessorsByInstructionsCountAsync()
        {
            var listOfProfessors = await _professorRepository.GetTopFiveProfessorsByInstructionsCountAsync();

            var listOfProfessorsDTO = new List<ProfessorDTO>();

            foreach (var professor in listOfProfessors)
            {
                var professorDTO = MapToProfessorDTO.mapToProfessorDTO(professor);
                listOfProfessorsDTO.Add(professorDTO);
            }

            return ServiceResult<IEnumerable<ProfessorDTO>>.Success(listOfProfessorsDTO);
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
    }
}
