using Dapper;
using dotInstrukcijeBackend.DataTransferObjects;
using dotInstrukcijeBackend.Interfaces.RepositoryInterfaces;
using dotInstrukcijeBackend.Models;
using System.Data;
using System.Linq;

namespace dotInstrukcijeBackend.Repositories
{
    public class SessionRepository : ISessionRepository
    {
        private readonly IDbConnection _connection;

        public SessionRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task AddSessionAysnc(Session session)
        {
            const string query = @"INSERT INTO session (student_id, professor_id, subject_id, date_time, status) 
                                  VALUES(@StudentId, @ProfessorId, @SubjectId, @DateTime, @Status)";

            await _connection.ExecuteAsync(query, new
            {
                StudentId = session.StudentId,
                ProfessorId = session.ProfessorId,
                SubjectId = session.SubjectId,
                DateTime = session.DateTime,
                Status = session.Status
            });
        }

        public async Task<SessionsDTO<SessionWithProfessorDTO>> GetAllStudentSessionsAsync(int studentId)
        {
            const string query = @"
    -- Pending sessions
    SELECT session.id AS ""id"", session.date_time, status, 
           professor.id AS ""id"", name, surname, profile_picture, instructions_count,
           subject.id AS ""id"", title     
    FROM session
    JOIN professor ON session.professor_id = professor.id
    JOIN subject ON session.subject_id = subject.id
    WHERE session.student_id = @StudentId AND session.status = 'Pending' AND session.date_time > CURRENT_TIMESTAMP;

    -- Upcoming sessions
    SELECT session.id AS ""id"", session.date_time, status, 
           professor.id AS ""id"", name, surname, profile_picture, instructions_count,
           subject.id AS ""id"", title     
    FROM session
    JOIN professor ON session.professor_id = professor.id
    JOIN subject ON session.subject_id = subject.id
    WHERE session.student_id = @StudentId AND session.status = 'Confirmed' AND session.date_time > CURRENT_TIMESTAMP;

    -- Past sessions
    SELECT session.id AS ""id"", session.date_time, status, 
           professor.id AS ""id"", name, surname, profile_picture, instructions_count,
           subject.id AS ""id"", title     
    FROM session
    JOIN professor ON session.professor_id = professor.id
    JOIN subject ON session.subject_id = subject.id
    WHERE session.student_id = @StudentId AND session.status = 'Confirmed' AND session.date_time <= CURRENT_TIMESTAMP;

    --Cancelled sessions
    SELECT session.id AS ""id"", session.date_time, status, 
           professor.id AS ""id"", name, surname, profile_picture, instructions_count,
           subject.id AS ""id"", title     
    FROM session
    JOIN professor ON session.professor_id = professor.id
    JOIN subject ON session.subject_id = subject.id
    WHERE session.student_id = @StudentId AND session.status = 'Cancelled'";

            using (var multi = await _connection.QueryMultipleAsync(query, new { StudentId = studentId }))
            {
                // Pending sessions
                var pendingSessions = multi.Read<Session, Professor, Subject, SessionWithProfessorDTO>((session, professor, subject) =>
                {
                    return new SessionWithProfessorDTO
                    {
                        SessionId = session.Id,
                        DateTime = session.DateTime,
                        Status = session.Status,
                        Professor = new Professor
                        {
                            Id = professor.Id,
                            Name = professor.Name,
                            Surname = professor.Surname,
                            ProfilePicture = professor.ProfilePicture,
                            InstructionsCount = professor.InstructionsCount
                        },
                        Subject = new Subject
                        {
                            Id = subject.Id,
                            Title = subject.Title
                        }
                    };
                }, splitOn: "id,id").ToList();

                // Upcoming sessions
                var upcomingSessions = multi.Read<Session, Professor, Subject, SessionWithProfessorDTO>((session, professor, subject) =>
                {
                    return new SessionWithProfessorDTO
                    {
                        SessionId = session.Id,
                        DateTime = session.DateTime,
                        Status = session.Status,
                        Professor = new Professor
                        {
                            Id = professor.Id,
                            Name = professor.Name,
                            Surname = professor.Surname,
                            ProfilePicture = professor.ProfilePicture,
                            InstructionsCount = professor.InstructionsCount
                        },
                        Subject = new Subject
                        {
                            Id = subject.Id,
                            Title = subject.Title
                        }
                    };
                }, splitOn: "id,id").ToList();

                // Past sessions
                var pastSessions = multi.Read<Session, Professor, Subject, SessionWithProfessorDTO>((session, professor, subject) =>
                {
                    return new SessionWithProfessorDTO
                    {
                        SessionId = session.Id,
                        DateTime = session.DateTime,
                        Status = session.Status,
                        Professor = new Professor
                        {
                            Id = professor.Id,
                            Name = professor.Name,
                            Surname = professor.Surname,
                            ProfilePicture = professor.ProfilePicture,
                            InstructionsCount = professor.InstructionsCount
                        },
                        Subject = new Subject
                        {
                            Id = subject.Id,
                            Title = subject.Title
                        }
                    };
                }, splitOn: "id,id").ToList();

                var cancelledSessions = multi.Read<Session, Professor, Subject, SessionWithProfessorDTO>((session, professor, subject) =>
                {
                    return new SessionWithProfessorDTO
                    {
                        SessionId = session.Id,
                        DateTime = session.DateTime,
                        Status = session.Status,
                        Professor = new Professor
                        {
                            Id = professor.Id,
                            Name = professor.Name,
                            Surname = professor.Surname,
                            ProfilePicture = professor.ProfilePicture,
                            InstructionsCount = professor.InstructionsCount
                        },
                        Subject = new Subject
                        {
                            Id = subject.Id,
                            Title = subject.Title
                        }
                    };
                }, splitOn: "id,id").ToList();

                return new SessionsDTO<SessionWithProfessorDTO>
                {
                    PastSessions = pastSessions,
                    PendingSessions = pendingSessions,
                    UpcomingSessions = upcomingSessions,
                    CancelledSessions = cancelledSessions
                };
            }
        }

        public async Task<SessionsDTO<SessionWithStudentDTO>> GetAllSessionsForProfessorAsync(int professorId)
        {
            const string query = @"
        --Past sessions
        SELECT session.id, date_time, status, student.id, student.name, student.surname, student.profile_picture, subject.id, title
        FROM session 
        JOIN student ON session.student_id = student.id 
        JOIN professor ON session.professor_id = professor.id 
        JOIN subject ON subject.id = session.subject_id
        WHERE session.professor_id = @ProfessorId AND session.status = 'Confirmed' AND session.date_time <= CURRENT_TIMESTAMP;
        
        --Pending sessions
        SELECT session.id, date_time, status, student.id, student.name, student.surname, student.profile_picture, subject.id, title
        FROM session 
        JOIN student ON session.student_id = student.id 
        JOIN professor ON session.professor_id = professor.id 
        JOIN subject ON subject.id = session.subject_id
        WHERE session.professor_id = @ProfessorId AND session.status = 'Pending' AND session.date_time > CURRENT_TIMESTAMP;

        --Upcoming confirmed sessions
        SELECT session.id, date_time, status, student.id, student.name, student.surname, student.profile_picture, subject.id, title
        FROM session 
        JOIN student ON session.student_id = student.id 
        JOIN professor ON session.professor_id = professor.id 
        JOIN subject ON subject.id = session.subject_id
        WHERE session.professor_id = @ProfessorId AND session.status = 'Confirmed' AND session.date_time > CURRENT_TIMESTAMP;

        --Cancelled sessions
        SELECT session.id, date_time, status, student.id, student.name, student.surname, student.profile_picture, subject.id, title
        FROM session 
        JOIN student ON session.student_id = student.id 
        JOIN professor ON session.professor_id = professor.id 
        JOIN subject ON subject.id = session.subject_id
        WHERE session.professor_id = @ProfessorId AND session.status = 'Cancelled';";

            using (var multi = await _connection.QueryMultipleAsync(query, new { ProfessorId = professorId }))
            {
                var pastSessions = multi.Read<Session, Student, Subject, SessionWithStudentDTO>((session, student, subject) =>
                {
                    return new SessionWithStudentDTO
                    {
                        SessionId = session.Id,
                        DateTime = session.DateTime,
                        Status = session.Status,

                        Student = new Student
                        {
                            Id = student.Id,
                            Name = student.Name,
                            Surname = student.Surname,
                            ProfilePicture = student.ProfilePicture
                        },

                        Subject = new Subject
                        {
                            Id = subject.Id,
                            Title = subject.Title
                        }
                    };
                }, splitOn: "id,id").ToList();

                var pendingSessions = multi.Read<Session, Student, Subject, SessionWithStudentDTO>((session, student, subject) =>
                {
                    return new SessionWithStudentDTO
                    {
                        SessionId = session.Id,
                        DateTime = session.DateTime,
                        Status = session.Status,

                        Student = new Student
                        {
                            Id = student.Id,
                            Name = student.Name,
                            Surname = student.Surname,
                            ProfilePicture = student.ProfilePicture
                        },

                        Subject = new Subject
                        {
                            Id = subject.Id,
                            Title = subject.Title
                        }
                    };
                }, splitOn: "id,id").ToList();

                var upcomingSessions = multi.Read<Session, Student, Subject, SessionWithStudentDTO>((session, student, subject) =>
                {
                    return new SessionWithStudentDTO
                    {
                        SessionId = session.Id,
                        DateTime = session.DateTime,
                        Status = session.Status,

                        Student = new Student
                        {
                            Id = student.Id,
                            Name = student.Name,
                            Surname = student.Surname,
                            ProfilePicture = student.ProfilePicture
                        },

                        Subject = new Subject
                        {
                            Id = subject.Id,
                            Title = subject.Title
                        }
                    };
                }, splitOn: "id,id").ToList();

                var cancelledSessions = multi.Read<Session, Student, Subject, SessionWithStudentDTO>((session, student, subject) =>
                {
                    return new SessionWithStudentDTO
                    {
                        SessionId = session.Id,
                        DateTime = session.DateTime,
                        Status = session.Status,

                        Student = new Student
                        {
                            Id = student.Id,
                            Name = student.Name,
                            Surname = student.Surname,
                            ProfilePicture = student.ProfilePicture
                        },

                        Subject = new Subject
                        {
                            Id = subject.Id,
                            Title = subject.Title
                        }
                    };
                }, splitOn: "id,id").ToList();

                return new SessionsDTO<SessionWithStudentDTO>
                {
                    PastSessions = pastSessions,
                    PendingSessions = pendingSessions,
                    UpcomingSessions = upcomingSessions,
                    CancelledSessions = cancelledSessions
                };
            }
        }

        public async Task<Session> GetSessionByIdAsync(int sessionId)
        {
            const string query = @"SELECT * FROM session WHERE id = @SessionId";

            return await _connection.QueryFirstOrDefaultAsync<Session>(query, new { SessionId = sessionId });
        }

        public async Task ManageSessionRequestAsync(int sessionId, string newStatus)
        {
            const string query = @"UPDATE session SET status = @NewStatus WHERE id = @SessionId";

            await _connection.ExecuteAsync(query, new { SessionId = sessionId, NewStatus = newStatus });
        }
    }
}
