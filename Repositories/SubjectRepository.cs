using Dapper;
using dotInstrukcijeBackend.DataTransferObjects;
using dotInstrukcijeBackend.Interfaces.RepositoryInterfaces;
using dotInstrukcijeBackend.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Identity.Client;
using Org.BouncyCastle.Asn1;
using System.Collections;
using System.Data;

namespace dotInstrukcijeBackend.Repositories
{
    public class SubjectRepository : ISubjectRepository
    {
        private readonly IDbConnection _connection;

        public SubjectRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task<int> AddSubjectAsync(Subject subject) //stao ovdje, mijenjam u integer
        {
            const string query = @"
        INSERT INTO subject (Title, Url, Description) OUTPUT INSERTED.Id VALUES (@Title, @Url, @Description)";  

            // ExecuteScalarAsync is used here because we expect a single return value: the ID of the inserted row
            var subjectId = await _connection.ExecuteScalarAsync<int>(query, new
            {
                Title = subject.Title,
                Url = subject.Url,
                Description = subject.Description
            });

            return subjectId; // Return the ID of the newly created subject
        }

        public async Task<IEnumerable<Subject>> GetAllSubjectsAsnyc()
        {
            const string query = "SELECT * FROM subject ORDER BY Title;";

            return await _connection.QueryAsync<Subject>(query);
        }

        public async Task<Subject> GetSubjectByTitleAsync(string title)
        {
            const string query = "SELECT * FROM subject WHERE Title = @Title";

            return await _connection.QueryFirstOrDefaultAsync<Subject>(query, new { Title = title });

        }

        public async Task<SubjectDetailsDTO> GetSubjectByURLAsync(string url)
        {
            const string storedProcedure = "GetSubjectAndSubjectInstructors";

            using (var multi = await _connection.QueryMultipleAsync(storedProcedure, new { Url = url }, commandType: CommandType.StoredProcedure))
            {
                // Read the subject details
                var subject = await multi.ReadSingleOrDefaultAsync<Subject>();

                // Read the list of professors (users) associated with the subject
                var instructors = await multi.ReadAsync<User>();

                // Map the result to the DTO
                return new SubjectDetailsDTO
                {
                    Subject = subject,
                    SubjectInstructors = instructors.ToList()
                };
            }
        }


        public async Task<IEnumerable<Subject>> GetAllSubjectsForInstructorAsync(int instructorId)
        {
            const string query = @"SELECT s.* FROM ""user"" u JOIN ""instructorsubject"" isp ON u.Id = isp.InstructorId
                                    JOIN subject s ON isp.SubjectId = s.Id WHERE u.Id = @InstructorId;";


            return await _connection.QueryAsync<Subject>(query, new { InstructorId = instructorId });
        }

        public async Task<Subject> GetSubjectByIdAsync(int id)
        {
            const string query = @"SELECT * FROM subject WHERE id = @Id";
            var subject = await _connection.QueryFirstOrDefaultAsync<Subject>(query, new { Id = id });

            return subject;
        }

        public async Task<IEnumerable<SubjectFrequencyDTO>> GetTopFiveRequestedSubjectsAsync(int studentId)
        {
            var query = @"
        SELECT TOP 5 
            s.Title AS Title, 
            COUNT(sess.Id) AS SessionCount
        FROM session sess
        INNER JOIN Subject s ON sess.SubjectId = s.Id
        WHERE sess.StudentId = @StudentId
        GROUP BY s.Title
        ORDER BY COUNT(sess.Id) DESC";

            var listOfMostChosenSubjects = await _connection.QueryAsync<SubjectFrequencyDTO>(query, new { StudentId = studentId });
            
            return listOfMostChosenSubjects;
        }
    }
}
