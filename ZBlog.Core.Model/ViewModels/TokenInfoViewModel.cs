namespace ZBlog.Core.Model.ViewModels
{
    public class TokenInfoViewModel
    {
        public bool Success { get; set; }

        public string Token { get; set; }

        public double Expires_In { get; set; }

        public string Token_Type { get; set; }
    }
}
