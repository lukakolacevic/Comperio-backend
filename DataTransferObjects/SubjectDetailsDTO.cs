using dotInstrukcijeBackend.Models;

namespace dotInstrukcijeBackend.DataTransferObjects
{
    public class SubjectDetailsDTO
    {
        public Subject Subject { get; set; }

        public IEnumerable<Professor> SubjectProfessors { get; set; }
    }
}
