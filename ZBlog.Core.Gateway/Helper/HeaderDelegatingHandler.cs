
using System.Net.Http.Headers;

namespace ZBlog.Core.Gateway.Helper
{
    public class HeaderDelegatingHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HeaderDelegatingHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            IEnumerable<string> headerValues;
            if (request.Headers.TryGetValues("AccessToken", out headerValues))
            {
                string accessToken = headerValues.First();

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                request.Headers.Remove("AccessToken");
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
