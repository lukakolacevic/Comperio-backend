using Dapper;
using dotInstrukcijeBackend.Interfaces.RepositoryInterfaces;
using dotInstrukcijeBackend.Models;
using System.Data;

namespace dotInstrukcijeBackend.Repositories
{
    public class StudentRepository : IStudentRepository
    {
        private readonly IDbConnection _connection;

        public StudentRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task<bool> CanScheduleMoreSessionsAsync(int studentId)
        {
            const string query = @"SELECT COUNT(*) FROM session where StudentId = @StudentId AND status = 'Pending' 
                                    AND DateTime > CURRENT_TIMESTAMP;";

            var numberOfSessions = await _connection.ExecuteScalarAsync<int>(query, new {StudentId = studentId});

            return numberOfSessions < 5;
        }
    }
}
