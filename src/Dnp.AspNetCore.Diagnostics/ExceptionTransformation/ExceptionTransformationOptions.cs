namespace DNP.AspNetCore.Diagnostics
{
    public class ExceptionTransformationOptions
    {
        public ExceptionTransformationOptions()
        {
            this.Transformations = new TransformationCollection();
        }

        internal ExceptionTransformationOptions(TransformationCollection transformations)
        {
            this.Transformations = transformations;
        }

        public ITransformationCollection Transformations { get; }
    }
}