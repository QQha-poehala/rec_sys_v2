namespace Rec_backend.Contracts;

public record GetUserRequest(
        string login,
        string password
    );
