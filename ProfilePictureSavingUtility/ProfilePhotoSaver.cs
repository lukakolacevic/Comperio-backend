using dotInstrukcijeBackend.Interfaces.Utility;

namespace dotInstrukcijeBackend.ProfilePictureSavingUtility
{
    public class ProfilePhotoSaver : IProfilePhotoSaver
    {
        public async Task<byte[]> SaveProfilePictureAsync(IFormFile profilePicture)
        {
            if (profilePicture == null || profilePicture.Length == 0)
            {
                return null; // Return null if no picture is provided
            }

            using (var memoryStream = new MemoryStream())
            {
                await profilePicture.CopyToAsync(memoryStream);
                return memoryStream.ToArray(); // Return the byte array that will be saved as a BLOB in the database
            }
        }
    }
}
