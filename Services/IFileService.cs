using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using System.Threading.Tasks;
using Services.logs;

namespace Services
{
    public interface IFileService
    {
        Task<bool> UploadFile(string uploadPath, IFormFile file, string fileName, string userId);
        bool DeleteFile(string filePath , string userId);
    }

    public class FileService : IFileService
    {
        private readonly IErrorLogService _errorLogService;
        public FileService(IErrorLogService errorLogService)
        {
            _errorLogService = errorLogService;
        }
        public Task<bool> UploadFile(string uploadPath, IFormFile file, string fileName, string userId)
        {
            try
            {
                // fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"').Replace(" ", "_");
                fileName = fileName.Trim('"').Replace(" ", "_");
                string fullUploadPath = uploadPath + fileName;
                return Task.Run(() =>
                {
                    if (!Directory.Exists(uploadPath))
                    {

                        Directory.CreateDirectory(uploadPath);
                    }

                    using (FileStream fs = File.Create(fullUploadPath))
                    {
                        file.CopyTo(fs);
                        fs.Flush();
                    }
                    return File.Exists(fullUploadPath);
                });
            }
            catch (Exception x)
            {
                _errorLogService.LogError($"FileService - UploadFile: {x.Message} {x.InnerException}", userId);
                return Task.FromResult(false);
            }         
        }

        public bool DeleteFile(string filePath, string userId)
        {
            try
            {
                GC.Collect();               //release files from memory
                GC.WaitForPendingFinalizers();
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                return (!File.Exists(filePath));
            }
            catch (Exception x)
            {
                _errorLogService.LogError($"FileService - UploadFile: {x.Message} {x.InnerException}", userId);
                return false;
            }
        }
    }
}
