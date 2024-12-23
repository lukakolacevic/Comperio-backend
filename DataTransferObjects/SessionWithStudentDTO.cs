﻿using dotInstrukcijeBackend.Models;

namespace dotInstrukcijeBackend.DataTransferObjects
{
    public class SessionWithStudentDTO
    {
        public int SessionId { get; set; }
        public DateTime DateTime { get; set; }
        public string Status { get; set; }
        public User Student { get; set; }
        public Subject Subject { get; set; }
    }
}
