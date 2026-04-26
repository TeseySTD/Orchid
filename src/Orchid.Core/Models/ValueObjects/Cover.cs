namespace Orchid.Core.Models.ValueObjects;

public record Cover : Image
{
    private Cover(string name, byte[] data): base(name, data) {}

    private Cover(string name, string path, byte[] data) : base(name, data)
    {
        Path = path;
    }

    public string Path { get; set; } = string.Empty;

    public new static Cover? Create(string name, byte[]? data) =>
        data is not null && data.Length > 0
            ? new Cover(name, data)
            : null;
    public static Cover? Create(string name, string path, byte[]? data) =>
        data is not null && data.Length > 0
            ? new Cover(name, path, data)
            : null;
}