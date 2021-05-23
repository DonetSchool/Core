using DonetSchool.QoS.Config;
using DonetSchool.QoS.Middlewares;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DonetSchool.QoS.Middlewares
{
    public class QoSMiddleware
    {
        private readonly RequestDelegate _next;

        public QoSMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, IQoSProcessor qoSProcessor)
        {
            await qoSProcessor.ExecuteAsync(async (isLimit, polyContext, qoSConfig, cancellationToken) =>
            {
                var httpContext = polyContext["HttpContext"] as HttpContext;
                httpContext.RequestAborted = cancellationToken;
                await _next.Invoke(httpContext);
            }, CreateRequestIdentity(context), context.RequestAborted, pollyContextData: new Dictionary<string, object>()
            {
                ["RequestDelegate"] = _next,
                ["HttpContext"] = context
            });
        }

        private RequestInfo CreateRequestIdentity(HttpContext context)
        {
            var request = context.Request;
            return new RequestInfo()
            {
                Method = request.Method,
                MatchInput = new StringBuilder()
                            .Append(request.PathBase)
                            .Append(request.Path)
                            .ToString()
            };
        }
    }
}

namespace Microsoft.AspNetCore.Builder
{
    public static class QoSMiddlewareExtensions
    {
        public static IApplicationBuilder UseQoS(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<QoSMiddleware>();
        }
    }
}