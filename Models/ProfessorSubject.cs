using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace dotInstrukcijeBackend.Models
{
    public class ProfessorSubject
    {
        public int ProfessorId { get; set; }

        public int SubjectId { get; set; }
    }
}
