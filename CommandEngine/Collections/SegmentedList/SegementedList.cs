using System;
using System.Collections;
using System.Collections.Generic;

namespace CommandEngine.Collections
{
    /// <summary>
    /// Meant for large sets that may contain duplicate entries/copies of data, such as a list of instantiated prefabs. 
    /// This class implements an internal dictionary that stores the first index of a given subset of like elements.
    /// For smaller sets, this is less effiecient than simply iterating through the list yourself. 
    /// </summary>
    /// <typeparam name="TKey">The key type used to define segments</typeparam>
    /// <typeparam name="TValue">The type of the elements</typeparam>
    public partial class SegmentedList<TKey, TValue> : IList<TValue> where TKey : IComparable<TKey>, IEquatable<TKey>
    {
        public delegate TKey KeyFromValueDelegate(TValue value);

        List<TValue> values;
        Dictionary<TKey, Segment> segmentDict = new Dictionary<TKey, Segment>();
        KeyFromValueDelegate GetKeyFromValue;

        public int Count => values.Count;
        public bool IsReadOnly => false;
        public TValue this[int index]
        {
            get => values[index];
            set // TODO: Implement IEquatable 
            {
                if (GetKeyFromValue(value).Equals(GetKeyFromValue(values[index])))
                    values[index] = value;
                else throw new InvalidOperationException("Attempted to update an element with a differing key");
            }
        }

        public SegmentedList(KeyFromValueDelegate keyFromValue)
        {
            values = new List<TValue>();
            this.GetKeyFromValue = keyFromValue;
        }
        public SegmentedList(KeyFromValueDelegate keyFromValue, int capacity)
        {
            values = new List<TValue>(capacity);
            this.GetKeyFromValue = keyFromValue;
        }
        /// <summary>
        /// Note: this constructor must sort the collection given to it, then construct the segments afterwards.
        /// If you dataset is unsorted, it is faster to add them one by one instead.
        /// </summary>
        public SegmentedList(KeyFromValueDelegate keyFromValue, IEnumerable<TValue> enumerable)
        {
            values = new List<TValue>(enumerable);
            this.GetKeyFromValue = keyFromValue;
            Sort();
        }


        public Segment GetSegmentAt(int index) => segmentDict[GetKeyFromValue(this[index])];
        public Segment GetSegment(TKey key) => segmentDict[key];
        public bool TryGetSegmentIndex(TKey key, out Segment segment) => segmentDict.TryGetValue(key, out segment);

        public bool SegmentExists(TKey key) => segmentDict.ContainsKey(key);
        public bool SegmentExists(TValue value) => segmentDict.ContainsKey(GetKeyFromValue(value));

        public bool RemoveSegment(TKey segmentKey)
        {
            if (!SegmentExists(segmentKey))
                return false;

            Segment segment = segmentDict[segmentKey];
            segmentDict.Remove(segmentKey);
            values.RemoveRange(segment.StartIndex, segment.Count);
            MoveSegementsAbove(segment.StartIndex - 1, -segment.Count);

            segment.StartIndex = -1;
            segment.EndIndex = -1;

            return true;
        }

        void AddDirect(Segment segment, TValue value)
        {
            if (segment.EndIndex == this.LastIndex())
            {
                segment.EndIndex = this.Count;
                values.Add(value);
            }
            else InsertDirect(segment, segment.EndIndex + 1, value);
        }
        public void Add(TValue value)
        {
            TKey key = GetKeyFromValue(value);
            if (!segmentDict.TryGetValue(key, out Segment segment))
            {
                segmentDict.Add(key, new Segment(key, this.Count, this));
                values.Add(value);
            }
            else AddDirect(segment, value);
        }

        void InsertDirect(Segment segment, int index, TValue value)
        {
            values.Insert(index, value);
            MoveSegementsAbove(segment.EndIndex, 1);
            ++segment.EndIndex;
        }
        public void Insert(int index, TValue value)
        {
            TKey key = GetKeyFromValue(value);
            if (segmentDict.TryGetValue(key, out Segment segment))
            {
                // If this value belongs in an existing segment but is inserted outside the range, THROW THAT SHIT
                if (!Mathc.ValueIsBetween(index, segment.StartIndex, segment.EndIndex))
                    throw new InvalidOperationException("Cannot insert a value outside its segment's range.");
                InsertDirect(segment, index, value);
            }
        }

        public bool Remove(TValue value)
        {
            if (values.Remove(value))
            {
                var key = GetKeyFromValue(value);
                var data = segmentDict[key];

                // Remove if its the last element in the segment
                if (data.StartIndex == data.EndIndex)
                    segmentDict.Remove(key);

                --data.EndIndex;
                MoveSegementsAbove(data.EndIndex, -1);
                return true;
            }
            return false;
        }

        void RemoveDirect(Segment segment, int index)
        {
            values.RemoveAt(index);

            // Remove if its the last element in the segment
            if (segment.StartIndex == segment.EndIndex)
                segmentDict.Remove(segment.Key);
            else
            {
                --segment.EndIndex;
                MoveSegementsAbove(segment.EndIndex, -1);
            }
        }
        public void RemoveAt(int index)
        {
            if (!Mathc.ValueIsBetween(index, 0, Count - 1)) 
                return;

            var key = GetKeyFromValue(this[index]);
            var segment = segmentDict[key];
            RemoveDirect(segment, index);
        }

        public void Sort()
        {
            values.Sort((x, y) => GetKeyFromValue(x).CompareTo(GetKeyFromValue(y)));

            segmentDict.Clear();
            for (int i = 0; i < Count; ++i)
            {
                var key = GetKeyFromValue(this[i]);
                if (!segmentDict.ContainsKey(key))
                    segmentDict.Add(key, new Segment(key, i, this));
                else
                    segmentDict[key].EndIndex = i;
            }
        }

        public void Clear()
        {
            values.Clear();
            segmentDict.Clear();
        }

        public int IndexOf(TValue item)
        {
            TKey key = GetKeyFromValue(item);
            if (segmentDict.TryGetValue(key, out Segment segment))
                return values.IndexOf(item, segment.StartIndex, segment.Count);
            return -1;
        }

        public bool Contains(TValue item)
        {
            TKey key = GetKeyFromValue(item);
            if (segmentDict.TryGetValue(key, out Segment segment))
            {
                for (int i = segment.StartIndex; i < segment.Count; ++i)
                    if (item.Equals(values[i]))
                        return true;
            }
            return false;
        }

        public void CopyTo(TValue[] array, int arrayIndex) => values.CopyTo(array, arrayIndex);

        IEnumerator IEnumerable.GetEnumerator() => values.GetEnumerator();
        public IEnumerator<TValue> GetEnumerator() => values.GetEnumerator();
        public IEnumerable<Segment> Segments => segmentDict.Values;
        
        void MoveSegementsAbove(int index, int amount)
        {
            foreach (var k in segmentDict.Keys)
            {
                if (segmentDict[k].StartIndex > index)
                    segmentDict[k].UpdatePosition(amount);
            }
        }
    }
}

