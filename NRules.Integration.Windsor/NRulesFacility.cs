namespace NRules.Integration.Windsor
{
    using System;
    using System.Collections.Generic;
    using Castle.MicroKernel;
    using Castle.MicroKernel.Facilities;
    using Castle.MicroKernel.Registration;
    using NRules.Fluent;
    using NRules.Fluent.Dsl;
    using NRules.RuleModel;

    /// <summary>
    /// Castle Windsor <see cref="IFacility"/> implementation that provides direct integration of NRules
    /// </summary>
    public class NRulesFacility : AbstractFacility
    {
        private Action<IRuleLoadSpec> _loadSpecAction;

        private readonly HashSet<Type> _knownRuleTypes = new HashSet<Type>();

        /// <summary>
        /// Provide a custom <see cref="IRuleLoadSpec"/> delegate to use during <see cref="RuleRepository"/> load.
        /// </summary>
        /// <param name="specAction">configuration delegate</param>
        public void UseLoadSpec(Action<IRuleLoadSpec> specAction)
        {
            if (specAction == null)
            {
                throw new ArgumentNullException("specAction");
            }

            _loadSpecAction = specAction;
        }

        /// <summary>
        /// Initialize the facility
        /// </summary>
        protected override void Init()
        {
            Kernel.Register(
                Component.For<NRules.IDependencyResolver>()
                    .UsingFactoryMethod(k => new WindsorDependencyResolver(k), true)
                    .LifestyleSingleton(),
                Component.For<IRuleActivator>()
                    .UsingFactoryMethod(k => new WindsorRuleActivator(k), true)
                    .LifestyleSingleton(),
                Component.For<IRuleRepository>()
                    .ImplementedBy<RuleRepository>()
                    .OnCreate(r => ((RuleRepository)r).Load(_loadSpecAction ?? OnLoadSpecAction))
                    .LifestyleSingleton(),
                Component.For<ISessionFactory>()
                    .UsingFactoryMethod(k =>
                    {
                        var r = k.Resolve<IRuleRepository>();
                        var s = r.Compile();
                        s.DependencyResolver = k.Resolve<NRules.IDependencyResolver>();
                        k.ReleaseComponent(r);
                        return s;
                    })
                    .LifestyleSingleton(),
                Component.For<ISession>()
                    .UsingFactoryMethod(k => k.Resolve<ISessionFactory>().CreateSession())
            );

            Kernel.ComponentRegistered += OnComponentRegistered;
        }

        private void OnComponentRegistered(string key, IHandler handler)
        {
            var type = handler.ComponentModel.Implementation;
            if (typeof(Rule).IsAssignableFrom(type))
            {
                _knownRuleTypes.Add(type);
            }
        }

        private void OnLoadSpecAction(IRuleLoadSpec spec)
        {
            if (_knownRuleTypes.Count > 0)
            {
                spec.From(_knownRuleTypes);
            }
        }
    }
}
