using Dapper;
using dotInstrukcijeBackend.DataTransferObjects;
using dotInstrukcijeBackend.Interfaces.RepositoryInterfaces;
using dotInstrukcijeBackend.Models;
using Humanizer.Localisation.TimeToClockNotation;
using Npgsql.Replication.PgOutput.Messages;
using System.Data;

namespace dotInstrukcijeBackend.Repositories
{
    public class InstructorRepository : IInstructorRepository
    {
        private readonly IDbConnection _connection;

        public InstructorRepository(IDbConnection connection) 
        {  
            _connection = connection;
        }


        public async Task AssociateInstructorWithSubjectAsync(int instructorId, int subjectId)
        {
            const string query = @"INSERT INTO instructorsubject (InstructorId, SubjectId) 
                                 VALUES (@InstructorId, @SubjectId);";

            await _connection.ExecuteAsync(query, new { InstructorId = instructorId, SubjectId = subjectId });
        }


        public async Task<IEnumerable<User>> GetTopFiveInstructorsBySessionCountAsync()
        {
            const string query = @"SELECT TOP 5 u.* FROM dbo.[user] AS u 
                                   JOIN dbo.[instructorstats] AS iss ON u.Id = iss.InstructorId
                                   ORDER BY SessionCount DESC; ";

            return await _connection.QueryAsync<User>(query);
        }



        public async Task DeleteInstructorFromSubjectAsync(int instructorId, int subjectId)
        {
            _connection.Open();

            using (var transaction = _connection.BeginTransaction())
            {
                try
                {
                    var query = @"DELETE FROM instructorsubject WHERE InstructorId = @InstructorId 
                          AND SubjectId = @SubjectId";
                    await _connection.ExecuteAsync(query,
                                                   new { InstructorId = instructorId, SubjectId = subjectId },
                                                   transaction: transaction); // Pass transaction here

                    query = @"UPDATE session SET status = 'Cancelled' WHERE InstructorId = @InstructorId 
                      AND SubjectId = @SubjectId";
                    await _connection.ExecuteAsync(query,
                                                   new { InstructorId = instructorId, SubjectId = subjectId },
                                                   transaction: transaction); // Pass transaction here

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw;
                }
                finally
                {
                    _connection.Close();
                }
            }
        }



        public async Task<bool> IsInstructorTeachingSubject(int instructorId, int subjectId)
        {
            const string query = @"
        SELECT CASE 
                   WHEN COUNT(1) > 0 THEN CAST(1 AS BIT) 
                   ELSE CAST(0 AS BIT) 
               END
        FROM instructorsubject
        WHERE InstructorId = @InstructorId AND SubjectId = @SubjectId;";

            return await _connection.ExecuteScalarAsync<bool>(query, new { InstructorId = instructorId, SubjectId = subjectId });
        }



        public async Task<IEnumerable<InstructorFrequencyDTO>> GetTopFiveRequestedProfessorsAsync(int studentId)
        {
            var query = @"
        SELECT TOP 5 
            u.Name AS Name, 
            u.Surname AS Surname, 
            COUNT(sess.Id) AS SessionCount
        FROM session sess
        INNER JOIN dbo.[user] u ON sess.InstructorId = u.Id
        WHERE sess.StudentId = @StudentId AND u.RoleId = 2
        GROUP BY u.Name, u.Surname
        ORDER BY COUNT(sess.Id) DESC";

            var listOfMostChosenInstructors = await _connection.QueryAsync<InstructorFrequencyDTO>(query, new { StudentId = studentId });

            return listOfMostChosenInstructors;
        }

        public async Task InitializeInstructorStats(int instructorId)
        {
            const string query = @"INSERT INTO instructorstats (InstructorId, SessionCount, Rating) VALUES (@InstructorId, 0, 0)";

            await _connection.ExecuteAsync(query, new { InstructorId = instructorId });
        }
    }
}
