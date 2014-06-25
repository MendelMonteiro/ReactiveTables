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
    public class PerformanceTest
    {
        private readonly Func<ITest> _test;

        public static void Main(string[] args)
        {
            try
            {
                PerformanceTest test = new PerformanceTest(() => new GroupedTableTest(70000));
                int seconds;
                if (args.Length < 1 || !int.TryParse(args[0], out seconds)) seconds = 30;
                int iterationPause;
                if (args.Length < 2 || !int.TryParse(args[1], out iterationPause)) iterationPause = 0;

                test.Run(seconds, iterationPause);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private PerformanceTest(Func<ITest> test)
        {
            _test = test;
        }

        private void Run(int seconds, int iterationPause)
        {
            LogAction("- Warming up");
            RunTest(5, 10, new MemoryStream());
            GC.Collect(2, GCCollectionMode.Forced, true);

            SystemState stateBefore = new SystemState();
            LogAction("- State before:");
            Console.WriteLine(stateBefore);

            LogAction("- Running test");
            using (var fileStream = new FileStream("perf-log.csv", FileMode.Create, FileAccess.ReadWrite, FileShare.None))
            {
                RunTest(seconds, iterationPause, fileStream);
            }

            SystemState stateAfter = new SystemState();
            LogAction("- State after:");
            Console.WriteLine(stateAfter);

            LogAction("- Difference:");
            stateAfter.DumpDifference(stateBefore);
        }

        private static void LogAction(string action)
        {
            Console.WriteLine();
            Console.WriteLine(action);
        }

        private void RunTest(int seconds, int iterationPause, Stream logStream)
        {
            var test = _test();

            using (var logWriter = new StreamWriter(logStream))
            {
                logWriter.WriteLine(SystemState.DumpCsvHeader());

                var watch = new Stopwatch();
                watch.Start();
                long lastLog = 0;
                while (watch.Elapsed.TotalSeconds < seconds)
                {
                    var elapsedMilliseconds = watch.ElapsedMilliseconds;
                    if (elapsedMilliseconds - lastLog > 1000)
                    {
                        lastLog = elapsedMilliseconds;
                        logWriter.WriteLine(new SystemState().DumpCsv());
                        logWriter.Flush();
                    }

                    test.Iterate();

                    if (iterationPause > 0) Thread.Sleep(iterationPause);
                    //SpinWait.SpinUntil(() => false, iterationPause);
                }

                logWriter.WriteLine(new SystemState().DumpCsv());
                logWriter.Close();
            }

            Console.WriteLine("Items processed: {0:N0}", test.Metric);
        }
    }

    internal class SystemState
    {
        private static readonly PerformanceCounter _allBytes;
        private static readonly PerformanceCounter _gen0Collections;
        private static readonly PerformanceCounter _gen1Collections;
        private static readonly PerformanceCounter _gen2Collections;
        private static readonly PerformanceCounter _gen0Size;
        private static readonly PerformanceCounter _gen1Size;
        private static readonly PerformanceCounter _gen2Size;
        private static readonly PerformanceCounter _largeObjectHeap;
        private static readonly PerformanceCounter _timeInGc;

        public long HeapSize { get; private set; }
        public long Gen0Size { get; private set; }
        public long Gen1Size { get; private set; }
        public long Gen2Size { get; private set; }
        public long LargeObjectHeapSize { get; private set; }
        public int Gen0Collections { get; private set; }
        public int Gen1Collections { get; private set; }
        public int Gen2Collections { get; private set; }
        public double TimeInGc { get; private set; }

        static SystemState()
        {
            string instance = Process.GetCurrentProcess().ProcessName;
            _allBytes = new PerformanceCounter(".NET CLR Memory", "# Bytes in all Heaps", instance);

            _gen0Collections = new PerformanceCounter(".NET CLR Memory", "# Gen 0 Collections", instance);
            _gen1Collections = new PerformanceCounter(".NET CLR Memory", "# Gen 1 Collections", instance);
            _gen2Collections = new PerformanceCounter(".NET CLR Memory", "# Gen 2 Collections", instance);

            _gen0Size = new PerformanceCounter(".NET CLR Memory", "Gen 0 heap size", instance);
            _gen1Size = new PerformanceCounter(".NET CLR Memory", "Gen 1 heap size", instance);
            _gen2Size = new PerformanceCounter(".NET CLR Memory", "Gen 2 heap size", instance);

            _largeObjectHeap = new PerformanceCounter(".NET CLR Memory", "Large Object Heap size", instance);
            _timeInGc = new PerformanceCounter(".NET CLR Memory", "% Time in GC", instance);
        }

        public SystemState(bool initialise = true)
        {
            if (initialise)
            {
                HeapSize = (long) _allBytes.NextValue();

                Gen0Collections = (int) _gen0Collections.NextValue();
                Gen1Collections = (int) _gen1Collections.NextValue();
                Gen2Collections = (int) _gen2Collections.NextValue();

                Gen0Size = (long) _gen0Size.NextValue();
                Gen1Size = (long) _gen1Size.NextValue();
                Gen2Size = (long) _gen2Size.NextValue();

                LargeObjectHeapSize = (long) _largeObjectHeap.NextValue();
                TimeInGc = _timeInGc.NextValue();
            }
        }

        public void DumpBeforeAndAfter(SystemState stateBefore)
        {
            Console.WriteLine("Before");
            Console.WriteLine(stateBefore);
            Console.WriteLine();
            Console.WriteLine("After");
            Console.WriteLine(this);
        }

        public void DumpDifference(SystemState stateBefore)
        {
            SystemState diff = new SystemState(false);
            diff.Gen0Collections = Gen0Collections - stateBefore.Gen0Collections;
            diff.Gen1Collections = Gen1Collections - stateBefore.Gen1Collections;
            diff.Gen2Collections = Gen2Collections - stateBefore.Gen2Collections;

            diff.Gen0Size = Gen0Size - stateBefore.Gen0Size;
            diff.Gen1Size = Gen1Size - stateBefore.Gen1Size;
            diff.Gen2Size = Gen2Size - stateBefore.Gen2Size;

            diff.HeapSize = HeapSize - stateBefore.HeapSize;
            diff.LargeObjectHeapSize = LargeObjectHeapSize - stateBefore.LargeObjectHeapSize;
            diff.TimeInGc = TimeInGc - stateBefore.TimeInGc;

            Console.WriteLine(diff);
        }

        public static string DumpCsvHeader()
        {
            return "HeapSize; Gen0Size; Gen1Size; Gen2Size; LargeObjectHeapSize; Gen0Collections; Gen1Collections; Gen2Collections; TimeInGc";
        }

        public string DumpCsv()
        {
            return string.Format("{0:N0};{1:N0};{2:N0};{3:N0};{4:N0};{5:N0};{6:N0};{7:N0};{8:N0}",
                                 HeapSize, Gen0Size, Gen1Size, Gen2Size, LargeObjectHeapSize, Gen0Collections, Gen1Collections,
                                 Gen2Collections, TimeInGc);
        }

        public override string ToString()
        {
            return
                string.Format(
                    "HeapSize: {0:N0}\nGen0Size: {1:N0}\nGen1Size: {2:N0}\nGen2Size: {3:N0}\nLargeObjectHeapSize: {4:N0}\nGen0Collections: {5:N0}\nGen1Collections: {6:N0}\nGen2Collections: {7:N0}\nTimeInGc: {8:N0}",
                    HeapSize, Gen0Size, Gen1Size, Gen2Size, LargeObjectHeapSize, Gen0Collections, Gen1Collections, Gen2Collections, TimeInGc);
        }
    }
}