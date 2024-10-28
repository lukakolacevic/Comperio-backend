using dotInstrukcijeBackend.Models;

namespace dotInstrukcijeBackend.Interfaces
{
    public interface IProfessorRepository
    {
        Task<Professor> GetProfessorByEmailAsync(string email);

        Task AddProfessorAsync(Professor professor);

        Task<IEnumerable<Professor>> GetAllProfessorsAsync();

        Task AssociateProfessorWithSubjectAsync(int professorId, int subjectId);

        //Task<bool> CanAssociateMoreSubjects(int professorId);

        Task<IEnumerable<Professor>> GetTopFiveProfessorsByInstructionsCountAsync();

        Task RemoveProfessorFromSubjectAsync(int professorId, int subjectId);

        Task<Professor> GetProfessorByIdAsync(int id);

        Task<bool> IsProfessorTeachingSubject(int professorId, int subjectId);

        Task JoinProfessorToSubjectAsync(int professorId, int subjectId);
    }
}
