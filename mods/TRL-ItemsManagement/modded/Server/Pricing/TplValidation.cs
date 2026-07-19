namespace TRLItemsManagement.Pricing;

/// <summary>
///     Shared MongoId (24-char hex) validation. Extracted from the identical private copies in
///     <see cref="StockApplier"/> and <see cref="FleaFloorOverrideStore"/> (code-review CR-03). The
///     char-by-char form is used by the boot appliers (no Regex allocation per entry); the API
///     controllers keep their own compiled <c>TplPattern</c> Regex on the request path.
/// </summary>
internal static class TplValidation
{
    internal static bool IsHex24(string? s)
    {
        if (s is null || s.Length != 24)
        {
            return false;
        }

        foreach (var c in s)
        {
            if (!Uri.IsHexDigit(c))
            {
                return false;
            }
        }

        return true;
    }
}
