using System;
using System.IO;
using ReactiveTables.Framework.PerformanceTests.Tests;

namespace ReactiveTables.Framework.PerformanceTests
{
    public class PerformanceTestBase
    {
        protected readonly Func<ITest> Test;

        protected PerformanceTestBase(Func<ITest> test)
        {
            Test = test;
        }

        protected void Run(Action runWarmup, Action<Stream> runTest)
        {
            WarmUp(runWarmup);

//            var stateBefore = DumpStateBefore();

            LogAction("- Running test");
            using (var fileStream = new FileStream("perf-log.csv", FileMode.Create, FileAccess.ReadWrite, FileShare.None))
            {
                runTest(fileStream);
            }

//            var stateAfter = DumpStateAfter();

//            LogAction("- Difference:");
//            stateAfter.DumpDifference(stateBefore);
        }

        protected static SystemState DumpStateAfter()
        {
            var stateAfter = SystemState.Create();
            LogAction("- State after:");
            Console.WriteLine(stateAfter);
            return stateAfter;
        }

        protected static SystemState DumpStateBefore()
        {
            var stateBefore = SystemState.Create();
            LogAction("- State before:");
            Console.WriteLine(stateBefore);
            return stateBefore;
        }

        protected void WarmUp(Action runTest)
        {
            LogAction("- Warming up");
            runTest();
            GC.Collect(2, GCCollectionMode.Forced, true);
        }

        protected static void LogAction(string action)
        {
            Console.WriteLine();
            Console.WriteLine(action);
        }
    }
}