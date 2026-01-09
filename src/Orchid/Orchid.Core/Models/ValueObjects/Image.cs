namespace Orchid.Core.Models.ValueObjects;

public record Image
{
    public string Name { get; init; }
    public byte[] Data { get; init; }

    protected Image(string name, byte[] data)
    {
        Name = name;
        Data = data;
    }

    public static Image Create(string imageName, byte[] data) => new(imageName, data);

    public string ToBase64() => $"data:image/png;base64,{Convert.ToBase64String(Data)}";
}