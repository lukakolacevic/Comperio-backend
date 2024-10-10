using dotInstrukcijeBackend.Models;

namespace dotInstrukcijeBackend.DataTransferObjects
{
    public class SessionWithProfessorDTO
    {
        public int SessionId { get; set; }
        public DateTime DateTime { get; set; }
        public string Status { get; set; }
        public ProfessorDTO Professor { get; set; }
        public SubjectDTO Subject { get; set; }
        
    }
}
