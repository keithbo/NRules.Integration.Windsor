namespace NRules.Integration.Windsor.Tests
{
    using Castle.Windsor;
    using System;
    using Xunit;

    public class NRulesFacilityTests
    {
        [Fact]
        public void Test1()
        {
            using (var container = new WindsorContainer())
            {
                container.AddFacility<NRulesFacility>();
            }
        }
    }
}
