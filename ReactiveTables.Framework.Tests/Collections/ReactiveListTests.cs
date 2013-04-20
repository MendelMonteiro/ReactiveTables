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

using NUnit.Framework;
using ReactiveTables.Framework.Collections;

namespace ReactiveTables.Framework.Tests.Collections
{
    [TestFixture]
    public class ReactiveListTests
    {
        [Test]
        public void TestAdd()
        {
            var list = CreateAndTestList(100);

            Assert.AreEqual(100, list.Count);
        }

        private static ReactiveList<int> CreateAndTestList(int iterations)
        {
            ReactiveList<int> list = new ReactiveList<int>();

            for (int i = 0; i < iterations; i++)
            {
                list.Add(iterations + i);

                Assert.AreEqual(i + 1, list.Count);
                Assert.AreEqual(iterations + i, list[i]);
            }

            return list;
        }

        [Test]
        public void TestRemoveFromStart()
        {
            int iterations = 100;
            ReactiveList<int> list = CreateAndTestList(iterations);

            for (int i = 0; i < iterations; i++)
            {
                list.RemoveAt(i);

                Assert.AreEqual(iterations - (i + 1), list.Count);
                Assert.AreEqual(iterations + i + 1, list[0]);
            }

            Assert.AreEqual(0, list.Count);
        }

        [Test]
        public void TestRemoveFromEnd()
        {
            int iterations = 100;
            ReactiveList<int> list = CreateAndTestList(iterations);

            for (int i = iterations; i >= 0; i--)
            {
                list.RemoveAt(i);

                Assert.AreEqual(iterations - (i + 1), list.Count);
                Assert.AreEqual(iterations + (i - 1), list[i-1]);
            }

            Assert.AreEqual(0, list.Count);
        }



        [Test]
        public void TestRemoveInTheMiddle()
        {
            int iterations = 100;
            ReactiveList<int> list = CreateAndTestList(iterations);

            list.RemoveAt(50);

            Assert.AreEqual(iterations-1, list.Count);
            Assert.AreEqual(50, list[49]);
            Assert.AreEqual(52, list[50]);
        }

        [Test]
        public void TestInsert()
        {
            // At start
            var list = CreateAndTestList(100);
            list.Insert(0, 1);

            Assert.AreEqual(101, list.Count);
            Assert.AreEqual(1, list[0]);
            Assert.AreEqual(101, list[1]);

            // In middle
            list = CreateAndTestList(100);
            list.Insert(50, 1);

            Assert.AreEqual(101, list.Count);
            Assert.AreEqual(1, list[50]);
            Assert.AreEqual(149, list[49]);
            Assert.AreEqual(150, list[51]);

            // At end
            list = CreateAndTestList(100);
            list.Insert(100, 1);

            Assert.AreEqual(101, list.Count);
            Assert.AreEqual(1, list[100]);
            Assert.AreEqual(200, list[99]);
        }
    }
}