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
            const string query = @"INSERT INTO session (StudentId, InstructorId, SubjectId, DateTime, DateTimeEnd, Status) 
                                  VALUES(@StudentId, @InstructorId, @SubjectId, @DateTime, @DateTimeEnd, @Status)";

            await _connection.ExecuteAsync(query, new
            {
                StudentId = session.StudentId,
                InstructorId = session.InstructorId,
                SubjectId = session.SubjectId,
                DateTime = session.DateTime,
                DateTimeEnd = session.DateTimeEnd,
                Status = session.Status
            });
        }

        public async Task<SessionsDTO<SessionWithUserDTO>> GetAllStudentSessionsAsync(int studentId)
        {
            using (var multi = await _connection.QueryMultipleAsync("GetAllSessionsForStudent", new { StudentId = studentId }, commandType: CommandType.StoredProcedure))
            {
                var pendingSessions = MapSessionResults(multi);
                var upcomingSessions = MapSessionResults(multi);
                var pastSessions = MapSessionResults(multi);
                var cancelledSessions = MapSessionResults(multi);

                return new SessionsDTO<SessionWithUserDTO>
                {
                    PendingSessions = pendingSessions,
                    UpcomingSessions = upcomingSessions,
                    PastSessions = pastSessions,
                    CancelledSessions = cancelledSessions
                };
            }
        }

        public async Task<SessionsDTO<SessionWithUserDTO>> GetAllInstructorSessionsAsync(int instructorId)
        {
            using (var multi = await _connection.QueryMultipleAsync("GetAllSessionsForInstructor", new { InstructorId = instructorId }, commandType: CommandType.StoredProcedure))
            {
                var pendingSessions = MapSessionResults(multi);
                var upcomingSessions = MapSessionResults(multi);
                var pastSessions = MapSessionResults(multi);
                var cancelledSessions = MapSessionResults(multi);

                return new SessionsDTO<SessionWithUserDTO>
                {
                    PendingSessions = pendingSessions,
                    UpcomingSessions = upcomingSessions,
                    PastSessions = pastSessions,
                    CancelledSessions = cancelledSessions
                };
            }
        }

        public async Task<Session> GetSessionByIdAsync(int sessionId)
        {
            const string query = @"SELECT * FROM session WHERE Id = @SessionId";

            return await _connection.QueryFirstOrDefaultAsync<Session>(query, new { SessionId = sessionId });
        }

        public async Task ManageSessionRequestAsync(int sessionId, string newStatus)
        {
            const string query = @"UPDATE session SET Status = @NewStatus WHERE Id = @SessionId";

            await _connection.ExecuteAsync(query, new { SessionId = sessionId, NewStatus = newStatus });
        }

        public async Task<SessionDetailsDTO> GetSessionDetailsAsync(int sessionId)
        {
            var query = "EXEC GetSessionDetails @SessionId";
            var parameters = new { SessionId = sessionId };

            
            var result = await _connection.QueryFirstOrDefaultAsync<SessionDetailsDTO>(query, parameters);

            return result;
        }

        public async Task EditSessionNoteAsync(int sessionId, string newNote)
        {
            var query = @"UPDATE session SET Note = @NewNote WHERE Id = @SessionId";
            await _connection.ExecuteAsync(query, new { NewNote = newNote, SessionId = sessionId});
        }

        private List<SessionWithUserDTO> MapSessionResults(SqlMapper.GridReader multi)
        {
            return multi.Read<Session, User, Subject, SessionWithUserDTO>(
                (session, user, subject) => new SessionWithUserDTO
                {
                    SessionId = session.Id,
                    DateTime = session.DateTime,
                    DateTimeEnd = session.DateTimeEnd,
                    Status = session.Status,
                    User = new User
                    {
                        Id = user.Id,
                        RoleId = user.RoleId,
                        Email = user.Email,
                        Name = user.Name,
                        Surname = user.Surname,
                        PasswordHash = user.PasswordHash,
                        ProfilePicture = user.ProfilePicture,
                        OAuthId = user.OAuthId,
                        CreatedAt = user.CreatedAt,
                        IsVerified = user.IsVerified
                    },
                    Subject = new Subject
                    {
                        Id = subject.Id,
                        Title = subject.Title,
                        Url = subject.Url,
                        Description = subject.Description
                    }
                }, splitOn: "Id,Id").ToList();
        }
    }
}
