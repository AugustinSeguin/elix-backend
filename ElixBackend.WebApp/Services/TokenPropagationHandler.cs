using System.Net.Http.Headers;

namespace ElixBackend.WebApp.Services;

public class TokenPropagationHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var ctx = httpContextAccessor.HttpContext;
        if (ctx != null && !request.Headers.Contains("Authorization"))
        {
            if (ctx.Request.Cookies.TryGetValue("JwtToken", out var token) && !string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        return base.SendAsync(request, cancellationToken);
    }
}