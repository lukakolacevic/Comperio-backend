using dotInstrukcijeBackend.Models;

namespace dotInstrukcijeBackend.DataTransferObjects
{
    public class SessionWithUserDTO
    {
        public int SessionId { get; set; }
        public DateTime DateTime { get; set; }
        public DateTime DateTimeEnd { get; set; }
        public string Status { get; set; }
        public User User { get; set; }
        public Subject Subject { get; set; }

    }
}
