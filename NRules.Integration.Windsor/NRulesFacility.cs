namespace NRules.Integration.Windsor
{
    using System;
    using System.Collections.Generic;
#if NETSTANDARD
    using System.Reflection;
#endif
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
        private const string RuleRepositoryNameFormat = "{0}RuleRepository";
        private const string SessionFactoryNameFormat = "{0}SessionFactory";
        private const string SessionNameFormat = "{0}";

        private Action<IRuleLoadSpec> _loadSpecAction;

        internal readonly HashSet<Type> KnownRuleTypes = new HashSet<Type>();

        internal readonly Dictionary<string, Action<IRuleLoadSpec>> NamedSpecs = new Dictionary<string, Action<IRuleLoadSpec>>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Provide a custom <see cref="IRuleLoadSpec"/> delegate to use during <see cref="RuleRepository"/> load.
        /// </summary>
        /// <param name="specAction">configuration delegate</param>
        public NRulesFacility Use(Action<IRuleLoadSpec> specAction)
        {
            _loadSpecAction = specAction ?? throw new ArgumentNullException(nameof(specAction));

            return this;
        }

        /// <summary>
        /// Provide a custom <see cref="IRuleLoadSpec"/> delegate to use during <see cref="RuleRepository"/> load.
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="specAction">configuration delegate</param>
        public NRulesFacility Use(string prefix, Action<IRuleLoadSpec> specAction)
        {
            if (specAction is null)
            {
                throw new ArgumentNullException(nameof(specAction));
            }

            NamedSpecs.Add(prefix, specAction);

            return this;
        }

        /// <summary>
        /// Initialize the facility
        /// </summary>
        protected override void Init()
        {
            Kernel.Register(
                Component.For<NRules.Extensibility.IDependencyResolver>()
                    .UsingFactoryMethod(k => new WindsorDependencyResolver(k), true)
                    .LifestyleSingleton(),
                Component.For<IRuleActivator>()
                    .UsingFactoryMethod(k => new WindsorRuleActivator(k), true)
                    .LifestyleSingleton()
            );

            Kernel.Register(
                Component.For<IRuleRepository>()
                    .ImplementedBy<RuleRepository>()
                    .OnCreate((kernel, r) => ((RuleRepository)r).Load(_loadSpecAction ?? OnLoadSpecAction))
                    .LifestyleSingleton(),
                Component.For<ISessionFactory>()
                    .UsingFactoryMethod(k =>
                    {
                        var r = k.Resolve<IRuleRepository>();
                        var s = r.Compile();
                        s.DependencyResolver = k.Resolve<NRules.Extensibility.IDependencyResolver>();
                        k.ReleaseComponent(r);
                        return s;
                    })
                    .LifestyleSingleton(),
                Component.For<ISession>()
                    .UsingFactoryMethod(k => k.Resolve<ISessionFactory>().CreateSession())
            );

            foreach (var pair in NamedSpecs)
            {
                RegisterNamedSpec(pair.Key, pair.Value);
            }

            Kernel.ComponentRegistered += OnComponentRegistered;
        }

        internal void RegisterNamedSpec(string name, Action<IRuleLoadSpec> loadSpecAction)
        {
            Kernel.Register(
                Component.For<IRuleRepository>()
                    .ImplementedBy<RuleRepository>()
                    .OnCreate(r => ((RuleRepository)r).Load(loadSpecAction))
                    .LifestyleSingleton()
                    .Named(string.Format(RuleRepositoryNameFormat, name)),
                Component.For<ISessionFactory>()
                    .UsingFactoryMethod(k =>
                    {
                        var r = k.Resolve<IRuleRepository>(string.Format(RuleRepositoryNameFormat, name));
                        var s = r.Compile();
                        s.DependencyResolver = k.Resolve<NRules.Extensibility.IDependencyResolver>();
                        k.ReleaseComponent(r);
                        return s;
                    })
                    .LifestyleSingleton()
                    .Named(string.Format(SessionFactoryNameFormat, name)),
                Component.For<ISession>()
                    .UsingFactoryMethod(k =>
                    {
                        var f = k.Resolve<ISessionFactory>(string.Format(SessionFactoryNameFormat, name));
                        var s = f.CreateSession();
                        k.ReleaseComponent(f);
                        return s;
                    })
                    .Named(string.Format(SessionNameFormat, name))
            );
        }


        private void OnComponentRegistered(string key, IHandler handler)
        {
            var type = handler.ComponentModel.Implementation;
#if NETSTANDARD
            if (typeof(Rule).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
#elif NETFRAMEWORK
            if (typeof(Rule).IsAssignableFrom(type))
#endif
            {
                KnownRuleTypes.Add(type);
            }
        }

        private void OnLoadSpecAction(IRuleLoadSpec spec)
        {
            if (KnownRuleTypes.Count > 0)
            {
                spec.From(KnownRuleTypes);
            }
        }
    }
}
