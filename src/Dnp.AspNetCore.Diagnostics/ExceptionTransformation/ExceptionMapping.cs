using System;

namespace DNP.AspNetCore.Diagnostics
{
    public sealed class ExceptionMapping : IExceptionMapping
    {
        internal ExceptionMapping(ITransformationCollection transformations, int statusCode)
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
        /// Defines a new status code against which exceptions are to be mapped.
        /// </summary>
        /// <param name="statusCode">The status code.</param>
        /// <returns>A <see cref="IStatusCodeMapping"/> instance.</returns>
        public IStatusCodeMapping Map(int statusCode)
        {
            return new StatusCodeMapping(TransformationCollection, statusCode);
        }

        /// <summary>
        /// Maps an exception of <typeparamref name="T"/> to the defined status code.
        /// </summary>
        /// <typeparam name="T">The exception type to map.</typeparam>
        /// <returns>A <see cref="IExceptionMapping"/> instance.</returns>
        public IExceptionMapping Or<T>() where T : Exception
        {
            TransformationCollection.AddMappingFor<T>(StatusCode);
            return new ExceptionMapping(TransformationCollection, StatusCode);
        }
    }
}