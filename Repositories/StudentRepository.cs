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

        public async Task<Student> GetStudentByEmailAsync(string email)
        {
            const string query = @"SELECT id, email, name, surname, password, profile_picture
                                 FROM student WHERE email = @Email";

            return await _connection.QueryFirstOrDefaultAsync<Student>(query, new { Email = email });
        }

        public async Task AddStudentAsync(Student student)
        {
            const string query = @"INSERT INTO student (email, name, surname, password, profile_picture) 
                                   VALUES (@Email, @Name, @Surname, @Password, @ProfilePicture)";

            await _connection.ExecuteAsync(query,
                new { Email = student.Email, Name = student.Name, Surname = student.Surname, 
                    Password = student.Password, ProfilePicture = student.ProfilePicture
                });
        }

        public async Task<IEnumerable<Student>> GetAllStudentsAsync()
        {
            const string query = @"SELECT id, email, name, surname, password, profile_picture 
                                 FROM student";

            return await _connection.QueryAsync<Student>(query);
        }

        public async Task<bool> CanScheduleMoreSessionsAsync(int studentId)
        {
            const string query = @"SELECT COUNT(*) FROM session where student_id = @StudentId AND status = 'Pending' 
                                    AND date_time > CURRENT_TIMESTAMP;";

            var numberOfSessions = await _connection.ExecuteScalarAsync<int>(query, new {StudentId = studentId});

            return numberOfSessions < 5;
        }
    }
}
