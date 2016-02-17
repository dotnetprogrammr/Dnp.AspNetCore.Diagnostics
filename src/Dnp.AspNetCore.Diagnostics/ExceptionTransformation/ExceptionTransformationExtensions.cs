using System;

using DNP.AspNetCore.Diagnostics;

using Microsoft.AspNet.Builder;

namespace Microsoft.AspNetCore.Builder
{
    public static class ExceptionTransformationExtensions
    {
        /// <summary>
        /// Adds middleware to the pipeline that will catch exceptions, log them, and set the status code on the response.
        /// The status code of the response will not be altered is the response has already started.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseExceptionTransformations(this IApplicationBuilder app, ExceptionTransformationOptions options)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return app.UseMiddleware<ExceptionTransformationMiddleware>(options);
        }

        /// <summary>
        /// Adds middleware to the pipeline that will catch exceptions, log them, and set the status code on the response.
        /// The status code of the response will not be altered is the response has already started.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="transformations"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseExceptionTransformations(this IApplicationBuilder app, TransformationCollection transformations)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (transformations == null)
            {
                throw new ArgumentNullException(nameof(transformations));
            }

            var options = new ExceptionTransformationOptions(transformations);
            return app.UseMiddleware<ExceptionTransformationMiddleware>(options);
        }
    }
}
