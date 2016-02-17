using System;

namespace DNP.AspNetCore.Diagnostics
{
    public interface IStatusCodeMapping
    {
        IExceptionMapping To<T>() where T : Exception;
    }
}