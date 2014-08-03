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

namespace ReactiveTables.Framework.PerformanceTests
{
    class LogWriter : IDisposable
    {
        private const int LogInterval = 1000;
        private readonly StreamWriter _logWriter;
        private long _lastLog;

        public LogWriter(Stream logStream)
        {
            _logWriter = new StreamWriter(logStream);
            _logWriter.WriteLine(SystemState.DumpCsvHeader());
        }

        public void Dispose()
        {
//            _logWriter.WriteLine(SystemState.Create().DumpCsv());
            _logWriter.Dispose();
        }

        public void WriteEntry(string dumpCsv)
        {
            _logWriter.WriteLine(dumpCsv);
            _logWriter.Flush();
        }

        public void LogState(Stopwatch watch)
        {
            var elapsedMilliseconds = watch.ElapsedMilliseconds;
            if (elapsedMilliseconds - _lastLog > LogInterval)
            {
                _lastLog = elapsedMilliseconds;
//                WriteEntry(SystemState.Create().DumpCsv());
            }
        }
    }
}