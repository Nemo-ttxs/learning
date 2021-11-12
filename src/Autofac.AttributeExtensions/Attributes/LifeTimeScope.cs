namespace Autofac.AttributeExtensions.Attributes
{
    /// <summary>
    /// life time scope for registration
    /// </summary>
    public enum LifeTimeScope
    {
        /// <summary>
        /// instance per dependency
        /// </summary>
        InstancePerDependency,

        /// <summary>
        /// instance per life time scope
        /// </summary>
        InstancePerLifetimeScope,

        /// <summary>
        /// singleton
        /// </summary>
        SingleInstance
    }
}
