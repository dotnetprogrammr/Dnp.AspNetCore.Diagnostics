namespace DNP.AspNetCore.Diagnostics
{
    using System;

    public interface ITransformationCollection : IMapping
    {
        void AddMappingFor<T>(int statusCode) where T : Exception;

        int TransformException(Exception ex, int? defaultStatusCode = null);
    }
}