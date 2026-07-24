#nullable enable
namespace Template.Core.Services.Files;

public sealed record ValidatedFile(string OriginalName, string StorageName, string MimeType,
    long Size, string Sha256, byte[] Content);

public interface IDatabaseFileService
{
    Task<ValidatedFile> ValidateAsync(string fileName, string? contentType, Stream content,
        long length, CancellationToken cancellationToken);
}
