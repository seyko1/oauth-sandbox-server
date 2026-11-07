namespace AuthServer.Db;

public sealed record TestUser(string Id, string Email, string Name, string Password);

public static class TestUsers
{
    private static readonly TestUser[] Users =
    [
        new("1234", "seyko@pwet.fr", "seyko", "pwet")
    ];

    public static TestUser? Get(string email, string password)
    {
        return Users.FirstOrDefault(user =>
            string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase)
            && user.Password == password
        );
    }
}