﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OverTheBoard.Infrastructure.Services
{
    public class FileUploader : IFileUploader
    {
        private IHostingEnvironment _hostingEnvironment;
        public FileUploader(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }
        public async Task<string> UploadImage(IFormFile file, string userId)
        {
            if (file != null)
            {
                long totalBytes = file.Length;
                string fileExtention = Path.GetExtension(file.FileName.Trim('"'));
                var filename = $"{userId}{fileExtention}";
                var path = EnsureFileName(filename);
                var rootPath = _hostingEnvironment.WebRootPath + "\\uploads\\DisplayImages\\Users\\";
                string filesToDelete = @$"*{userId}*";   // Only delete DOC files containing "DeleteMe" in their filenames
                string[] displayImagesOfUser = Directory.GetFiles(rootPath, filesToDelete);
                foreach (string displayImage in displayImagesOfUser)
                {
                    File.Delete(displayImage);
                }
                byte[] buffer = new byte[16 * 1024];
                using (FileStream output = File.Create(path))
                {
                    using (Stream input = file.OpenReadStream())
                    {
                        int readBytes;
                        while ((readBytes = input.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            await output.WriteAsync(buffer, 0, readBytes);
                            totalBytes += readBytes;
                        }
                    }
                }
                return filename;
            }
            else
            {
                return null;
            }
        }

        private string GetPathAndFileName(string path)
        {
            if (path.Contains("\\"))
                path = path.Substring(path.LastIndexOf("\\") + 1);

            return path;
        }

        private string EnsureFileName(string filename)
        {
            //string path = _hostingEnvironment.ContentRootPath + "\\Uploads\\DisplayImages\\";
            string path = _hostingEnvironment.WebRootPath + "\\uploads\\DisplayImages\\Users\\";

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return path + filename;
        }
    }
}
