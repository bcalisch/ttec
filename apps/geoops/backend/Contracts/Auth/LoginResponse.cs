namespace GeoOps.Api.Contracts.Auth;

public record LoginResponse(
    string Token,
    DateTimeOffset ExpiresAt,
    UserInfo User
);

public record UserInfo(
    Guid Id,
    string Email,
    string DisplayName
);
