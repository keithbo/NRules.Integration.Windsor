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
            Kernel.Register(Component.For<NRules.IDependencyResolver>()
                                     .UsingFactoryMethod(k => new WindsorDependencyResolver(k), true)
                                     .LifestyleSingleton());

            Kernel.Register(Component.For<IRuleRepository>().UsingFactoryMethod(k =>
            {
                var r = new RuleRepository {Activator = new WindsorRuleActivator(Kernel)};
                r.Load(_loadSpecAction ?? OnLoadSpecAction);

                return r;
            }).LifestyleSingleton());
            Kernel.Register(Component.For<ISessionFactory>().UsingFactoryMethod(k =>
            {
                var s = k.Resolve<IRuleRepository>().Compile();
                s.DependencyResolver = k.Resolve<NRules.IDependencyResolver>();
                return s;
            }));
            Kernel.Register(Component.For<ISession>().UsingFactoryMethod(k =>
                k.Resolve<ISessionFactory>().CreateSession()));

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
