using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Rabbithole.Domain.Extensions
{
    // Deezer, Navidrome and Lidarr spell names differently ("The Beatles" / "Beatles", "Beyoncé" / "Beyonce").
    // Everything that compares names across services goes through these.
    public static partial class StringNormalisationExtensions
    {
        public static string NormaliseArtistName(this string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return string.Empty;
            }

            var normalised = RemoveDiacritics(name).ToLowerInvariant().Replace("&", " and ");

            if (normalised.StartsWith("the "))
            {
                normalised = normalised[4..];
            }

            return NonAlphanumeric().Replace(normalised, string.Empty);
        }

        public static string NormaliseTitle(this string? title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return string.Empty;
            }

            var normalised = RemoveDiacritics(title).ToLowerInvariant();

            // Drop "(Remastered 2011)", "[Deluxe Edition]", " - Remastered" etc.
            normalised = Bracketed().Replace(normalised, string.Empty);
            normalised = DashSuffix().Replace(normalised, string.Empty);

            return NonAlphanumeric().Replace(normalised.Replace("&", " and "), string.Empty);
        }

        private static string RemoveDiacritics(string value)
        {
            var decomposed = value.Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder(decomposed.Length);

            foreach (var character in decomposed)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
                {
                    builder.Append(character);
                }
            }

            return builder.ToString().Normalize(NormalizationForm.FormC);
        }

        [GeneratedRegex("[^a-z0-9]")]
        private static partial Regex NonAlphanumeric();

        [GeneratedRegex(@"[\(\[][^\)\]]*[\)\]]")]
        private static partial Regex Bracketed();

        [GeneratedRegex(@"\s-\s.*(remaster|edition|version|mix|live|mono|stereo).*$")]
        private static partial Regex DashSuffix();
    }
}
