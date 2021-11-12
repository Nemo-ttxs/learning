using System;

namespace Autofac.AttributeExtensions.Attributes
{
    /// <summary>
    /// ParameterRegistrationAttribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public abstract class ParameterRegistrationAttribute : Attribute
    {
        /// <summary>
        /// init <see cref="ParameterRegistrationAttribute"/>
        /// </summary>
        /// <param name="key"></param>
        protected ParameterRegistrationAttribute(object key) => Keyed = key;

        /// <summary>
        /// init <see cref="ParameterRegistrationAttribute"/>
        /// </summary>
        /// <param name="name"></param>
        protected ParameterRegistrationAttribute(string name) => Named = name;

        /// <summary>
        /// name of registration
        /// </summary>
        public string? Named { get; set; }

        /// <summary>
        /// key of registration
        /// </summary>
        public object? Keyed { get; set; }
    }
}
