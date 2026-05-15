namespace Orchid.Application.Dto;

public record CloudUserProfile(
    string Name,
    string? Email,
    string? AvatarUrl
);