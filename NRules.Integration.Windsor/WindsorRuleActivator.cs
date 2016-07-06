namespace NRules.Integration.Windsor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Castle.MicroKernel;
    using Castle.Windsor;
    using NRules.Fluent;
    using NRules.Fluent.Dsl;

    /// <summary>
    /// <see cref="IRuleActivator"/> implementation that wraps Castle Windsor to resolve rules
    /// registered with the container.
    /// </summary>
    public class WindsorRuleActivator : IRuleActivator
    {
        private readonly IKernel _kernel;

        /// <summary>
        /// Construct a new WindsorRuleActivator with an <see cref="IWindsorContainer"/>
        /// </summary>
        /// <param name="container"><see cref="IWindsorContainer"/></param>
        public WindsorRuleActivator(IWindsorContainer container)
            : this(container.Kernel)
        {
        }

        /// <summary>
        /// Construct a new WindsorRuleActivator with an <see cref="IKernel"/>
        /// </summary>
        /// <param name="kernel"><see cref="IKernel"/></param>
        public WindsorRuleActivator(IKernel kernel)
        {
            _kernel = kernel;
        }

        /// <summary>
        /// Activate returns all instances of type <paramref name="type"/>
        /// </summary>
        /// <param name="type">type to activate</param>
        /// <returns>IEnumerable collection of <see cref="Rule"/> instances of type <paramref name="type"/></returns>
        public IEnumerable<Rule> Activate(Type type)
        {
            return _kernel.ResolveAll(type).Cast<Rule>();
        }
    }
}
