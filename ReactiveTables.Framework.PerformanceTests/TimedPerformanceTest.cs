// This file is part of ReactiveTables.
// 
// ReactiveTables is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ReactiveTables is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with ReactiveTables.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using ReactiveTables.Framework.PerformanceTests.Tests;

namespace ReactiveTables.Framework.PerformanceTests
{
    public class TimedPerformanceTest
    {
        private readonly Func<ITest> _test;

        internal TimedPerformanceTest(Func<ITest> test)
        {
            _test = test;
        }

        public void Run(int seconds, int iterationPause)
        {
            WarmUp();

            var stateBefore = DumpStateBefore();

            LogAction("- Running test");
            using (var fileStream = new FileStream("perf-log.csv", FileMode.Create, FileAccess.ReadWrite, FileShare.None))
            {
                RunTest(seconds, iterationPause, fileStream);
            }

            var stateAfter = DumpStateAfter();

            LogAction("- Difference:");
            stateAfter.DumpDifference(stateBefore);
        }

        private static SystemState DumpStateAfter()
        {
            SystemState stateAfter = SystemState.Create();
            LogAction("- State after:");
            Console.WriteLine(stateAfter);
            return stateAfter;
        }

        private static SystemState DumpStateBefore()
        {
            SystemState stateBefore = SystemState.Create();
            LogAction("- State before:");
            Console.WriteLine(stateBefore);
            return stateBefore;
        }

        private void WarmUp()
        {
            LogAction("- Warming up");
            RunTest(5, 10, new MemoryStream());
            GC.Collect(2, GCCollectionMode.Forced, true);
        }

        private static void LogAction(string action)
        {
            Console.WriteLine();
            Console.WriteLine(action);
        }

        private void RunTest(int seconds, int iterationPause, Stream logStream)
        {
            var test = _test();

            using (var logWriter = new LogWriter(logStream))
            {
                var watch = new Stopwatch();
                watch.Start();
                while (watch.Elapsed.TotalSeconds < seconds)
                {
                    logWriter.LogState(watch);

                    test.Iterate();

                    if (iterationPause > 0) Thread.Sleep(iterationPause);
                    //SpinWait.SpinUntil(() => false, iterationPause);
                }
            }

            Console.WriteLine("Items processed: {0:N0}", test.Metric);
        }
    }
}