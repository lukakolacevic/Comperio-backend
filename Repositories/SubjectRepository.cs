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
        INSERT INTO subject (title, url, description) VALUES (@Title, @Url, @Description)
        RETURNING id;";  // SQLite specific SQL to return the last inserted row ID

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
            const string query = "SELECT * FROM subject ORDER BY title;";

            return await _connection.QueryAsync<Subject>(query);
        }

        public async Task<Subject> GetSubjectByTitleAsync(string title)
        {
            const string query = "SELECT * FROM subject WHERE title = @Title";

            return await _connection.QueryFirstOrDefaultAsync<Subject>(query, new { Title = title });

        }

        public async Task<SubjectDetailsDTO> GetSubjectByURLAsync(string url)
        {
            const string query = @"
            SELECT * FROM subject WHERE url = @Url; 
            SELECT p.* FROM professor p
            INNER JOIN professor_subject ps ON p.id = ps.professor_id
            INNER JOIN subject s ON s.id = ps.subject_id
            WHERE s.url = @Url;";

            using (var multi = await _connection.QueryMultipleAsync(query, new { Url = url }))
            {
                var subject = await multi.ReadSingleOrDefaultAsync<Subject>();
                var professors = await multi.ReadAsync<Professor>();

                var professorsList = professors.ToList();
                return new SubjectDetailsDTO
                {
                    Subject = subject,
                    SubjectProfessors = professorsList
                };
            }
        }

        public async Task<IEnumerable<Subject>> GetAllSubjectsForProfessorAsync(int professorId)
        {
            const string query = @"SELECT s.* FROM professor p JOIN professor_subject ps ON p.id = ps.professor_id
                                    JOIN subject s ON ps.subject_id = s.id WHERE p.id = @Professor_id;";


            return await _connection.QueryAsync<Subject>(query, new { Professor_id = professorId });
        }

        public async Task<Subject> GetSubjectByIdAsync(int id)
        {
            const string query = @"SELECT * FROM subject WHERE id = @Id";
            var subject = await _connection.QueryFirstOrDefaultAsync<Subject>(query, new { Id = id });

            return subject;
        }

        public async Task<IEnumerable<SubjectFrequencyDTO>> GetTopFiveRequestedSubjectsAsync(int studentId)
        {
            const string query = @"SELECT * FROM most_chosen_subjects_per_student WHERE student_id = @StudentId;";

            var listOfMostChosenSubjects = await _connection.QueryAsync<SubjectFrequencyDTO>(query, new { StudentId = studentId });
            
            return listOfMostChosenSubjects;
        }
    }
}
