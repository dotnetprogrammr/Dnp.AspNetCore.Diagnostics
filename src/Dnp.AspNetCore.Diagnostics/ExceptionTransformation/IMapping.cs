namespace DNP.AspNetCore.Diagnostics
{
    public interface IMapping
    {
        IStatusCodeMapping Map(int statusCode);
    }
}