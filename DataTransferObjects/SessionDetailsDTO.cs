namespace dotInstrukcijeBackend.DataTransferObjects
{
    public class SessionDetailsDTO
    {
        // Session Details
        public int SessionId { get; set; }
        public DateTime DateTime { get; set; }
        public DateTime DateTimeEnd { get; set; }
        public string Status { get; set; }
        public string Note { get; set; } // Optional - if a note exists for the session

        // Subject Details
        public int SubjectId { get; set; }
        public string SubjectTitle { get; set; }

        // Professor Details
        public int InstructorId { get; set; }
        public string InstructorName { get; set; }
        public string InstructorSurname { get; set; }
        public string InstructorEmail { get; set; } // Optional

        // Student Details
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public string StudentSurname { get; set;}
        public string StudentEmail { get; set; } // Optional
    }

}
