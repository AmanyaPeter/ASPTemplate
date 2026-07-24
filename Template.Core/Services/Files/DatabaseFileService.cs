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
            throw new InvalidDataException("Only PDF and DOCX files are allowed.");

        await using var buffer = new MemoryStream((int)length);
        await content.CopyToAsync(buffer, cancellationToken);
        var bytes = buffer.ToArray();
        var valid = extension switch
        {
            ".pdf" => bytes.Length >= 5 && bytes.AsSpan(0, 5).SequenceEqual("%PDF-"u8),
            ".docx" => bytes.Length >= 4 && bytes[0] == 0x50 && bytes[1] == 0x4B,
            _ => false
        };
        if (!valid) throw new InvalidDataException("The file content does not match its extension.");
        var mime = extension == ".pdf"
            ? "application/pdf"
            : "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
        return new(Path.GetFileName(fileName), $"{Guid.NewGuid():N}{extension}", mime, bytes.LongLength,
            Convert.ToHexString(SHA256.HashData(bytes)), bytes);
    }
}
