using Microsoft.AspNetCore.Http;

namespace GearZone.Application.Abstractions.External
{
    public interface IFileStorageService
    {
        Task<List<string>> UploadAsync(List<IFormFile> files);
        Task DeleteAsync(string fileUrl);
    }
}
