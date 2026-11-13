namespace CiOHjemmeside.Data.Models
{
    /// <summary>
    /// Billeddata for en produktgruppe, hentes kun naar billedet serveres.
    /// </summary>
    public class ProductImage
    {
        public byte[] Content { get; set; } = Array.Empty<byte>();
        public string ContentType { get; set; } = string.Empty;
    }
}
