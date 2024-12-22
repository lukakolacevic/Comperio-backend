using System.ComponentModel.DataAnnotations;

namespace dotInstrukcijeBackend.ViewModels
{
    public class ScheduleSessionModel
    {
        [Required]
        public DateTime DateTime { get; set; }
        
        [Required]
        public int InstructorId { get; set; }

        [Required]
        public int SubjectId { get; set; }
    }
}
