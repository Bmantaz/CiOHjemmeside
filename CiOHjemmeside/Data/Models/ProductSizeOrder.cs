using System.Text.RegularExpressions;

namespace CiOHjemmeside.Data.Models
{
    /// <summary>
    /// Sorterer toejstoerrelser i naturlig raekkefoelge (S, M, L, XL, XXL ...)
    /// i stedet for alfabetisk (L, M, S, XL ...). Ukendte navne kommer til sidst.
    /// </summary>
    public static class ProductSizeOrder
    {
        private static readonly string[] KnownSizes =
            { "XXS", "XS", "S", "M", "L", "XL", "XXL", "XXXL", "XXXXL", "XXXXXL" };

        public static IEnumerable<ProductVariant> Sort(IEnumerable<ProductVariant> variants) =>
            variants
                .OrderBy(v => Rank(v.VariantName))
                .ThenBy(v => v.VariantName, StringComparer.OrdinalIgnoreCase);

        public static int Rank(string? name)
        {
            var normalized = Normalize(name);
            var index = Array.IndexOf(KnownSizes, normalized);
            return index >= 0 ? index : int.MaxValue;
        }

        private static string Normalize(string? name)
        {
            var value = (name ?? string.Empty).Trim().ToUpperInvariant();

            switch (value)
            {
                case "SMALL": return "S";
                case "MEDIUM": return "M";
                case "LARGE": return "L";
            }

            // "2XL" -> "XXL", "3XL" -> "XXXL" osv.
            var match = Regex.Match(value, @"^(\d)XL$");
            if (match.Success)
            {
                return new string('X', int.Parse(match.Groups[1].Value)) + "L";
            }

            return value;
        }
    }
}
