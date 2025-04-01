namespace Rec_backend.Services;
public class AdminService
{
    private readonly Dictionary<string, string> _directions = new()
    {
        ["admin"] = "admin",
        ["root"] = "1234",
        ["test"] = "test",
    };
    public bool GetAdmin(string login, string pass)
    {
        if (string.IsNullOrEmpty(login))
            return false;
        if (string.IsNullOrEmpty(pass))
            return false;

        return _directions.TryGetValue(login, out var storedPass) && storedPass == pass;
    }
}

