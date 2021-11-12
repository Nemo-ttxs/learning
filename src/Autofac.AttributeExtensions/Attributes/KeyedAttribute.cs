namespace Autofac.AttributeExtensions.Attributes
{
    /// <summary>
    /// keyed parameter
    /// </summary>
    public sealed class KeyedAttribute : ParameterRegistrationAttribute
    {
        /// <summary>
        /// init <see cref="KeyedAttribute"/>
        /// </summary>
        /// <param name="key"></param>
        public KeyedAttribute(object key) : base(key)
        {
        }
    }
}
