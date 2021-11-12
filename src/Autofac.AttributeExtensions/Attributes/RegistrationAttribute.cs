using System;

namespace Autofac.AttributeExtensions.Attributes
{
    /// <summary>
    /// registry declaration
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.GenericParameter, AllowMultiple = true)]
    public abstract class RegistrationAttribute : Attribute
    {
        /// <summary>
        /// life time scope of registration
        /// </summary>
        public LifeTimeScope LifeTimeScope { get; set; }

        /// <summary>
        /// init <see cref="RegistrationAttribute"/>
        /// </summary>
        /// <param name="lifeTimeScope"></param>
        protected RegistrationAttribute(LifeTimeScope lifeTimeScope) => LifeTimeScope = lifeTimeScope;

        /// <summary>
        /// life time scope tags
        /// </summary>
        public object[]? LifetimeScopeTags { get; protected set; }

        /// <summary>
        /// name of registration
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// key of registration
        /// </summary>
        public object? Key { get; set; }

        /// <summary>
        /// registry as implemented interfaces
        /// default is true
        /// </summary>
        public bool AsImplementedInterfaces { get; set; } = true;

        /// <summary>
        /// registry as specific type
        /// </summary>
        public Type[]? As { get; set; }

        /// <summary>
        /// default tag for request life time
        /// </summary>
        public static readonly object RequestLifetimeScopeTag = new object();
    }
}
