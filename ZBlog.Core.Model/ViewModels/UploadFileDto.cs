using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ZBlog.Core.Model.ViewModels
{
    public class UploadFileDto
    {
        //多文件
        [Required]
        public IFormFileCollection File { get; set; }

        //单文件
        //public IFormFile File { get; set; }

        //其他数据
        public string Foo { get; set; }

    }
}
