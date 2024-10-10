﻿using Dapper;
using dotInstrukcijeBackend.Interfaces;
using dotInstrukcijeBackend.Models;
using System.Data;

namespace dotInstrukcijeBackend.Repositories
{
    public class ProfessorRepository : IProfessorRepository
    {
        private readonly IDbConnection _connection;

        public ProfessorRepository(IDbConnection connection) 
        {  
            _connection = connection;
        }

        public async Task<Professor> GetProfessorByEmailAsync(string email)
        {
            const string query = @"SELECT id, email, name, surname, password, profile_picture 
                                 FROM professor WHERE email = @Email;";


            return await _connection.QueryFirstOrDefaultAsync<Professor>(query, new { Email = email });
        }

        public async Task AddProfessorAsync(Professor professor)
        {
            const string query = @"INSERT INTO professor (email, name, surname, password, profile_Picture, instructions_count) 
                                 VALUES 
                                 (@Email, @Name, @Surname, @Password, @ProfilePicture, @InstructionsCount);";

            await _connection.ExecuteAsync(query, new
            {
                Email = professor.Email,
                Name = professor.Name,
                Surname = professor.Surname,
                Password = professor.Password,
                ProfilePicture = professor.ProfilePicture,
                InstructionsCount = professor.InstructionsCount,
                //Subjects = professor.subjects
            });
        }

        public async Task<IEnumerable<Professor>> GetAllProfessorsAsync()
        {
            const string query = @"SELECT id, email, name, surname, password, profile_picture, instructions_count
                                 FROM professor;";

            return await _connection.QueryAsync<Professor>(query);
        }

        public async Task AssociateProfessorWithSubjectAsync(int professorId, int subjectId)
        {
            const string query = @"INSERT INTO professor_subject (professor_id, subject_id) 
                                 VALUES (@ProfessorId, @SubjectId);";

            await _connection.ExecuteAsync(query, new { ProfessorId = professorId, SubjectId = subjectId });
        }

        public async Task<IEnumerable<Professor>> GetTopFiveProfessorsByInstructionsCountAsync()
        {
            const string query = @"SELECT id, email, name, surname, password, profile_picture, instructions_count FROM professor
                                    ORDER BY instructions_count DESC LIMIT 5;";

            return await _connection.QueryAsync<Professor>(query);
        }

        
    }
}
