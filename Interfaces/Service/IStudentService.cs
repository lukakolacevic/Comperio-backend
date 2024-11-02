using dotInstrukcijeBackend.ViewModels;
using dotInstrukcijeBackend.ServiceResultUtility;
using dotInstrukcijeBackend.DataTransferObjects;
using dotInstrukcijeBackend.Models;

namespace dotInstrukcijeBackend.Interfaces.ServiceInterfaces
{
    public interface IStudentService
    {
        Task<ServiceResult> RegisterStudentAsync(StudentRegistrationModel model);

        Task<ServiceResult<(StudentDTO student, string token)>> LoginStudentAsync(LoginModel model);

        Task<ServiceResult<StudentDTO>> FindStudentByEmailAsync(string email);

        Task<ServiceResult<IEnumerable<StudentDTO>>> FindAllStudentsAsync();

        Task<ServiceResult<IEnumerable<SubjectFrequencyDTO>>> FindTopFiveRequestedSubjectsAsync(int studentId);
        Task<ServiceResult<IEnumerable<ProfessorFrequencyDTO>>> FindTopFiveRequestedProfessorsAsync(int studentId);
    }
}
