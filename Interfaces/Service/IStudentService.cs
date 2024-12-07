using dotInstrukcijeBackend.ViewModels;
using dotInstrukcijeBackend.ServiceResultUtility;
using dotInstrukcijeBackend.DataTransferObjects;
using dotInstrukcijeBackend.Models;

namespace dotInstrukcijeBackend.Interfaces.ServiceInterfaces
{
    public interface IStudentService
    {
        Task<ServiceResult> RegisterStudentAsync(RegistrationModel model);

        Task<ServiceResult<(StudentDTO, string, string)>> LoginStudentAsync(LoginModel model);
        Task<ServiceResult<StudentDTO>> FindStudentByEmailAsync(string email);

        Task<ServiceResult<IEnumerable<StudentDTO>>> FindAllStudentsAsync();

        Task<ServiceResult<IEnumerable<SubjectFrequencyDTO>>> FindTopFiveRequestedSubjectsAsync(int studentId);
        Task<ServiceResult<IEnumerable<ProfessorFrequencyDTO>>> FindTopFiveRequestedProfessorsAsync(int studentId);
        Task<ServiceResult> ConfirmEmailAsync(int id);
    }
}
