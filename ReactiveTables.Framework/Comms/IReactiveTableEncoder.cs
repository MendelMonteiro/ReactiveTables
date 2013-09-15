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
using System.IO;

namespace ReactiveTables.Framework.Comms
{
    /// <summary>
    /// A class used for encoding data sent on the wire
    /// </summary>
    public interface IReactiveTableEncoder : IDisposable
    {
        /// <summary>
        /// Configure the encoder
        /// </summary>
        /// <param name="outputStream">The stream to write to</param>
        /// <param name="table">The table to read from</param>
        /// <param name="state">Any state required by the encoder</param>
        void Setup(Stream outputStream, IReactiveTable table, object state);

        /// <summary>
        /// Stop encoding the table
        /// </summary>
        void Close();
    }
}