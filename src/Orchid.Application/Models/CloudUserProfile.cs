namespace Orchid.Application.Models;

public record CloudUserProfile(
    string Name,
    string? Email,
    string? AvatarUrl
);