using dotInstrukcijeBackend.DataTransferObjects;
using dotInstrukcijeBackend.Models;
using dotInstrukcijeBackend.ServiceResultUtility;
using dotInstrukcijeBackend.ViewModels;

namespace dotInstrukcijeBackend.Interfaces.Service
{
    public interface IProfessorService
    {
        Task<ServiceResult> RegisterProfessorAsync(ProfessorRegistrationModel model);
        Task<ServiceResult<(ProfessorDTO professor, string token)>> LoginProfessorAsync(LoginModel model);
        Task<ServiceResult<ProfessorDTO>> FindProfessorByEmailAsync(string email);
        Task<ServiceResult<IEnumerable<Subject>>> FindAllSubjectsForProfessorAsync(int professorId);
        Task<ServiceResult<IEnumerable<ProfessorDTO>>> FindTopFiveProfessorsByInstructionsCountAsync();
        Task<ServiceResult> RemoveProfessorFromSubjectAsync(int professorid, int subjectId);
        Task<ServiceResult> JoinProfessorToSubject(int professorId, int subjectId);
    }
}
