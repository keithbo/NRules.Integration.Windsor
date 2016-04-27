namespace NRules.Integration.Windsor
{
    using System;
    using Castle.MicroKernel;
    using Castle.Windsor;

    public class WindsorDependencyResolver : NRules.IDependencyResolver
    {
        private readonly IKernel _kernel;

        public WindsorDependencyResolver(IWindsorContainer container)
            : this(container.Kernel)
        {
        }

        public WindsorDependencyResolver(IKernel kernel)
        {
            _kernel = kernel;
        }

        public object Resolve(IResolutionContext context, Type serviceType)
        {
            return _kernel.Resolve(serviceType);
        }
    }
}
