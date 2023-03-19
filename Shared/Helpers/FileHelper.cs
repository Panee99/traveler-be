﻿using Microsoft.AspNetCore.Http;
using Shared.ResultExtensions;

namespace Shared.Helpers;

public static class FileHelper
{
    private static HashSet<string> ValidContentTypes = new HashSet<string>()
    {
        "image/jpeg",
        "image/png",
        "image/gif",
        "image/gif",
        "image/webp",
        "image/webp",
        "image/bmp",
        "image/vnd.wap.wbmp",
    };

    private static readonly string _validContentTypesAsString = string.Join(", ", ValidContentTypes);

    private static bool _isValidImageFormat(string contentType)
    {
        return ValidContentTypes.Contains(contentType);
    }

    public static Result ValidateImageFile(IFormFile file)
    {
        if (!_isValidImageFormat(file.ContentType))
            return Error.Validation(new Dictionary<string, string>()
            {
                { "contentType", "Invalid ContentType. Supported formats: " + _validContentTypesAsString }
            });

        if (file.Length > AppConstants.FileSizeMax)
            return Error.Validation(new Dictionary<string, string>()
            {
                { "fileSize", "File too big, max = 5mb." }
            });

        return Result.Success();
    }
}