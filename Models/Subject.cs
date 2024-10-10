using System.ComponentModel.DataAnnotations;

namespace dotInstrukcijeBackend.Models
{
    public class Subject
    {
        [Key]
        public int Id { get; set; }
        public String Title { get; set; }
        public String Url { get; set; }
        public String Description { get; set; }
    }
}
