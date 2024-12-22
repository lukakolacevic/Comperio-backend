using dotInstrukcijeBackend.Models;

namespace dotInstrukcijeBackend.Interfaces.RepositoryInterfaces
{
    public interface IStudentRepository
    {
        Task<bool> CanScheduleMoreSessionsAsync(int studentId);
    }
}
