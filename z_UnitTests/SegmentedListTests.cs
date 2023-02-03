using GameEngine;
using GameEngine.Collections;

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

        [TestMethod]
        public void TestAddFunction()
        {
            int segmentLength = 10;
            int startSegmentCount = 10;
            SegmentedList<int, ListValue> list = new SegmentedList<int, ListValue>(ListValue.GetListKey);

            for (int i = 0; i < segmentLength * startSegmentCount; ++i)
                list.Add(new ListValue(i / startSegmentCount, i));

            // Assert that adding to the list works as expected.
            foreach (SegmentedList<int, ListValue>.Segment segment in list.Segments)
            {
                Assert.AreEqual(segment.Count, segmentLength);
                foreach (ListValue value in segment)
                    Assert.AreEqual(value.ListKey, segment.Key);
            }

            // Assert that adding to a segment works as expected
            var firstSegment = list.GetSegmentAt(0);
            for (int i = 0; i < segmentLength; ++i)
                firstSegment.Add(new ListValue(firstSegment.Key, list.Count));
            Assert.AreEqual(firstSegment.Count, segmentLength * 2);
            foreach (ListValue v in firstSegment)
                Assert.AreEqual(v.ListKey, firstSegment.Key);

            // Check if middle segment is where we would expect
            var middleSegment = list.GetSegmentAt(list.Count / 2);
            int middleSegmentKey = middleSegment.Key;

            // The first segment was doubled, so the start index should shifted by segmentLength
            Assert.AreEqual(middleSegment.StartIndex, (middleSegmentKey * segmentLength) + segmentLength);
        }
    }
}