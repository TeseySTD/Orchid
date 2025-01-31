namespace Orchid.Core.Models.ValueObjects;

public record Cover
{
    private Cover(byte[] imageData) => ImageData = imageData;

    public byte[] ImageData { get; }

    public static Cover Create(byte[] imageData) =>
        imageData?.Length > 0 
            ? new Cover(imageData) 
            : throw new ArgumentException("Invalid cover image");

    public string ToBase64() => $"data:image/png;base64,{Convert.ToBase64String(ImageData)}";
}