namespace dotInstrukcijeBackend.Models
{
    public class Message
    {
        public string[] To { get; }
        public string Subject { get; }
        public string Content { get; }

        public Message(string[] to, string subject, string content)
        {
            To = to;
            Subject = subject;
            Content = content;
        }
    }
}
