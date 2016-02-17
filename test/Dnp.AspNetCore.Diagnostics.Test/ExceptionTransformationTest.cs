using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.TestHost;
using Microsoft.AspNetCore.Builder;

using Xunit;

namespace DNP.AspNetCore.Diagnostics.Test
{
    public class ExceptionTransformationTest
    {
        [Fact]
        public async Task DoesNotModifyCacheHeaders_WhenNoExceptionIsThrown()
        {
            var expiresTime = DateTime.UtcNow.AddDays(10).ToString("R");
            var expectedResponseBody = "Hello world!";
            var builder = new WebHostBuilder()
                .UseStartup(app =>
                {
                    var options = new ExceptionTransformationOptions();
                    options.Transformations.Map(404).To<Exception>();

                    app.UseExceptionTransformations(options);

                    app.Run(async (httpContext) =>
                    {
                        httpContext.Response.Headers.Add("Cache-Control", new[] { "max-age=3600" });
                        httpContext.Response.Headers.Add("Pragma", new[] { "max-age=3600" });
                        httpContext.Response.Headers.Add("Expires", new[] { expiresTime });
                        httpContext.Response.Headers.Add("ETag", new[] { "abcdef" });

                        await httpContext.Response.WriteAsync(expectedResponseBody);
                    });
                });

            using (var server = new TestServer(builder))
            {
                var client = server.CreateClient();
                var response = await client.GetAsync(string.Empty);
                response.EnsureSuccessStatusCode();
                Assert.Equal(expectedResponseBody, await response.Content.ReadAsStringAsync());
                IEnumerable<string> values;
                Assert.True(response.Headers.TryGetValues("Cache-Control", out values));
                Assert.Single(values);
                Assert.Equal("max-age=3600", values.First());
                Assert.True(response.Headers.TryGetValues("Pragma", out values));
                Assert.Single(values);
                Assert.Equal("max-age=3600", values.First());
                Assert.True(response.Content.Headers.TryGetValues("Expires", out values));
                Assert.Single(values);
                Assert.Equal(expiresTime, values.First());
                Assert.True(response.Headers.TryGetValues("ETag", out values));
                Assert.Single(values);
                Assert.Equal("abcdef", values.First());
            }
        }

        [Fact]
        public async Task DoesNotClearCacheHeaders_WhenResponseHasAlreadyStarted()
        {
            var expiresTime = DateTime.UtcNow.AddDays(10).ToString("R");
            var builder = new WebHostBuilder()
                .UseStartup(app =>
                {
                    app.Use(async (httpContext, next) =>
                    {
                        Exception exception = null;
                        try
                        {
                            await next();
                        }
                        catch (InvalidOperationException ex)
                        {
                            exception = ex;
                        }

                        Assert.NotNull(exception);
                        Assert.Equal("Something bad happened", exception.Message);
                    });

                    var options = new ExceptionTransformationOptions();
                    options.Transformations.Map(404).To<Exception>();

                    app.UseExceptionTransformations(options);

                    app.Run(async (httpContext) =>
                    {
                        httpContext.Response.Headers.Add("Cache-Control", new[] { "max-age=3600" });
                        httpContext.Response.Headers.Add("Pragma", new[] { "max-age=3600" });
                        httpContext.Response.Headers.Add("Expires", new[] { expiresTime });
                        httpContext.Response.Headers.Add("ETag", new[] { "abcdef" });

                        await httpContext.Response.WriteAsync("Hello");

                        throw new InvalidOperationException("Something bad happened");
                    });
                });

            using (var server = new TestServer(builder))
            {
                var client = server.CreateClient();
                var response = await client.GetAsync(string.Empty);
                response.EnsureSuccessStatusCode();
                Assert.Equal("Hello", await response.Content.ReadAsStringAsync());
                IEnumerable<string> values;
                Assert.True(response.Headers.TryGetValues("Cache-Control", out values));
                Assert.Single(values);
                Assert.Equal("max-age=3600", values.First());
                Assert.True(response.Headers.TryGetValues("Pragma", out values));
                Assert.Single(values);
                Assert.Equal("max-age=3600", values.First());
                Assert.True(response.Content.Headers.TryGetValues("Expires", out values));
                Assert.Single(values);
                Assert.Equal(expiresTime, values.First());
                Assert.True(response.Headers.TryGetValues("ETag", out values));
                Assert.Single(values);
                Assert.Equal("abcdef", values.First());
            }
        }

        [Fact]
        public async Task ShoudNotHandleUnhandledExceptions_WhenTheResponseHasAlreadyStarted()
        {
            var builder = new WebHostBuilder().UseStartup(app =>
                {
                    app.Use(async (httpContext, next) =>
                    {
                        Exception exception = null;
                        try
                        {
                            await next();
                        }
                        catch (InvalidOperationException ex)
                        {
                            exception = ex;
                        }

                        Assert.NotNull(exception);
                        Assert.Equal("Something bad happened", exception.Message);
                    });

                    var options = new ExceptionTransformationOptions();
                    options.Transformations.Map(404).To<Exception>();

                    app.UseExceptionTransformations(options);

                    app.Run(async (httpContext) =>
                    {
                        await httpContext.Response.WriteAsync("Hello");
                        throw new InvalidOperationException("Something bad happened");
                    });
                });

            using (var server = new TestServer(builder))
            {
                var client = server.CreateClient();
                var response = await client.GetAsync(string.Empty);
                response.EnsureSuccessStatusCode();
                Assert.Equal("Hello", await response.Content.ReadAsStringAsync());
            }
        }

        [Fact]
        public async Task ShouldOnlyHandle_UnhandledExceptions()
        {
            var builder = new WebHostBuilder().UseStartup(app =>
            {
                var options = new ExceptionTransformationOptions();
                options.Transformations.Map(404).To<Exception>();

                app.UseExceptionTransformations(options);

                app.Run((RequestDelegate)(async (context) =>
                {
                    context.Response.StatusCode = 500;
                    context.Response.ContentType = "text/plain; charset=utf-8";
                    await context.Response.WriteAsync("An error occurred whilst handling the request.");
                }));
            });

            using (var server = new TestServer(builder))
            {
                var client = server.CreateClient();
                var response = await client.GetAsync(string.Empty);
                Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
                Assert.Equal("An error occurred whilst handling the request.", await response.Content.ReadAsStringAsync());
            }
        }

        [Fact]
        public async Task ShouldSetTheStatusCode_OnHandledExceptions()
        {
            var builder = new WebHostBuilder().UseStartup(app =>
            {
                var options = new ExceptionTransformationOptions();
                options.Transformations.Map(404).To<InvalidOperationException>();

                app.UseExceptionTransformations(options);

                app.Run(context =>
                {
                    throw new InvalidOperationException("An error occurred whilst handling the request.");
                });
            });

            using (var server = new TestServer(builder))
            {
                var client = server.CreateClient();
                var response = await client.GetAsync(string.Empty);
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            }
        }
    }
}
