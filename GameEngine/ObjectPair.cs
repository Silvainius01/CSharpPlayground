using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandEngine
{
    public class ObjectPair<T1, T2>
    {
        public T1 Item1;
        public T2 Item2;

        public ObjectPair(T1 item1, T2 item2)
        {
            Item1 = item1;
            Item2 = item2;
        }
        public ObjectPair((T1, T2) tuple)
        {
            Item1 = tuple.Item1;
            Item2 = tuple.Item2;
        }
    }
}
