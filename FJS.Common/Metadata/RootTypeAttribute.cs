namespace FJS.Common.Metadata
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class RootTypeAttribute : Attribute
    {
        public RootTypeAttribute(Type type)
        {

        }
    }
}