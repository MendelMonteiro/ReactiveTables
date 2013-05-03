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
using ReactiveTables.Framework.Columns;
using ReactiveTables.Framework.Marshalling;

namespace ReactiveTables.Framework.Synchronisation
{
    public class ReactivePassThroughTable : IWritableReactiveTable
    {
        private readonly FieldRowManager _rowManager = new FieldRowManager();
        private readonly ReactiveTable _targetTable;
        private readonly IThreadMarshaller _marshaller;

        public int RowCount
        {
            get { return _rowManager.RowCount; }
        }

        public Dictionary<string, IReactiveColumn> Columns
        {
            get { throw new NotImplementedException(); }
        }

        public PropertyChangedNotifier ChangeNotifier
        {
            get { throw new NotImplementedException(); }
        }

        public ReactivePassThroughTable(ReactiveTable table, IThreadMarshaller marshaller)
        {
            _targetTable = table;
            _marshaller = marshaller;
        }

        public IDisposable Subscribe(IObserver<TableUpdate> observer)
        {
            throw new NotImplementedException();
        }

        public void Unsubscribe(IObserver<TableUpdate> observer)
        {
            throw new NotImplementedException();
        }

        public void AddColumn(IReactiveColumn column)
        {
            throw new NotImplementedException();
        }

        public T GetValue<T>(string columnId, int rowIndex)
        {
            throw new NotImplementedException();
        }

        public IReactiveTable Join(IReactiveTable otherTable, IReactiveTableJoiner joiner)
        {
            throw new NotImplementedException();
        }

        public void ReplayRows(IObserver<TableUpdate> observer)
        {
            throw new NotImplementedException();
        }

        public void SetValue<T>(string columnId, int rowIndex, T value)
        {
            _marshaller.Dispatch(() => _targetTable.SetValue(columnId, rowIndex, value));
        }

        public void SetValue(string columnId, int rowIndex, IReactiveColumn sourceColumn, int sourceRowIndex)
        {
            throw new NotImplementedException();
        }

        public int AddRow()
        {
            int rowIndex = _rowManager.AddRow();
            _marshaller.Dispatch(() => _targetTable.AddRow());
            return rowIndex;
        }

        public void DeleteRow(int rowIndex)
        {
            _rowManager.DeleteRow(rowIndex);
            _marshaller.Dispatch(() => _targetTable.DeleteRow(rowIndex));
        }
    }
}