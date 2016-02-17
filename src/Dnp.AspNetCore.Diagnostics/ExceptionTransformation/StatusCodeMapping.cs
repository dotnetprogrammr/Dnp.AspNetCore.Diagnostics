using System;

namespace DNP.AspNetCore.Diagnostics
{
    public sealed class StatusCodeMapping : IStatusCodeMapping
    {
        public StatusCodeMapping(ITransformationCollection transformations, int statusCode)
        {
            if (transformations == null)
            {
                throw new ArgumentNullException(nameof(transformations));
            }

            StatusCode = statusCode;
            TransformationCollection = transformations;
        }

        internal int StatusCode { get; }

        internal ITransformationCollection TransformationCollection { get; }

        /// <summary>
        /// Maps an exception of <typeparamref name="T"/> to the defined status code.
        /// </summary>
        /// <typeparam name="T">The exception type to map.</typeparam>
        /// <returns>A <see cref="IExceptionMapping"/> instance.</returns>
        public IExceptionMapping To<T>() where T : Exception
        {
            TransformationCollection.AddMappingFor<T>(StatusCode);
            return new ExceptionMapping(TransformationCollection, StatusCode);
        }
    }
}