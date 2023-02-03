using System;
using System.Collections;
using System.Collections.Generic;

namespace GameEngine.Collections
{
    partial class SegmentedList<TKey, TValue>
    {
        public class Segment : IList<TValue>
        {
            public int StartIndex
            {
                get => _start;
                internal set
                {
                    _start = value;
                    UpdateCount();
                }
            }
            public int EndIndex
            {
                get => _end;
                internal set
                {
                    _end = value;
                    UpdateCount();
                }
            }
            public int Count { get => _count; }
            public bool IsReadOnly => false;
            public TKey Key { get; private set; }
            public TValue this[int index]
            {
                get
                {
                    if (index >= Count)
                        throw new IndexOutOfRangeException();
                    return _parentList[index + StartIndex];
                }
                set
                {
                    if (!ValidateItem(value))
                        throw new InvalidOperationException("Cannot set an element to a value of a different key.");
                    _parentList[index + StartIndex] = value;
                }
            }

            private int _count;
            private int _start;
            private int _end;
            private SegmentedList<TKey, TValue> _parentList;
            private KeyFromValueDelegate GetKeyFromValue => _parentList.GetKeyFromValue;

            internal Segment(TKey key, int index, SegmentedList<TKey, TValue> parent)
            {
                _start = index;
                _end = index;
                _count = 1;
                _parentList = parent;
                Key = key;
            }
            internal void UpdatePosition(int amount = 1)
            {
                _start += amount;
                _end += amount;
            }
            internal void Decrement(int amount = 1)
            {
                _start -= amount;
                _end -= amount;
            }

            void UpdateCount() => _count = 1 + EndIndex - StartIndex;
            bool ValidateItem(TValue item) => GetKeyFromValue(item).Equals(Key);

            public int IndexOf(TValue item)
                => _parentList.values.IndexOf(item, StartIndex, Count);

            public void Add(TValue value)
            {
                if (!ValidateItem(value))
                    throw new InvalidOperationException("Cannot add item of differing key.");
                _parentList.AddDirect(this, value);
            }

            public void Insert(int index, TValue item)
            {
                if (!ValidateItem(item))
                    throw new InvalidOperationException("Cannot add item of differing key.");
                if (!Mathc.ValueIsBetween(index, 0, Count - 1))
                    throw new IndexOutOfRangeException();
                _parentList.InsertDirect(this, index + StartIndex, item);
            }

            public bool Remove(TValue item)
            {
                if (!ValidateItem(item))
                    return false;

                for (int i = 0; i < Count; ++i)
                    if (item.Equals(this[i]))
                    {
                        _parentList.RemoveDirect(this, i + StartIndex);
                        return true;
                    }
                return false;
            }

            public void RemoveAt(int index)
            {
                if (!Mathc.ValueIsBetween(index, 0, Count - 1))
                    return;

                _parentList.RemoveDirect(this, index + StartIndex);
            }

            public void Clear() => _parentList.RemoveSegment(Key);

            public bool Contains(TValue item)
            {
                for (int i = 0; i < Count; ++i)
                    if (item.Equals(this[i]))
                        return true;
                return false;
            }

            public void CopyTo(TValue[] array, int arrayIndex)
            {
                _parentList.values.CopyTo(StartIndex, array, arrayIndex, Count);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
            public IEnumerator<TValue> GetEnumerator()
            {
                return new SegmentEnumerator(this);
            }
        }
        internal class SegmentEnumerator : IEnumerator<TValue>
        {
            Segment _segment;
            object IEnumerator.Current => GetCurrent();
            TValue IEnumerator<TValue>.Current => GetCurrent();
            int position = -1; // MoveNext() is called before returning the first element

            public SegmentEnumerator(Segment seg)
            {
                _segment = seg;
            }

            public void Dispose()
            {
                GC.SuppressFinalize(this);
            }
            public bool MoveNext()
            {
                ++position;
                if (position >= _segment.Count)
                    return false;
                return true;
            }
            public void Reset()
            {
                position = -1;
            }

            private TValue GetCurrent()
            {
                try
                {
                    return _segment[position];
                }
                catch (IndexOutOfRangeException)
                {
                    throw new InvalidOperationException();
                }
            }
        }
    }
}
