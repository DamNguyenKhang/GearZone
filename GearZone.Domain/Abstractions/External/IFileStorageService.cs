using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GearZone.Domain.Abstractions.External
{
    public record FileUploadRequest(string FileName, Stream Stream, long Length);

    public interface IFileStorageService
    {
        Task<List<string>> UploadAsync(List<FileUploadRequest> files);
        Task DeleteAsync(string fileUrl);
    }
}
