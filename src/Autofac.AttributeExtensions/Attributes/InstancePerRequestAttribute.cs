namespace Autofac.AttributeExtensions.Attributes
{
    /// <summary>
    /// instance per request registration
    /// </summary>
    public class InstancePerRequestAttribute : InstancePerLifetimeScopeAttribute
    {
        /// <summary>
        /// init <see cref="InstancePerRequestAttribute"/>
        /// </summary>
        protected InstancePerRequestAttribute() : base(RequestLifetimeScopeTag)
        {
        }
    }
}
