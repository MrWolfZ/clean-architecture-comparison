using System.Collections.Immutable;
using NUnit.Framework;

namespace CAC.Core.UnitTests.Domain
{
    public sealed class ValueListTests
    {
        [Test]
        public void GivenPrimitiveValues_UsesValueEquality()
        {
            var list1 = ValueList<int>.Empty.Add(1).Add(2);
            var list2 = ValueList.Of(1, 2);
            var list3 = ValueList.Of(2, 1);

            var record1 = new TestRecordWithList<int>(list1);
            var record2 = new TestRecordWithList<int>(list2);
            var record3 = new TestRecordWithList<int>(list3);

            Assert.AreEqual(list1, list2);
            Assert.AreNotEqual(list1, list3);
            Assert.AreEqual(record1, record2);
            Assert.AreNotEqual(record1, record3);

            Assert.IsTrue(list1 == list2);
            Assert.IsFalse(list1 == list3);
            Assert.IsTrue(record1 == record2);
            Assert.IsFalse(record1 == record3);
        }

        [Test]
        public void GivenRecordValues_UsesValueEquality()
        {
            var list1 = ValueList<TestRecord>.Empty.Add(new(1)).Add(new(2));
            var list2 = ValueList.Of(new TestRecord(1), new TestRecord(2));
            var list3 = ValueList.Of(new TestRecord(2), new TestRecord(1));

            var record1 = new TestRecordWithList<TestRecord>(list1);
            var record2 = new TestRecordWithList<TestRecord>(list2);
            var record3 = new TestRecordWithList<TestRecord>(list3);

            Assert.AreEqual(list1, list2);
            Assert.AreNotEqual(list1, list3);
            Assert.AreEqual(record1, record2);
            Assert.AreNotEqual(record1, record3);

            Assert.IsTrue(list1 == list2);
            Assert.IsFalse(list1 == list3);
            Assert.IsTrue(record1 == record2);
            Assert.IsFalse(record1 == record3);
        }

        private sealed record TestRecordWithList<T>(ValueList<T> List)
            where T : notnull;

        private sealed record TestRecord(int Value);
    }
}
