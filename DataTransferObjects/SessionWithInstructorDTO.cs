using dotInstrukcijeBackend.Models;

namespace dotInstrukcijeBackend.DataTransferObjects
{
    public class SessionWithInstructorDTO
    {
        public int SessionId { get; set; }
        public DateTime DateTime { get; set; }
        public string Status { get; set; }
        public User Instructor { get; set; }
        public Subject Subject { get; set; }
        
    }
}
