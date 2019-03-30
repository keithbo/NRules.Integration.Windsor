namespace NRules.Integration.Windsor
{
    using Castle.MicroKernel.Registration;
    using NRules.Fluent;
    using NRules.RuleModel;
    using System;
    using Castle.MicroKernel;
    using Castle.Windsor;

    public static class RegistrationExtensions
    {
        private const string RuleRepositoryNameFormat = "{0}RuleRepository";
        private const string SessionFactoryNameFormat = "{0}SessionFactory";
        private const string SessionNameFormat = "{0}";

        public static ComponentRegistration<IRuleRepository> WithLoadSpec(this ComponentRegistration<IRuleRepository> registration, Action<IRuleLoadSpec> loadSpecAction)
        {
            return registration
                .OnCreate(r => ((RuleRepository)r).Load(loadSpecAction));
        }

        public static ComponentRegistration<IRuleRepository> AsRuleRepository(this ComponentRegistration<IRuleRepository> registration, string prefix, Action<IRuleLoadSpec> loadSpecAction)
        {
            return registration
                .ImplementedBy<RuleRepository>()
                .WithLoadSpec(loadSpecAction)
                .LifestyleSingleton()
                .Named(string.Format(RuleRepositoryNameFormat, prefix));
        }

        public static ComponentRegistration<ISessionFactory> AsSessionFactory(this ComponentRegistration<ISessionFactory> registration, string prefix)
        {
            return registration.UsingFactoryMethod(k =>
                {
                    var r = k.Resolve<IRuleRepository>(string.Format(RuleRepositoryNameFormat, prefix));
                    var s = r.Compile();
                    s.DependencyResolver = k.Resolve<NRules.IDependencyResolver>();
                    k.ReleaseComponent(r);
                    return s;
                })
                .LifestyleSingleton()
                .Named(string.Format(SessionFactoryNameFormat, prefix));
        }

        public static IWindsorContainer RegisterNRules(this IWindsorContainer container, string name, Action<IRuleLoadSpec> loadSpecAction)
        {
            container.Kernel.RegisterNRules(name, loadSpecAction);

            return container;
        }

        public static IKernel RegisterNRules(this IKernel kernel, string name, Action<IRuleLoadSpec> loadSpecAction)
        {
            kernel.Register(
                Component.For<IRuleRepository>().AsRuleRepository(name, loadSpecAction),
                Component.For<ISessionFactory>().AsSessionFactory(name),
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
            return kernel;
        }
    }
}
