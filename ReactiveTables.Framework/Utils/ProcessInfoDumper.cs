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
using System.Linq;
using System.Text;

namespace ReactiveTables.Framework.Utils
{
    public static class ProcessInfoDumper
    {
        public static void Dump()
        {
            string info = GetProcessInfo();
            Console.WriteLine(info);
        }

        public static string GetProcessInfo()
        {
            StringBuilder info = new StringBuilder();
            foreach (var category in PerformanceCounterCategory.GetCategories().Where(c => c.CategoryName.StartsWith(".NET")))
            {
                try
                {
                    info.AppendLine(category.CategoryName);
//                    Console.WriteLine(category.CategoryName);
                    foreach (var counter in category.GetCounters(Process.GetCurrentProcess().ProcessName))
                    {
                        try
                        {
                            info.Append('\t');
                            info.AppendLine(string.Format("{0} : {1:N}", counter.CounterName, counter.NextValue()));
//                            Console.WriteLine("\t" + counter.CounterName);
                        }
                        catch (Exception e)
                        {
//                            Console.WriteLine(e);
                        }
                    }
                }
                catch (Exception ex)
                {
//                    Console.WriteLine(ex);
                }
            }

            return info.ToString();
        }
    }
}