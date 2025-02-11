export function createBlobUrl(imageData, mimeType) {
    let blob = new Blob([imageData], { type: mimeType });
    return URL.createObjectURL(blob);
}
