using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;

namespace CiOHjemmeside.Data.Services
{
    /// <summary>
    /// Laver signerede, tidsbegraensede links til EPK-filer.
    ///
    /// /epk-file og /epk-thumb er almindelige HTTP-endpoints og kan ikke se Blazors
    /// session storage, hvor EPK-adgangen er gemt. I stedet udstedes et link med en
    /// signeret token, men KUN til brugere der allerede er inde bag EPK-gaten.
    /// Uden en gyldig token returnerer endpointet 404.
    /// </summary>
    public interface IEpkFileUrlService
    {
        string FileUrl(int assetId);
        string ThumbUrl(int assetId);
        bool IsValid(string kind, int assetId, string? token);
    }

    public class EpkFileUrlService : IEpkFileUrlService
    {
        public const string FileKind = "file";
        public const string ThumbKind = "thumb";

        // Laenge nok til at en side kan staa aaben en arbejdsdag; en genindlaesning giver nye links.
        private static readonly TimeSpan LinkLifetime = TimeSpan.FromHours(12);

        private readonly ITimeLimitedDataProtector _protector;

        public EpkFileUrlService(IDataProtectionProvider dataProtectionProvider)
        {
            _protector = dataProtectionProvider
                .CreateProtector("CiO.EpkFileLinks.v1")
                .ToTimeLimitedDataProtector();
        }

        public string FileUrl(int assetId) => $"/epk-file/{assetId}?t={CreateToken(FileKind, assetId)}";

        public string ThumbUrl(int assetId) => $"/epk-thumb/{assetId}?t={CreateToken(ThumbKind, assetId)}";

        public bool IsValid(string kind, int assetId, string? token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return false;
            }

            try
            {
                // Unprotect fejler hvis tokenen er forfalsket, aendret eller udloebet.
                return _protector.Unprotect(token) == Payload(kind, assetId);
            }
            catch (CryptographicException)
            {
                return false;
            }
        }

        private string CreateToken(string kind, int assetId) =>
            Uri.EscapeDataString(_protector.Protect(Payload(kind, assetId), LinkLifetime));

        private static string Payload(string kind, int assetId) => $"{kind}:{assetId}";
    }
}
