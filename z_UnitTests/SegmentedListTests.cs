using CommandEngine;
using CommandEngine.Collections;

namespace ACLibTests
{
    [TestClass]
    public class SegmentedListTests
    {
        class ListValue
        {
            public int ListKey { get; set; }
            public int UniqueValue { get; set; }

            public ListValue(int key, int value)
            {
                this.ListKey = key;
                this.UniqueValue = value;
            }

            public static int GetListKey(ListValue v) => v.ListKey;

        }

        private const int SegmentLength = 10;
        private const int StartSegmentCount = 10;
        private SegmentedList<int, ListValue> CreateTestList()
        {
            SegmentedList<int, ListValue> list = new SegmentedList<int, ListValue>(ListValue.GetListKey);

            for (int i = 0; i < SegmentLength * StartSegmentCount; ++i)
                list.Add(new ListValue(i / StartSegmentCount, i));

            return list;
        }

        // Since Visual Studio executes tests alphabetically, tests follow this pattern: [letter][number]_[name]
        // NO TEST SHOULD RELY ON ANOTHER TO RUN. In our case, however, SegmentedList needs to make sure
        // its adding function works, otherwise all other tests will be inaccurate. 
        // NOTE: treat all passing tests below a failed higher-order one as failures.

        [TestMethod]
        public void A000_TestAdd()
        {
            SegmentedList<int, ListValue> list = CreateTestList();

            // Assert that adding to the list works as expected.
            foreach (SegmentedList<int, ListValue>.Segment segment in list.Segments)
            {
                Assert.AreEqual(segment.Count, SegmentLength);
                foreach (ListValue value in segment)
                    Assert.AreEqual(value.ListKey, segment.Key);
            }

            // Assert that adding to a segment works as expected
            var firstSegment = list.GetSegmentAt(0);
            for (int i = 0; i < SegmentLength; ++i)
                firstSegment.Add(new ListValue(firstSegment.Key, list.Count));
            Assert.AreEqual(firstSegment.Count, SegmentLength * 2);
            foreach (ListValue v in firstSegment)
                Assert.AreEqual(v.ListKey, firstSegment.Key);

            // Check if middle segment is where we would expect
            // The first segment was doubled, so the start index should shifted by segmentLength
            var middleSegment = list.GetSegmentAt(list.Count / 2);
            int middleSegmentKey = middleSegment.Key;
            Assert.AreEqual(middleSegment.StartIndex, (middleSegmentKey * SegmentLength) + SegmentLength);

            // Test adding to a mid-list segment
            var secondSegment = list.GetSegment(1);
            int middleSegmentStart = middleSegment.StartIndex;
            for (int i = 0; i < SegmentLength; ++i)
                secondSegment.Add(new ListValue(secondSegment.Key, list.Count));
            // Make sure the first segment is unaltered
            Assert.AreEqual(firstSegment.StartIndex, 0);
            Assert.AreEqual(firstSegment.EndIndex, firstSegment.Count - 1);
            // Make sure the second segment's starting place didnt change
            Assert.AreEqual(secondSegment.StartIndex, firstSegment.Count);
            // Make sure the middle segment, again, went where we expect it to.
            Assert.AreEqual(middleSegment.StartIndex, middleSegmentStart + SegmentLength);
        }

        [TestMethod]
        public void Z999_TestRemove()
        {
            SegmentedList<int, ListValue> list = CreateTestList();

            int totalItems = list.Count;
            var firstSegment = list.GetSegmentAt(0);
            var secondSegment = list.GetSegmentAt(SegmentLength);
            var middleSegment = list.GetSegmentAt(list.Count / 2);

            // Test remove by object
            int numRemovedTotal = 1;
            int numRemovedFirst = 1;
            var item = firstSegment.RandomItem();
            firstSegment.Remove(item);
            Assert.AreEqual(firstSegment.StartIndex, 0); // StartIndex should be unchanged
            Assert.AreEqual(firstSegment.Count, SegmentLength - numRemovedFirst); // Ensure count changed
            Assert.AreEqual(firstSegment.EndIndex, firstSegment.Count - 1); // EndIndex should be count-1 still
            Assert.AreEqual(firstSegment.Count, SegmentLength - numRemovedFirst); // Count should be 1 fewer
            Assert.AreEqual(secondSegment.StartIndex, SegmentLength - numRemovedFirst); // secondSegment start should be shifted
            Assert.AreEqual(secondSegment.EndIndex, SegmentLength * 2 - numRemovedFirst - 1); // secondSegment end should be shifted
            Assert.AreEqual(list.Count, totalItems - numRemovedTotal); // Ensure the underlying list is updated

            // Test remove by index
            numRemovedTotal = 2;
            numRemovedFirst = 2;
            firstSegment.RemoveAt(1);
            Assert.AreEqual(firstSegment.StartIndex, 0); // StartIndex should be unchanged
            Assert.AreEqual(firstSegment.Count, SegmentLength - numRemovedFirst); // Ensure count changed
            Assert.AreEqual(firstSegment.EndIndex, firstSegment.Count - 1); // EndIndex should be count-1 still
            Assert.AreEqual(secondSegment.StartIndex, SegmentLength - numRemovedFirst); // secondSegment start should be shifted
            Assert.AreEqual(secondSegment.EndIndex, SegmentLength * 2 - numRemovedFirst - 1); // secondSegment end should be shifted
            Assert.AreEqual(list.Count, totalItems - numRemovedTotal); // Ensure the underlying list is updated

            // Same tests, but from the second segment.
            numRemovedTotal = 3;
            int numRemovedSecond = 1;
            secondSegment.RemoveAt(0);
            Assert.AreEqual(firstSegment.StartIndex, 0); // firstSegment should be unchanged
            Assert.AreEqual(firstSegment.Count, SegmentLength - numRemovedFirst);
            Assert.AreEqual(firstSegment.EndIndex, firstSegment.Count - 1);
            Assert.AreEqual(secondSegment.Count, SegmentLength - numRemovedSecond); // should be different by -1
            Assert.AreEqual(secondSegment.EndIndex, SegmentLength * 2 - numRemovedTotal - 1); // should be different by -3 
            Assert.AreEqual(middleSegment.StartIndex, SegmentLength * middleSegment.Key - numRemovedTotal); // should be diffent by -3
            Assert.AreEqual(middleSegment.EndIndex, SegmentLength * (middleSegment.Key + 1) - numRemovedTotal - 1); // should be different by -3
            Assert.AreEqual(list.Count, totalItems - numRemovedTotal); // Ensure the underlying list is updated
        }

        [TestMethod]
        public void Z999_TestRemoveSegment()
        {
            SegmentedList<int, ListValue> list = CreateTestList();

            int totalItems = list.Count;
            var firstSegment = list.GetSegmentAt(0);
            var secondSegment = list.GetSegmentAt(SegmentLength);
            var middleSegment = list.GetSegmentAt(list.Count / 2);

            int middleStartExpected = SegmentLength * (middleSegment.Key - 1);
            Assert.IsTrue(list.RemoveSegment(secondSegment.Key));
            Assert.IsFalse(list.SegmentExists(secondSegment.Key));
            Assert.AreEqual(list.Count, totalItems - SegmentLength);
            Assert.AreEqual(firstSegment.StartIndex, 0); // First segment should be unchanged
            Assert.AreEqual(firstSegment.EndIndex, SegmentLength - 1);
            Assert.AreEqual(middleSegment.StartIndex, middleStartExpected); // Middle should be shifted
            Assert.AreEqual(middleSegment.EndIndex, middleStartExpected + SegmentLength-1);

            Assert.IsFalse(list.RemoveSegment(secondSegment.Key));
        }
    }
}