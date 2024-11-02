namespace dotInstrukcijeBackend.Interfaces.User
{
    public interface IUser
    {
        int Id { get; }
        string Email { get; }
        string Role { get; }
    }

}
