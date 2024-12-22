using dotInstrukcijeBackend.DataTransferObjects;
using dotInstrukcijeBackend.Models;
using dotInstrukcijeBackend.ServiceResultUtility;
using dotInstrukcijeBackend.ViewModels;

namespace dotInstrukcijeBackend.Interfaces.Service
{
    public interface IInstructorService
    {
        Task<ServiceResult<IEnumerable<Subject>>> FindAllSubjectsForInstructorAsync(int instructorId);
        Task<ServiceResult<IEnumerable<User>>> FindTopFiveInstructorsBySessionCountAsync();
        Task<ServiceResult> RemoveInstructorFromSubjectAsync(int instructorId, int subjectId);
        Task<ServiceResult> JoinInstructorToSubject(int instructorId, int subjectId);
      

    }
}
