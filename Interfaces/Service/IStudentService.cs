using dotInstrukcijeBackend.ViewModels;
using dotInstrukcijeBackend.ServiceResultUtility;
using dotInstrukcijeBackend.DataTransferObjects;
using dotInstrukcijeBackend.Models;

namespace dotInstrukcijeBackend.Interfaces.ServiceInterfaces
{
    public interface IStudentService
    {
        Task<ServiceResult<IEnumerable<SubjectFrequencyDTO>>> FindTopFiveRequestedSubjectsAsync(int studentId);
        Task<ServiceResult<IEnumerable<InstructorFrequencyDTO>>> FindTopFiveRequestedProfessorsAsync(int studentId);
    }
}
