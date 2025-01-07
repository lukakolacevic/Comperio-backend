﻿namespace dotInstrukcijeBackend.DataTransferObjects
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
        public int ProfessorId { get; set; }
        public string ProfessorName { get; set; }
        public string ProfessorSurname { get; set; }
        public string ProfessorEmail { get; set; } // Optional

        // Student Details
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public string StudentSurname { get; set;}
        public string StudentEmail { get; set; } // Optional
    }

}