using dotInstrukcijeBackend.DataTransferObjects;
using dotInstrukcijeBackend.Models;

namespace dotInstrukcijeBackend.Interfaces.RepositoryInterfaces
{
    public interface IProfessorRepository
    {
        Task<Professor> GetProfessorByEmailAsync(string email);

        Task AddProfessorAsync(Professor professor);

        Task<IEnumerable<Professor>> GetAllProfessorsAsync();

        Task AssociateProfessorWithSubjectAsync(int professorId, int subjectId);

        //Task<bool> CanAssociateMoreSubjects(int professorId);

        Task<IEnumerable<Professor>> GetTopFiveProfessorsByInstructionsCountAsync();

        Task DeleteProfessorFromSubjectAsync(int professorId, int subjectId);

        Task<Professor> GetProfessorByIdAsync(int id);

        Task<bool> IsProfessorTeachingSubject(int professorId, int subjectId);

        Task<IEnumerable<ProfessorFrequencyDTO>> GetTopFiveRequestedProfessorsAsync(int studentId);
        Task SetEmailVerifiedAsync(int id);
    }
}
