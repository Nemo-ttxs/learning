namespace Autofac.AttributeExtensions.Attributes
{
    /// <summary>
    /// named parameter
    /// </summary>
    public sealed class NamedAttribute : ParameterRegistrationAttribute
    {
        /// <summary>
        /// init <see cref="NamedAttribute"/>
        /// </summary>
        /// <param name="name"></param>
        public NamedAttribute(string name) : base(name) { }
    }
}
