namespace FJS.Common.Metadata
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RootTypeAttribute : Attribute
    {
        public RootTypeAttribute(Type type)
        {

        }
    }
}