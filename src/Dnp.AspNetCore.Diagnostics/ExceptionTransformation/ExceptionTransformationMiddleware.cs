using System;
using System.Threading.Tasks;

using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace DNP.AspNetCore.Diagnostics
{
    public class ExceptionTransformationMiddleware
    {
        private readonly Func<object, Task> _clearCacheHeadersDelegate;
        private readonly ILogger _logger;
        private readonly RequestDelegate _next;
        private readonly ExceptionTransformationOptions _options;

        public ExceptionTransformationMiddleware(RequestDelegate next, ExceptionTransformationOptions options, ILoggerFactory loggerFactory)
        {
            _clearCacheHeadersDelegate = ClearCacheHeaders;
            _logger = loggerFactory.CreateLogger<ExceptionTransformationMiddleware>();
            _next = next;
            _options = options;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(0, "An unhandled exception has occurred: " + ex.Message, ex);
                if (context.Response.HasStarted)
                {
                    _logger.LogWarning("The response has already started, the error handler will not be executed.");
                    throw;
                }

                try
                {
                    context.Response.Clear();
                    context.Response.StatusCode = _options.Transformations.TransformException(ex, 500);
                    context.Response.OnStarting(_clearCacheHeadersDelegate, context.Response);

                    return;
                }
                catch (Exception ex2)
                {
                    _logger.LogError(0, "An exception was thrown attempting to execute the error handler.", ex2);
                }
                throw;
            }
        }

        private Task ClearCacheHeaders(object state)
        {
            var response = (HttpResponse)state;
            response.Headers[HeaderNames.CacheControl] = "no-cache";
            response.Headers[HeaderNames.Pragma] = "no-cache";
            response.Headers[HeaderNames.Expires] = "-1";
            response.Headers.Remove(HeaderNames.ETag);
            return Task.FromResult(0);
        }
    }
}
