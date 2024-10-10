namespace dotInstrukcijeBackend.DataTransferObjects
{
    public class StudentDTO
    {
        public int StudentId {  get; set; }
        public String Name { get; set; }
        public String Surname { get; set; }
        public String ProfilePictureBase64String { get; set; }
        public StudentDTO(int studentId, string name, string surname, string profilePictureBase64String)
        {
            this.StudentId = studentId;
            this.Name = name;
            this.Surname = surname;
            this.ProfilePictureBase64String = profilePictureBase64String;
        }

        public StudentDTO() { }
    }
}
