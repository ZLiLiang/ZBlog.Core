using System.IO.Compression;
using Com.Ctrip.Framework.Apollo.Enums;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace ZBlog.Core.Extensions.ServiceExtensions
{
    /// <summary>
    /// 将前端UI压缩文件进行解压
    /// </summary>
    public static class UiFilesZipSetup
    {
        public static void AddUiFilesZipSetup(this IServiceCollection services, IWebHostEnvironment environment)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            string wwwrootFolderPath = Path.Combine(environment.ContentRootPath, "wwwroot");
            string zipUiItemFiles = Path.Combine(wwwrootFolderPath, "ui.zip");
            if (!File.Exists(Path.Combine(wwwrootFolderPath, "ui", "index.html")))
            {
                ZipFile.ExtractToDirectory(zipUiItemFiles, wwwrootFolderPath);
            }
        }
    }
}
