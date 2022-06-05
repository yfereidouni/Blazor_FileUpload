using Microsoft.AspNetCore.StaticFiles;
using Microsoft.JSInterop;

namespace Blazor_FileUpload.Services;

public interface IFileDownload
{
    Task<List<string>> GetUploadedFiles();
    Task DownloadFile(string url);
}
public class FileDownload : IFileDownload
{
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IJSRuntime _js;

    public FileDownload(IWebHostEnvironment webHostEnvironment, IJSRuntime js)
    {
        _webHostEnvironment = webHostEnvironment;
        _js = js;
    }

    public async Task<List<string>> GetUploadedFiles()
    {
        var base64Urls = new List<string>();
        var uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
        var files = Directory.GetFiles(uploadPath);

        if (files is not null && files.Length > 0)
        {
            foreach (var file in files)
            {
                using (var fileInput = new FileStream(file, FileMode.Open, FileAccess.Read))
                {
                    var memoryStream = new MemoryStream();
                    await fileInput.CopyToAsync(memoryStream);

                    var buffer = memoryStream.ToArray();
                    var fileType = GetMimeTypeForFileExtension(file);
                    base64Urls.Add($"data:{fileType};base64,{Convert.ToBase64String(buffer)}");
                }
            }
        }
        return base64Urls;
    }

    public async Task DownloadFile(string url)
    {
        await _js.InvokeVoidAsync("downloadFile", url);
    }

    private string GetMimeTypeForFileExtension(string filePath)
    {
        #region Octet-stream
        //https://isotropic.co/what-is-octet-stream/
        // Video file extensions - mp4, .mpeg
        // Image extensions - .jpg, .png, .tiff, .gif
        // Flash extension -.swf
        // Adobe portable document format- .pdf
        // Microsoft Word extension - .doc, docx
        // Microsof Excel - .xml
        // Compressed files often come in a ZIP file -  .zip
        #endregion
        const string DefaultContextType = "application/octet-stream";

        var provider = new FileExtensionContentTypeProvider();

        if (!provider.TryGetContentType(filePath, out string contentType))
        {
            contentType = DefaultContextType;
        }

        return contentType;
    }
}
