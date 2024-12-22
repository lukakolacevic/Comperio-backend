namespace dotInstrukcijeBackend.Interfaces
{
    public interface IUser
    {
        int Id { get; }
        string Email { get; }
        string Role { get; }
    }

}
