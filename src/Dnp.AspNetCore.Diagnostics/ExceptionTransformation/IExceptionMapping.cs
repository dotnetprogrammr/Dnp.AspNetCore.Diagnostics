using System;

namespace DNP.AspNetCore.Diagnostics
{
    public interface IExceptionMapping : IMapping
    {
        IExceptionMapping Or<T>() where T : Exception;
    }
}