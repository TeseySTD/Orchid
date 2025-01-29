namespace Orchid.Core.Models.Book.ValueObjects;

public record Cover
{
    private Cover(byte[] imageData) => ImageData = imageData;

    public byte[] ImageData { get; }

    public static Cover Create(byte[] imageData) =>
        imageData?.Length > 0 
            ? new Cover(imageData) 
            : throw new ArgumentException("Invalid cover image");
}