﻿using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OverTheBoard.Infrastructure.Services
{
    public interface IFileUploader
    {
        Task<string> UploadImage(IFormFile file, string userId);
    }
}
