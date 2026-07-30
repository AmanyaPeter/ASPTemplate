using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Template.Core.Configuration;
using Template.Core.Services.Files;
using Xunit;

namespace Template.Tests;

public sealed class DatabaseFileServiceTests
{
    private static readonly AttachmentOptions Settings = new()
    {
        MaximumSizeMb = 10,
        AllowedExtensions = [".pdf", ".doc", ".docx", ".xls", ".xlsx", ".png", ".jpg", ".jpeg"]
    };

    [Fact]
    public async Task Accepts_an_allowed_png_attachment()
    {
        var service = new DatabaseFileService(Options.Create(Settings));
        byte[] content = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x01];
        await using var stream = new MemoryStream(content);

        var file = await service.ValidateAsync(
            "supporting-evidence.png",
            "image/png",
            stream,
            content.Length,
            CancellationToken.None);

        Assert.Equal("supporting-evidence.png", file.OriginalName);
        Assert.Equal("image/png", file.MimeType);
        Assert.Equal(content, file.Content);
    }

    [Fact]
    public async Task Rejects_content_that_does_not_match_the_extension()
    {
        var service = new DatabaseFileService(Options.Create(Settings));
        byte[] content = [0x01, 0x02, 0x03, 0x04];
        await using var stream = new MemoryStream(content);

        await Assert.ThrowsAsync<InvalidDataException>(() =>
            service.ValidateAsync(
                "not-really-an-image.png",
                "image/png",
                stream,
                content.Length,
                CancellationToken.None));
    }
}
