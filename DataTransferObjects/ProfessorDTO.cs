namespace dotInstrukcijeBackend.DataTransferObjects
{
    public class ProfessorDTO
    {
        public int ProfessorId {  get; set; }
        public String Name { get; set; }
        public String Surname { get; set; }
        public String ProfilePictureBase64String { get; set; }
        public int InstructionsCount { get; set; }
        
    }
}
