using System.ComponentModel.DataAnnotations;

namespace dotInstrukcijeBackend.Models
{
    public class Session
    {
        [Key]
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int InstructorId { get; set; }
        public int SubjectId { get; set; }        
        public DateTime DateTime { get; set; }
        public String Status { get; set; }
    }
}
