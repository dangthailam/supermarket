using System.Globalization;
using System.Text;

namespace SuperMarket.Application.Common;

public static class StringExtensions
{
    /// <summary>
    /// Removes Vietnamese accents from a string for search purposes.
    /// Example: "bánh mì" -> "banh mi"
    /// </summary>
    public static string RemoveVietnameseAccents(this string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        // Normalize to decomposed form (separate base characters from accents)
        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            // Skip combining marks (accents)
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        // Handle special Vietnamese characters that don't decompose properly
        return stringBuilder.ToString()
            .Normalize(NormalizationForm.FormC)
            .Replace("đ", "d")
            .Replace("Đ", "D");
    }

    /// <summary>
    /// Normalizes a string for search by removing accents and converting to lowercase.
    /// </summary>
    public static string NormalizeForSearch(this string text)
    {
        return text.RemoveVietnameseAccents().ToLower();
    }
}
