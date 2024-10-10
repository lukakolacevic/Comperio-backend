namespace dotInstrukcijeBackend.DataTransferObjects
{
    public class SessionsDTO<T>
    {
        public IEnumerable<T> PastSessions { get; set; }
        public IEnumerable<T> PendingSessions { get; set; }
        public IEnumerable<T> UpcomingSessions { get; set; }
        public IEnumerable<T> CancelledSessions { get; set; }
    }
}
