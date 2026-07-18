namespace ECommerce.Application.Common;
/// <summary>
/// ÈíÊÍŞŞ ãä ãÍÊæì Çáãáİ ÇáİÚáí Úä ØÑíŞ ŞÑÇÁÉ Ãæá ÈÇíÊÇÊ ãäå (Magic Bytes /
/// File Signature)¡ ÈÛÖ ÇáäÙÑ Úä ÇáÇÓã Ãæ ÇáÜ Content-Type ÇáãõÚáä ãä ÇáÚãíá.
/// static class ÚÔÇä ãÍÊÇÌæÔ DI æãİíåæÔ state — ÈíÊäÇÏì ãÈÇÔÑÉ ãä Ãí Validator.
/// </summary>
public static class FileSignatureChecker
{
    // ÈÕãÇÊ ÇáÃäæÇÚ ÇáãÏÚæãÉ
    private static readonly byte[] JpegSignature = { 0xFF, 0xD8, 0xFF };
    private static readonly byte[] PngSignature = { 0x89, 0x50, 0x4E, 0x47 };

    // WEBP ÈÊÈÏÃ ÈÜ "RIFF" (4 ÈÇíÊ) + ÍÌã Çáãáİ (4 ÈÇíÊ) + "WEBP" (4 ÈÇíÊ)
    // áÇÒã äÊÍŞŞ ãä ÇáÇÊäíä ÚÔÇä RIFF áæÍÏåÇ ãÔ ßÇİíÉ (WAV ÈÑÖå ÈíÈÏÃ ÈíåÇ)
    private static readonly byte[] RiffSignature = { 0x52, 0x49, 0x46, 0x46 }; // "RIFF"
    private static readonly byte[] WebpSignature = { 0x57, 0x45, 0x42, 0x50 }; // "WEBP"

    // ÃØæá signature ÚäÏäÇ WEBP æÈÊÍÊÇÌ 12 ÈÇíÊ
    private const int RequiredBufferSize = 12;

    /// <summary>
    /// ÈÊŞÑÃ Ãæá ÈÇíÊÇÊ ãä ÇáÜ Stream æÈÊÊÍŞŞ Åä ãÍÊæÇå ãØÇÈŞ ááÜ Content-Type
    /// ÇáãõÚáä. ÈÊÑÌÚ ÇáÜ Stream áæÖÚå ÇáÃÕáí (Position = 0) ÈÚÏ ÇáŞÑÇÁÉ ÚÔÇä
    /// ÇáÑİÚ ÇáİÚáí ÈÚÏíä íÔÊÛá ãä Ãæá Çáãáİ.
    /// </summary>
    /// <param name="stream">stream Çáãáİ ÇáãÑÇÏ İÍÕå</param>
    /// <param name="declaredContentType">ÇáÜ Content-Type Çááí ÈÚÊå ÇáÚãíá</param>
    /// <returns>true áæ ÇáãÍÊæì ãØÇÈŞ ááäæÚ ÇáãõÚáä</returns>
    public static async Task<bool> IsValidImageSignatureAsync(Stream stream, string declaredContentType)
    {
        if (!stream.CanSeek)
            return false;

        var buffer = new byte[RequiredBufferSize];
        var savedPosition = stream.Position;

        stream.Position = 0;
        var bytesRead = await stream.ReadAsync(buffer.AsMemory(0, RequiredBufferSize));

        // ãåã: äÑÌøÚ ÇáÜ Position áãßÇäå ÇáÃÕáí ÈÚÏ ÇáİÍÕ ÚÔÇä
        // Ãí ßæÏ ÊÇäí íÌí ÈÚÏäÇ íáÇŞí ÇáÜ stream Òí ãÇ ßÇäå
        stream.Position = savedPosition;

        if (bytesRead < 3)
            return false;

        return declaredContentType switch
        {
            "image/jpeg" => StartsWith(buffer, JpegSignature),
            "image/png" => StartsWith(buffer, PngSignature),
            "image/webp" => StartsWith(buffer, RiffSignature) && ContainsAt(buffer, WebpSignature, offset: 8),
            _ => false
        };
    }

    private static bool StartsWith(byte[] buffer, byte[] signature)
    {
        if (buffer.Length < signature.Length)
            return false;

        for (var i = 0; i < signature.Length; i++)
            if (buffer[i] != signature[i])
                return false;

        return true;
    }

    private static bool ContainsAt(byte[] buffer, byte[] signature, int offset)
    {
        if (buffer.Length < offset + signature.Length)
            return false;

        for (var i = 0; i < signature.Length; i++)
            if (buffer[offset + i] != signature[i])
                return false;

        return true;
    }
}