using System.Linq;
using NUnit.Framework;
using ReactiveTables.Framework.Columns;

namespace ReactiveTables.Framework.Tests.Columns
{
    [TestFixture]
    public class FieldRowListTests
    {
        [Test]
        public void TestAdd()
        {
            FieldRowList<string> list = new FieldRowList<string>();
            var row = list.Add("42");
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual("42", list[row]);
        }
        
        [Test]
        public void TestEnumerate()
        {
            FieldRowList<string> list = new FieldRowList<string>();
            var row = list.Add("42");
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual("42", list[row]);

            var row2 = list.Add("43");
            Assert.AreEqual(2, list.Count);
            Assert.AreEqual("43", list[row2]);

            var items = list.ToArray();
            Assert.AreEqual("42", items[0]);
            Assert.AreEqual("43", items[1]);
        }
        
        [Test]
        public void TestSet()
        {
            FieldRowList<string> list = new FieldRowList<string>();
            var row = list.Add("42");
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual("42", list[row]);

            list[row] = "43";
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual("43", list[row]);
        }
        
        [Test]
        public void TestRemove()
        {
            FieldRowList<string> list = new FieldRowList<string>();
            var row = list.Add("42");
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual("42", list[row]);

            list.RemoveAt(row);
            Assert.AreEqual(0, list.Count);
        }
        
        [Test]
        public void TestClear()
        {
            FieldRowList<string> list = new FieldRowList<string>();
            var row = list.Add("42");
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual("42", list[row]);
            var row1 = list.Add("43");

            list.Clear();
            Assert.AreEqual(0, list.Count);
        }
    }
}
