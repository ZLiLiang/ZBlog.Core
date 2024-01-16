using ZBlog.Core.Model.Models;

namespace ZBlog.Core.Model.ViewModels
{
    /// <summary>
    /// 用来测试 RestSharp Get 请求
    /// </summary>
    public class TestRestSharpGetDto
    {
        public string Success { get; set; }

        public BlogArticle Data { get; set; }
    }
}
