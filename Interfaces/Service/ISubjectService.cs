using dotInstrukcijeBackend.DataTransferObjects;
using dotInstrukcijeBackend.Models;
using dotInstrukcijeBackend.ServiceResultUtility;
using dotInstrukcijeBackend.ViewModels;

namespace dotInstrukcijeBackend.Interfaces.Service
{
    public interface ISubjectService
    {
        Task<ServiceResult> CreateSubjectAsync(SubjectRegistrationModel request, int professorId);
        Task<ServiceResult<(Subject subject, IEnumerable<Professor> professors)>> FindSubjectByURLAsync(string url);
        Task<ServiceResult<IEnumerable<Subject>>> FindAllSubjectsAsync();
        Task<ServiceResult<IEnumerable<Subject>>> FindAllSubjectsForProfessorAsync(int professorId);
    }
}
