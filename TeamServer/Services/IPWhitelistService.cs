using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace HardHatCore.TeamServer.Services
{
    public class IPWhitelistService
    {
        private readonly RequestDelegate _next;
        private readonly IPWhitelistOptions _iPWhitelistOptions;
        public IPWhitelistService(RequestDelegate next,IOptions<IPWhitelistOptions> applicationOptionsAccessor)
        {
            _iPWhitelistOptions = applicationOptionsAccessor.Value;
            _next = next;
        }
        public async Task Invoke(HttpContext context)
        {
            var ipAddress = context.Connection.RemoteIpAddress;
            List<string> whiteListIPList = _iPWhitelistOptions.Whitelist;
            if (!whiteListIPList.Contains("*"))
            {
                var isIPWhitelisted = whiteListIPList.Where(ip => IPAddress.Parse(ip).Equals(ipAddress)).Any();
                if (!isIPWhitelisted)
                {
                    LoggingService.EventLogger.Warning("Request from Remote IP address: {RemoteIp} is forbidden.", ipAddress);
                    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    return;
                }
            }
            await _next.Invoke(context);
        }
    }

    public class IPWhitelistOptions
    {
        public List<string> Whitelist { get; set; }
    }

    public static class IPWhitelistMiddlewareExtensions
    {
        public static IApplicationBuilder UseIPWhitelist(this
        IApplicationBuilder builder)
        {
            return builder.UseMiddleware<IPWhitelistService>();
        }
    }
}
