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
    class IterationPerformanceTest : PerformanceTestBase
    {
        internal IterationPerformanceTest(Func<ITest> test)
            : base(test)
        {
        }

        public void Run(int iterations, int iterationPause)
        {
            Run(() => RunTest(100000, 0, new MemoryStream()),
                fileStream => RunTest(iterations, iterationPause, fileStream));
        }

        private void RunTest(int iterations, int iterationPause, Stream logStream)
        {
            var test = Test();

            var watch = new Stopwatch();
            watch.Start();
            for (int i = 0; i < iterations; i++)
            {
                test.Iterate();

//                if (iterationPause > 0) Thread.Sleep(iterationPause);
            }

            watch.Stop();
            double opsPerMs = iterations/(double)watch.ElapsedMilliseconds;
            Console.WriteLine("Operations: {0:N0} in {1:N0}ms - {2:N} ops/ms",
                              iterations, watch.ElapsedMilliseconds, opsPerMs);
        }
    }
}
