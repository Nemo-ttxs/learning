namespace Autofac.AttributeExtensions.Attributes
{
    /// <summary>
    /// register as instance per depencency
    /// </summary>
    public sealed class InstancePerDependencyAttribute : RegistrationAttribute
    {
        /// <summary>
        /// init <see cref="InstancePerDependencyAttribute"/>
        /// </summary>
        public InstancePerDependencyAttribute() : base(LifeTimeScope.InstancePerDependency)
        {
        }
    }
}
