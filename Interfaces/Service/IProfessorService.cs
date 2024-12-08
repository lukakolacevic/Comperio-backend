using dotInstrukcijeBackend.DataTransferObjects;
using dotInstrukcijeBackend.Models;
using dotInstrukcijeBackend.ServiceResultUtility;
using dotInstrukcijeBackend.ViewModels;

namespace dotInstrukcijeBackend.Interfaces.Service
{
    public interface IProfessorService
    {
        Task<ServiceResult> RegisterProfessorAsync(RegistrationModel model);
        Task<ServiceResult<(Professor, string, string)>> LoginProfessorAsync(LoginModel model);
        Task<ServiceResult<Professor>> FindProfessorByEmailAsync(string email);
        Task<ServiceResult<IEnumerable<Subject>>> FindAllSubjectsForProfessorAsync(int professorId);
        Task<ServiceResult<IEnumerable<Professor>>> FindTopFiveProfessorsByInstructionsCountAsync();
        Task<ServiceResult> RemoveProfessorFromSubjectAsync(int professorid, int subjectId);
        Task<ServiceResult> JoinProfessorToSubject(int professorId, int subjectId);
        Task<ServiceResult> ConfirmEmailAsync(int id);

    }
}
