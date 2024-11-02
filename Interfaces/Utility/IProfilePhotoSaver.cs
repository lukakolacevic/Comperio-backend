namespace dotInstrukcijeBackend.Interfaces.Utility
{
    public interface IProfilePhotoSaver
    {
        Task<byte[]> SaveProfilePictureAsync(IFormFile profilePicture);
    }
}
