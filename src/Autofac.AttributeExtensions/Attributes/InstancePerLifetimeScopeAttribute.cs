namespace Autofac.AttributeExtensions.Attributes
{
    /// <summary>
    /// register as instance per lifetime scope
    /// </summary>
    public class InstancePerLifetimeScopeAttribute : RegistrationAttribute
    {
        /// <summary>
        /// init <see cref="InstancePerLifetimeScopeAttribute"/>
        /// </summary>
        public InstancePerLifetimeScopeAttribute() : base(LifeTimeScope.InstancePerLifetimeScope)
        {
        }

        /// <summary>
        /// init <see cref="InstancePerLifetimeScopeAttribute"/>
        /// </summary>
        /// <param name="lifetimeScopeTags"></param>
        public InstancePerLifetimeScopeAttribute(params object[] lifetimeScopeTags) : base(LifeTimeScope.InstancePerLifetimeScope) => LifetimeScopeTags = lifetimeScopeTags;
    }

}
