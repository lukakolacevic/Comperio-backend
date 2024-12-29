using dotInstrukcijeBackend.ProfilePictureSavingUtility;
using System.Text.Json.Serialization;

namespace dotInstrukcijeBackend.Models
{
    public class User
    {
        public int Id { get; set; }
        public int RoleId { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        
        [JsonIgnore]
        public string PasswordHash { get; set; }

        
        public string? ProfilePicture { get; set; }
        [JsonIgnore]
        public string? OAuthId { get; set; }
        [JsonIgnore]
        public DateTime CreatedAt { get; set; }
        [JsonIgnore]
        public bool IsVerified { get; set; }
    }
}
