using dotInstrukcijeBackend.Interfaces.User;
using dotInstrukcijeBackend.ProfilePictureSavingUtility;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace dotInstrukcijeBackend.Models
{
    public class Professor : IUser
    {
        [Key]
        public int Id { get; set; } 
        public String Email { get; set; }
        public String Name { get; set; }
        public String Surname { get; set; }
        
        [JsonIgnore]
        public String Password { get; set; }

        [JsonConverter(typeof(ByteArrayToBase64Converter))]
        public byte[]? ProfilePicture { get; set; }
        public int InstructionsCount { get; set; }
        public int OAuthId { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsVerified { get; set; }
        public string Role => "Professor";
    }
}