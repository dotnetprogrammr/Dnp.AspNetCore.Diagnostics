using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;

namespace ExceptionTransformationSample
{
    using System;

    using Dnp.AspNetCore.Diagnostics;

    using Microsoft.AspNetCore.Builder;

    public class Startup
    {
        public void Configure(IApplicationBuilder app)
        {
            var transformations = BuildTransformations();
            app.UseExceptionTransformations(transformations);

            app.Map("/throw", throwApp =>
            {
                throwApp.Run(context => { throw new ArgumentOutOfRangeException("Application Exception"); });
            });

            app.Run(async context =>
            {
                context.Response.ContentType = "text/html";
                await context.Response.WriteAsync("<html><body>Welcome to the sample<br><br>\r\n");
                await context.Response.WriteAsync("Click here to throw an exception: <a href=\"/throw\">throw</a>\r\n");
                await context.Response.WriteAsync("</body></html>\r\n");
            });
        }

        public static void Main(string[] args) => WebApplication.Run<Startup>(args);

        private static ITransformationCollection BuildTransformations()
        {
            return TransformationCollectionBuilder
                .Create()
                .Map(404)
                .To<ArgumentNullException>()
                .Or<ArgumentOutOfRangeException>()
                .Transformations;
        }
    }
}
