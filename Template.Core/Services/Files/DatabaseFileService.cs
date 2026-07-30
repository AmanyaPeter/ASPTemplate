#nullable enable
using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Template.Core.Configuration;

namespace Template.Core.Services.Files;

public sealed class DatabaseFileService(IOptions<AttachmentOptions> options) : IDatabaseFileService
{
    public async Task<ValidatedFile> ValidateAsync(string fileName, string? contentType, Stream content,
        long length, CancellationToken cancellationToken)
    {
        var settings = options.Value;
        if (length <= 0 || length > settings.MaximumSizeMb * 1024L * 1024L)
            throw new InvalidDataException($"File size must be between 1 byte and {settings.MaximumSizeMb} MB.");
        var extension = Path.GetExtension(Path.GetFileName(fileName)).ToLowerInvariant();
        if (!settings.AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            throw new InvalidDataException(
                $"Allowed file types: {string.Join(", ", settings.AllowedExtensions.Select(item => item.TrimStart('.').ToUpperInvariant()))}.");

        await using var buffer = new MemoryStream((int)length);
        await content.CopyToAsync(buffer, cancellationToken);
        var bytes = buffer.ToArray();
        var valid = extension switch
        {
            ".pdf" => bytes.Length >= 5 && bytes.AsSpan(0, 5).SequenceEqual("%PDF-"u8),
            ".docx" or ".xlsx" => HasPrefix(bytes, [0x50, 0x4B, 0x03, 0x04]),
            ".doc" or ".xls" => HasPrefix(bytes, [0xD0, 0xCF, 0x11, 0xE0]),
            ".png" => HasPrefix(bytes, [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]),
            ".jpg" or ".jpeg" => HasPrefix(bytes, [0xFF, 0xD8, 0xFF]),
            _ => false
        };
        if (!valid) throw new InvalidDataException("The file content does not match its extension.");
        var mime = extension switch
        {
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            _ => "application/octet-stream"
        };
        return new(Path.GetFileName(fileName), $"{Guid.NewGuid():N}{extension}", mime, bytes.LongLength,
            Convert.ToHexString(SHA256.HashData(bytes)), bytes);
    }

    private static bool HasPrefix(byte[] content, byte[] signature) =>
        content.Length >= signature.Length &&
        content.AsSpan(0, signature.Length).SequenceEqual(signature);
}
