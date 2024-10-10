namespace dotInstrukcijeBackend.ProfilePictureSavingUtility
{
    public static class ProfilePhotoSaver
    {
        public static async Task<byte[]> SaveProfilePicture(IFormFile profilePicture)
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
