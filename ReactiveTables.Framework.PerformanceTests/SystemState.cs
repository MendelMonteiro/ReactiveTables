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

namespace ReactiveTables.Framework.PerformanceTests
{
    public struct SystemState
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
        private int _gen0Collections1;
        private long _gen0Size1;
        private int _gen1Collections1;
        private long _gen1Size1;
        private int _gen2Collections1;
        private long _gen2Size1;
        private long _heapSize;
        private long _largeObjectHeapSize;
        private double _timeInGc1;

        static SystemState()
        {
            var instance = Process.GetCurrentProcess().ProcessName;
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

        public static SystemState Create()
        {
            return new SystemState
                   {
                       _heapSize = (long) _allBytes.NextValue(),

                       _gen0Collections1 = (int) _gen0Collections.NextValue(),
                       _gen1Collections1 = (int) _gen1Collections.NextValue(),
                       _gen2Collections1 = (int) _gen2Collections.NextValue(),

                       _gen0Size1 = (long) _gen0Size.NextValue(),
                       _gen1Size1 = (long) _gen1Size.NextValue(),
                       _gen2Size1 = (long) _gen2Size.NextValue(),

                       _largeObjectHeapSize = (long) _largeObjectHeap.NextValue(),
                       _timeInGc1 = _timeInGc.NextValue(),
                   };
        }

        private SystemState(SystemState stateBefore, SystemState systemState)
        {
            _gen0Collections1 = systemState.Gen0Collections - stateBefore.Gen0Collections;
            _gen1Collections1 = systemState.Gen1Collections - stateBefore.Gen1Collections;
            _gen2Collections1 = systemState.Gen2Collections - stateBefore.Gen2Collections;

            _gen0Size1 = systemState.Gen0Size - stateBefore.Gen0Size;
            _gen1Size1 = systemState.Gen1Size - stateBefore.Gen1Size;
            _gen2Size1 = systemState.Gen2Size - stateBefore.Gen2Size;

            _heapSize = systemState.HeapSize - stateBefore.HeapSize;
            _largeObjectHeapSize = systemState.LargeObjectHeapSize - stateBefore.LargeObjectHeapSize;
            _timeInGc1 = systemState.TimeInGc - stateBefore.TimeInGc;
        }

        public long HeapSize => _heapSize;

        public long Gen0Size => _gen0Size1;

        public long Gen1Size => _gen1Size1;

        public long Gen2Size => _gen2Size1;

        public long LargeObjectHeapSize => _largeObjectHeapSize;

        public int Gen0Collections => _gen0Collections1;

        public int Gen1Collections => _gen1Collections1;

        public int Gen2Collections => _gen2Collections1;

        public double TimeInGc => _timeInGc1;

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
            var diff = new SystemState(stateBefore, this);
            Console.WriteLine(diff);
        }

        public static string DumpCsvHeader()
        {
            return "HeapSize; Gen0Size; Gen1Size; Gen2Size; LargeObjectHeapSize; Gen0Collections; Gen1Collections; Gen2Collections; TimeInGc";
        }

        public string DumpCsv()
        {
            return
                $"{HeapSize:N0};{Gen0Size:N0};{Gen1Size:N0};{Gen2Size:N0};{LargeObjectHeapSize:N0};{Gen0Collections:N0};{Gen1Collections:N0};{Gen2Collections:N0};{TimeInGc:N0}";
        }

        public override string ToString()
        {
            return
                $"HeapSize: {HeapSize:N0}\nGen0Size: {Gen0Size:N0}\nGen1Size: {Gen1Size:N0}\nGen2Size: {Gen2Size:N0}\nLargeObjectHeapSize: {LargeObjectHeapSize:N0}\nGen0Collections: {Gen0Collections:N0}\nGen1Collections: {Gen1Collections:N0}\nGen2Collections: {Gen2Collections:N0}\nTimeInGc: {TimeInGc:N0}";
        }
    }
}