namespace Dnp.AspNetCore.Diagnostics
{
    public class ExceptionTransformationOptions
    {
        public ExceptionTransformationOptions()
        {
            this.Transformations = new TransformationCollection();
        }

        internal ExceptionTransformationOptions(ITransformationCollection transformations)
        {
            this.Transformations = transformations;
        }

        public ITransformationCollection Transformations { get; }
    }
}