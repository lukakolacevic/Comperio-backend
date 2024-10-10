namespace dotInstrukcijeBackend.ViewModels
{
    public class ProfessorRegistrationModel
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public IFormFile? ProfilePicture { get; set; }
        public List<string>? Subjects { get; set; }
    }
}
