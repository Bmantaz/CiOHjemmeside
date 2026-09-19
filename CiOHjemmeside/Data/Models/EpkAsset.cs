namespace CiOHjemmeside.Data.Models
{
    /// <summary>
    /// Metadata for a single EPK file (rider, press document, image).
    /// The file bytes themselves live in the epkassets table but are deliberately
    /// NOT part of this model, so listing a category never pulls megabytes of
    /// binary data into memory. Bytes are fetched on demand via EpkAssetFile.
    /// </summary>
    public class EpkAsset
    {
        public int Id { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public bool HasThumbnail { get; set; }
        public int SortOrder { get; set; }
        public DateTime UploadedAt { get; set; }

        public bool IsImage => ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// The actual binary payload for an asset, returned only when a file is served.
    /// </summary>
    public class EpkAssetFile
    {
        public byte[] Content { get; set; } = Array.Empty<byte>();
        public string ContentType { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
    }

    /// <summary>
    /// The fixed set of EPK categories. Admins manage items inside these,
    /// but cannot add or remove the categories themselves.
    /// </summary>
    public static class EpkCategories
    {
        public const string Riders = "riders";
        public const string Press = "press";
        public const string ImagesBand = "images-band";
        public const string ImagesPosters = "images-posters";
    }
}
