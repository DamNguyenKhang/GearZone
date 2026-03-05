using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GearZone.Application.Abstractions.External
{
    public interface IFileStorageService
    {
        Task<List<string>> UploadAsync(List<IFormFile> files);
        Task DeleteAsync(string fileUrl);
    }
}
