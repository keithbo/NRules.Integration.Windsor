namespace NRules.Integration.Windsor.Tests
{
    using Castle.MicroKernel.Registration;
    using Castle.Windsor;
    using Moq;
    using Xunit;

    public class WindsorDependencyResolverTests
    {
        [Fact]
        public void BasicTest()
        {
            var serviceMock = new Mock<ITestService>();

            using (var container = new WindsorContainer())
            {
                container.AddFacility<NRulesFacility>();
                container.Register(Component.For<DependencyCallTestRule>());
                container.Register(Component.For<ITestService>().Instance(serviceMock.Object));

                var session = container.Resolve<ISession>();
                Assert.NotNull(session);

                session.Insert(new TestInput());
                session.Fire();
            }

            serviceMock.Verify(x => x.Do(), Times.Once());
        }
    }
}