namespace ReactiveTables.Framework.PerformanceTests.Tests
{
    internal interface ITest
    {
        void Iterate();
        long Metric { get; }
    }
}