namespace NRules.Integration.Windsor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Castle.MicroKernel;
    using Castle.Windsor;
    using NRules.Fluent;
    using NRules.Fluent.Dsl;

    public class WindsorRuleActivator : IRuleActivator
    {
        private readonly IKernel _kernel;

        public WindsorRuleActivator(IWindsorContainer container)
            : this(container.Kernel)
        {
        }

        public WindsorRuleActivator(IKernel kernel)
        {
            _kernel = kernel;
        }

        public IEnumerable<Rule> Activate(Type type)
        {
            return _kernel.ResolveAll(type).Cast<Rule>();
        }
    }
}
