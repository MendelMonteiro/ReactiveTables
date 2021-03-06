﻿// This file is part of ReactiveTables.
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

namespace ReactiveTables.Framework.Protobuf
{
    /// <summary>
    /// Reserved fields used by this implementation
    /// </summary>
    public static class ProtobufFieldIds
    {
        /// <summary>
        /// The row index field id
        /// </summary>
        public const int RowId = short.MaxValue;
    }
}