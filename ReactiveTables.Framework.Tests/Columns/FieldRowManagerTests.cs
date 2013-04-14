using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using ReactiveTables.Framework.Columns;

namespace ReactiveTables.Framework.Tests.Columns
{
    [TestFixture]
    public class FieldRowManagerTests
    {
        [Test]
        public void Basic()
        {
            FieldRowManager rowManager = new FieldRowManager();
            int rowIndex = rowManager.AddRow();
            rowManager.DeleteRow(rowIndex);
            Assert.AreEqual(0, rowManager.RowCount);

            rowIndex = rowManager.AddRow();
            rowManager.DeleteRow(rowIndex);
            Assert.AreEqual(0, rowManager.RowCount);

            var rowIndex2 = rowManager.AddRow();
            var rowIndex3 = rowManager.AddRow();
            var rowIndex4 = rowManager.AddRow();
            rowManager.DeleteRow(rowIndex3);
            Assert.AreEqual(2, rowManager.RowCount);
            rowManager.DeleteRow(rowIndex4);
            Assert.AreEqual(1, rowManager.RowCount);
            var rowIndex5 = rowManager.AddRow();
            Assert.AreEqual(2, rowManager.RowCount);
            rowManager.DeleteRow(rowIndex5);
            Assert.AreEqual(1, rowManager.RowCount);
            rowManager.DeleteRow(rowIndex2);
            Assert.AreEqual(0, rowManager.RowCount);
        }
    }
}
