namespace NRules.Integration.Windsor
{
    using System;
    using Castle.MicroKernel;
    using Castle.Windsor;

    /// <summary>
    /// <see cref="NRules.Extensibility.IDependencyResolver"/> implementation for integration with Castle Windsor.
    /// </summary>
    public class WindsorDependencyResolver : NRules.Extensibility.IDependencyResolver
    {
        private readonly IKernel _kernel;

        /// <summary>
        /// Construct a new WindsorDependencyResolver
        /// </summary>
        /// <param name="container"><see cref="IWindsorContainer"/></param>
        public WindsorDependencyResolver(IWindsorContainer container)
            : this(container.Kernel)
        {
        }

        /// <summary>
        /// Construct a new WindsorDependencyResolver
        /// </summary>
        /// <param name="kernel"><see cref="IKernel"/></param>
        public WindsorDependencyResolver(IKernel kernel)
        {
            _kernel = kernel;
        }

        /// <summary>
        /// Resolve an instance of <paramref name="serviceType"/> using Castle Windsor.
        /// </summary>
        /// <param name="context"><see cref="NRules.Extensibility.IResolutionContext"/>. Not used.</param>
        /// <param name="serviceType">Component type to resolve</param>
        /// <returns>Instance of type <paramref name="serviceType"/></returns>
        public object Resolve(NRules.Extensibility.IResolutionContext context, Type serviceType)
        {
            return _kernel.Resolve(serviceType);
        }
    }
}
