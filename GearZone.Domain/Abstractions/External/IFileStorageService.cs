using Microsoft.AspNetCore.Http;

namespace GearZone.Domain.Abstractions.External
{
    public interface IFileStorageService
    {
        Task<List<string>> UploadAsync(List<IFormFile> files);
        Task DeleteAsync(string fileUrl);
    }
}
