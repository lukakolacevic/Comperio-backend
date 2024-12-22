using dotInstrukcijeBackend.DataTransferObjects;
using dotInstrukcijeBackend.Models;

namespace dotInstrukcijeBackend.Interfaces.RepositoryInterfaces
{
    public interface IInstructorRepository
    {
        

        Task AssociateInstructorWithSubjectAsync(int instructorId, int subjectId);
        //Task<bool> CanAssociateMoreSubjects(int professorId);
        Task<IEnumerable<User>> GetTopFiveInstructorsBySessionCountAsync();
        Task DeleteInstructorFromSubjectAsync(int instructorId, int subjectId);
        Task<bool> IsInstructorTeachingSubject(int instructorId, int subjectId);

        Task<IEnumerable<InstructorFrequencyDTO>> GetTopFiveRequestedProfessorsAsync(int studentId);
        Task InitializeInstructorStats(int instructorId);
    }
}
