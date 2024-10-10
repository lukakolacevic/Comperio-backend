using System.ComponentModel.DataAnnotations;

namespace dotInstrukcijeBackend.Models
{
    public class Professor
    {
        [Key]
        public int Id { get; set; } 
        public String Email { get; set; }
        public String Name { get; set; }
        public String Surname { get; set; }
        public String Password { get; set; }
        public byte[]? ProfilePicture { get; set; }
        public int InstructionsCount { get; set; }
    }
}