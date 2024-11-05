using dotInstrukcijeBackend.Models;

namespace dotInstrukcijeBackend.Interfaces.RepositoryInterfaces
{
    public interface IStudentRepository
    {
        Task<Student> GetStudentByEmailAsync(string email);

        Task AddStudentAsync(Student student);

        Task<IEnumerable<Student>> GetAllStudentsAsync();

        Task<bool> CanScheduleMoreSessionsAsync(int studentId);
        Task<Student> GetStudentByIdAsync(int id);
    }
}
