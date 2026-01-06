namespace Orchid.Core.Models.ValueObjects;

public record Cover
{
    private Cover(string path, byte[] imageData)
    {
        Path = path;
        ImageData = imageData;
    }

    public byte[] ImageData { get; }
    public string Path { get; set; }

    public static Cover? Create(string path, byte[]? imageData) =>
        imageData is not null && imageData.Length > 0
            ? new Cover(path, imageData)
            : null;

    public string ToBase64() => $"data:image/png;base64,{Convert.ToBase64String(ImageData)}";


}