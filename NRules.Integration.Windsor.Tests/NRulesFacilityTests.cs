namespace NRules.Integration.Windsor.Tests
{
    using System.Linq;
    using Castle.MicroKernel.Registration;
    using Castle.Windsor;
    using NRules.RuleModel;
    using Xunit;

    public class NRulesFacilityTests
    {
        [Fact]
        public void BasicTest()
        {
            using (var container = new WindsorContainer())
            {
                container.AddFacility<NRulesFacility>();
                container.Register(Component.For<InputOutputTestRule>());

                var facility = (NRulesFacility)container.Kernel.GetFacilities().Single(f => f is NRulesFacility);
                Assert.Contains(typeof(InputOutputTestRule), facility.KnownRuleTypes);

                var ruleRepository = container.Resolve<IRuleRepository>();
                Assert.NotNull(ruleRepository);
                var sessionFactory = container.Resolve<ISessionFactory>();
                Assert.NotNull(sessionFactory);
                var session = container.Resolve<ISession>();
                Assert.NotNull(session);

                session.Insert(new TestInput());
                session.Fire();
                Assert.Single(session.Query<TestOutput>().AsEnumerable());
            }
        }

        [Fact]
        public void NamedEmptySpecTest()
        {
            using (var container = new WindsorContainer())
            {
                container.AddFacility<NRulesFacility>(f => f.Use("Test", spec => { }));
                container.Register(Component.For<InputOutputTestRule>());

                var facility = (NRulesFacility)container.Kernel.GetFacilities().Single(f => f is NRulesFacility);
                Assert.Contains(typeof(InputOutputTestRule), facility.KnownRuleTypes);

                var ruleRepository = container.Resolve<IRuleRepository>("TestRuleRepository");
                Assert.NotNull(ruleRepository);
                var sessionFactory = container.Resolve<ISessionFactory>("TestSessionFactory");
                Assert.NotNull(sessionFactory);
                var session = container.Resolve<ISession>("Test");
                Assert.NotNull(session);

                session.Insert(new TestInput());
                session.Fire();
                Assert.Empty(session.Query<TestOutput>().AsEnumerable());
            }
        }

        [Fact]
        public void NamedUsingSpecTest()
        {
            using (var container = new WindsorContainer())
            {
                container.AddFacility<NRulesFacility>(f => f.Use("Test", spec => { spec.From(typeof(InputOutputTestRule)); }));
                container.Register(Component.For<InputOutputTestRule>());

                var facility = (NRulesFacility)container.Kernel.GetFacilities().Single(f => f is NRulesFacility);
                Assert.Contains(typeof(InputOutputTestRule), facility.KnownRuleTypes);

                var ruleRepository = container.Resolve<IRuleRepository>("TestRuleRepository");
                Assert.NotNull(ruleRepository);
                var sessionFactory = container.Resolve<ISessionFactory>("TestSessionFactory");
                Assert.NotNull(sessionFactory);
                var session = container.Resolve<ISession>("Test");
                Assert.NotNull(session);

                session.Insert(new TestInput());
                session.Fire();
                Assert.Single(session.Query<TestOutput>().AsEnumerable());
            }
        }
    }
}
