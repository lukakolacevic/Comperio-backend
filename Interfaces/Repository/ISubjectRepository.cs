using dotInstrukcijeBackend.DataTransferObjects;
using dotInstrukcijeBackend.Models;

namespace dotInstrukcijeBackend.Interfaces.RepositoryInterfaces
{
    public interface ISubjectRepository
    {
        Task<int> AddSubjectAsync(Subject subject);

        Task<IEnumerable<Subject>> GetAllSubjectsAsnyc();

        Task<Subject> GetSubjectByTitleAsync(string title);

        Task<SubjectDetailsDTO> GetSubjectByURLAsync(string url);

        Task<IEnumerable<Subject>> GetAllSubjectsForInstructorAsync(int instructorId);

        Task<Subject> GetSubjectByIdAsync(int id);
        Task<IEnumerable<SubjectFrequencyDTO>> GetTopFiveRequestedSubjectsAsync(int studentId);
    }
}
