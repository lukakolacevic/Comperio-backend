using Dapper;
using dotInstrukcijeBackend.Interfaces.Repository;
using dotInstrukcijeBackend.Models;
using System.Data;

namespace dotInstrukcijeBackend.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IDbConnection _connection;
        public UserRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task<int> AddUserAsync(User user)
        {
            const string query = @"
        INSERT INTO dbo.[user] (RoleId, Email, Name, Surname, PasswordHash, ProfilePicture, OAuthId, CreatedAt, IsVerified)
        OUTPUT INSERTED.Id
        VALUES (@RoleId, @Email, @Name, @Surname, @PasswordHash, @ProfilePicture, @OAuthId, @CreatedAt, @IsVerified);";

            return await _connection.ExecuteScalarAsync<int>(query, user);
        }


        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            const string query = @"SELECT * FROM dbo.[user];";

            return await _connection.QueryAsync<User>(query);
        }

        public async Task SetEmailVerifiedAsync(int id)
        {
            const string query = @"UPDATE dbo.[user] SET IsVerified = 1 WHERE id = @Id";

            await _connection.ExecuteAsync(query, new { Id = id });
        }

        public async Task<User> GetUserByIdAsync(int id)
        {
            const string query = @"SELECT * FROM dbo.[user] WHERE id = @Id";

            return await _connection.QueryFirstOrDefaultAsync<User>(query, new { Id = id });
        }

        public async Task<User> GetUserByEmailAsync(int roleId, string email)
        {
            const string query = @"SELECT * FROM dbo.[user] WHERE RoleId = @RoleId AND Email = @Email";
            return await _connection.QueryFirstOrDefaultAsync<User>(query, new { RoleId = roleId, Email = email });
        }

        public async Task<bool> IsUserVerifiedAsync(int id)
        {
            const string query = @"SELECT IsVerified FROM dbo.[user] WHERE id = @Id";
            return await _connection.ExecuteScalarAsync<bool>(query, new { Id = id });
        }

        public async Task<IEnumerable<User>> GetAllUsersByRoleAsync(int roleId)
        {
            const string query = @"SELECT * FROM dbo.[user] WHERE RoleId = @RoleId";
            return await _connection.QueryAsync<User>(query, new { RoleId = roleId });
        }


    }
}
