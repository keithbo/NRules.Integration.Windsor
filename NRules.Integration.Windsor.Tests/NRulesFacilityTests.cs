namespace NRules.Integration.Windsor.Tests
{
    using System.Linq;
    using Castle.MicroKernel.Registration;
    using Castle.Windsor;
    using NRules.Fluent.Dsl;
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
                container.AddFacility<NRulesFacility>();
                container.Register(Component.For<InputOutputTestRule>());

                container.RegisterNRules("Test", spec => { });

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
                container.AddFacility<NRulesFacility>();
                container.Register(Component.For<InputOutputTestRule>());

                container.RegisterNRules("Test", spec => { spec.From(typeof(InputOutputTestRule)); });

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

        public class InputOutputTestRule : Rule
        {
            /// <inheritdoc />
            public override void Define()
            {
                When()
                    .Match<TestInput>();

                Then()
                    .Do(ctx => Do(ctx));
            }

            public static void Do(IContext ctx)
            {
                ctx.Insert(new TestOutput());
            }
        }
    }
}
