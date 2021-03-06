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

using System;
using System.Collections.Generic;
using ReactiveTables.Framework;

namespace ReactiveTables.Demo.Syncfusion
{
    public interface ISyncfusionViewModel : IObservable<TableUpdate>
    {
        T GetValue<T>(int rowIndex, int columnIndex);
        void SetValue<T>(int rowIndex, int columnIndex, T value);
        int GetRowPosition(int rowIndex);
        int GetColPosition(string columnId);
        string GetColumnId(int columnIndex);
        IObservable<bool> RowPositionsUpdated { get; }
        IList<string> ColumnNames { get; }
    }
}