namespace NRules.Integration.Windsor.Tests
{
    using NRules.Fluent.Dsl;
    using NRules.RuleModel;

    public class DependencyCallTestRule : Rule
    {
        /// <inheritdoc />
        public override void Define()
        {
            ITestService service = null;

            When()
                .Match<TestInput>();

            Dependency()
                .Resolve(() => service);

            Then()
                .Do(ctx => service.Do());
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