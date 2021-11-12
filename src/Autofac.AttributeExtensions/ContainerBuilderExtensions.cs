using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac.AttributeExtensions.Attributes;
using Autofac.Core.Lifetime;
using Autofac.Features.AttributeFilters;
using Autofac.Util;
using Registration = Autofac.Builder.IRegistrationBuilder<object, Autofac.Builder.ConcreteReflectionActivatorData, Autofac.Builder.SingleRegistrationStyle>;

namespace Autofac.AttributeExtensions
{
    /// <summary>
    /// extension methods for <see cref="ContainerBuilder"/>
    /// </summary>
    public static class ContainerBuilderExtensions
    {
        /// <summary>
        /// register attributed classes
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="assemblies"></param>
        public static void RegisterAttributedClasses(this ContainerBuilder builder, params Assembly[] assemblies)
        {
            var attributedTypes = assemblies.SelectMany(a => a.GetLoadableTypes());

            foreach (var type in attributedTypes)
            {
                var registrationAttributes = type.GetTypeInfo().GetCustomAttributes<RegistrationAttribute>(false);

                foreach (var attribute in registrationAttributes)
                {
                    var registration = builder.RegisterType(type);

                    ConfigureParameters(registration, type);

                    SetLifeTimeScope(registration, attribute);

                    RegisterAs(registration, attribute, type);

                    ConfigureFilter(registration, type);
                }
            }
        }

        private static void ConfigureParameters(Registration registration, Type type)
        {
            var attributedParameters = type.GetTypeInfo().GetConstructors()
                .SelectMany(c => c.GetParameters())
                .Select(p => new { info = p, attribute = p.GetCustomAttribute<ParameterRegistrationAttribute>() })
                .Where(p => p.attribute != null);

            foreach (var parameter in attributedParameters)
            {
                if (parameter.attribute is KeyedAttribute keyead && keyead.Keyed != null)
                {
                    _ = registration
                        .WithParameter((p, c) => p == parameter.info, (p, c) => c.ResolveKeyed(parameter.attribute.Keyed!, parameter.info.ParameterType));
                }
                else if (parameter.attribute is NamedAttribute named && !string.IsNullOrWhiteSpace(named.Named))
                {
                    _ = registration
                        .WithParameter((p, c) => p == parameter.info, (p, c) => c.ResolveNamed(parameter.attribute.Named!, parameter.info.ParameterType));
                }
            }
        }

        private static void ConfigureFilter(Registration registration, Type type)
        {
            var attributedParameters = type.GetTypeInfo().GetConstructors()
                .SelectMany(c => c.GetParameters())
                .Select(p => new { info = p, attribute = p.GetCustomAttribute<ParameterFilterAttribute>(true) })
                .Where(p => p.attribute != null);
            foreach (var attributedParameter in attributedParameters)
            {
                if (attributedParameter.attribute != null)
                {
                    _ = registration.WithAttributeFiltering();
                }
            }
        }

        private static void SetLifeTimeScope(Registration registration, RegistrationAttribute attribute)
        {
            switch (attribute.LifeTimeScope)
            {
                case LifeTimeScope.InstancePerDependency:
                    _ = registration.InstancePerDependency();
                    break;
                case LifeTimeScope.InstancePerLifetimeScope:
                    if (attribute.LifetimeScopeTags?.Any() ?? false)
                    {
                        _ = registration.InstancePerMatchingLifetimeScope(FixedScopeTags(attribute).ToArray());
                    }
                    else
                    {
                        _ = registration.InstancePerLifetimeScope();
                    }
                    break;
                case LifeTimeScope.SingleInstance:
                    _ = registration.SingleInstance();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static IEnumerable<object> FixedScopeTags(RegistrationAttribute attribute) =>
            // Turns RequestLifetimeScopeTag into Autofac's version
            attribute.LifetimeScopeTags.Select(t => t == RegistrationAttribute.RequestLifetimeScopeTag ? MatchingScopeLifetimeTags.RequestLifetimeScopeTag : t);

        private static void RegisterAs(Registration registration, RegistrationAttribute attribute, Type type)
        {
            if (RegisterOnlyAsNamed(attribute))
            {
                foreach (var asType in RegisterAsTypes(attribute, type))
                {
                    RegisterNamed(registration, attribute, asType);
                }

                if (attribute.As == null)
                {
                    _ = registration.AsSelf();
                }
            }
            else
            {
                foreach (var asType in RegisterAsTypes(attribute, type))
                {
                    RegisterNamed(registration, attribute, asType);
                    RegisterKeyed(registration, attribute, asType);
                    _ = registration.As(asType);
                }
            }
        }
        private static bool RegisterOnlyAsNamed(RegistrationAttribute attribute) => attribute.Name != null && attribute.Key == null;

        private static IEnumerable<Type> RegisterAsTypes(RegistrationAttribute attribute, Type type) => attribute.As
                   ?? (attribute.AsImplementedInterfaces
                       ? type.GetTypeInfo().GetInterfaces().Concat(new[] { type })
                       : new[] { type });

        private static void RegisterNamed(Registration registration, RegistrationAttribute attribute, Type asType)
        {
            if (attribute.Name != null)
            {
                _ = registration.Named(attribute.Name, asType);
            }
        }

        private static void RegisterKeyed(Registration registration, RegistrationAttribute attribute, Type asType)
        {
            if (attribute.Key != null)
            {
                _ = registration.Keyed(attribute.Key, asType);
            }
        }
    }
}
