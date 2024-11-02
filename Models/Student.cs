using dotInstrukcijeBackend.Interfaces.User;
using System.ComponentModel.DataAnnotations;

namespace dotInstrukcijeBackend.Models
{
    public class Student : IUser
    {
        [Key]
        public int Id { get; set; }
        public String Email { get; set; }
        public String Name { get; set; }
        public String Surname { get; set; }
        public String Password { get; set; }
        public byte[]? ProfilePicture { get; set; }
        public string Role => "Student";
    }
}

