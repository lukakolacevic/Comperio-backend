using dotInstrukcijeBackend.DataTransferObjects;
using dotInstrukcijeBackend.Models;

namespace dotInstrukcijeBackend.HelperFunctions
{
    public static class MapToProfessorDTO
    {
        public static ProfessorDTO mapToProfessorDTO(Professor professor)
        {
            String profilePhotoBase64String = professor.ProfilePicture is not null ? Convert.ToBase64String(professor.ProfilePicture) : null;

            return new ProfessorDTO
            {
                ProfessorId = professor.Id,
                Name = professor.Name,
                Surname = professor.Surname,
                ProfilePictureBase64String = profilePhotoBase64String,
                InstructionsCount = professor.InstructionsCount
            };
        }
    }
}
