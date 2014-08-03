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
    public class TimedPerformanceTest : PerformanceTestBase
    {
        internal TimedPerformanceTest(Func<ITest> test)
            : base(test)
        {
        }

        public void Run(int seconds, int iterationPause)
        {
            Run(() => RunTest(5, 10, new MemoryStream()),
                fileStream => RunTest(seconds, iterationPause, fileStream));
        }

        private void RunTest(int seconds, int iterationPause, Stream logStream)
        {
            var test = Test();

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