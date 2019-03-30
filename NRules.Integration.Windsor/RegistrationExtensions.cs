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
        public static ComponentRegistration<IRuleRepository> WithLoadSpec(this ComponentRegistration<IRuleRepository> registration, Action<IRuleLoadSpec> loadSpecAction)
        {
            return registration
                .OnCreate(r => ((RuleRepository)r).Load(loadSpecAction));
        }

        public static ComponentRegistration<IRuleRepository> AsRuleRepository(this ComponentRegistration<IRuleRepository> registration, string name, Action<IRuleLoadSpec> loadSpecAction)
        {
            return registration
                .AsRuleRepository(loadSpecAction)
                .Named(name);
        }

        public static ComponentRegistration<IRuleRepository> AsRuleRepository(this ComponentRegistration<IRuleRepository> registration, Action<IRuleLoadSpec> loadSpecAction)
        {
            return registration
                .ImplementedBy<RuleRepository>()
                .WithLoadSpec(loadSpecAction)
                .LifestyleSingleton();
        }

        public static ComponentRegistration<ISessionFactory> AsSessionFactory(this ComponentRegistration<ISessionFactory> registration, string name)
        {
            return registration.UsingFactoryMethod(k =>
                {
                    var r = k.Resolve<IRuleRepository>(name);
                    var s = r.Compile();
                    s.DependencyResolver = k.Resolve<NRules.IDependencyResolver>();
                    k.ReleaseComponent(r);
                    return s;
                })
                .LifestyleSingleton()
                .Named(name);
        }

        public static ComponentRegistration<ISessionFactory> AsSessionFactory(this ComponentRegistration<ISessionFactory> registration)
        {
            return registration.UsingFactoryMethod(k =>
                {
                    var r = k.Resolve<IRuleRepository>();
                    var s = r.Compile();
                    s.DependencyResolver = k.Resolve<NRules.IDependencyResolver>();
                    k.ReleaseComponent(r);
                    return s;
                })
                .LifestyleSingleton();
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
                        var f = k.Resolve<ISessionFactory>(name);
                        var s = f.CreateSession();
                        k.ReleaseComponent(f);
                        return s;
                    })
                    .Named(name)
            );
            return kernel;
        }
    }
}
