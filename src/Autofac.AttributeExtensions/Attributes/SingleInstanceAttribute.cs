namespace Autofac.AttributeExtensions.Attributes
{
    /// <summary>
    /// singleton registration
    /// </summary>
    public sealed class SingleInstanceAttribute : RegistrationAttribute
    {
        /// <summary>
        /// init <see cref="SingleInstanceAttribute"/>
        /// </summary>
        public SingleInstanceAttribute() : base(LifeTimeScope.SingleInstance)
        {
        }
    }
}
