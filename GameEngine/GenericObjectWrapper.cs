using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine
{
    class GenericObjectWrapper
    {
        private Type type;
        private object obj;

        public GenericObjectWrapper(Type t, object obj)
        {
            type = t;
            this.obj = obj;
        }
        public static GenericObjectWrapper Create<T>(T value)
        {
            return new GenericObjectWrapper(typeof(T), value);
        }    

        public void Set<T>(T value)
        {
            type = typeof(T);
            obj = value;
        }

        public T Get<T>()
        {
            if (typeof(T) != type)
                throw new Exception($"Incorrect type for stored object, expected: {type.FullName}");
            return (T)obj;
        }
    }
}
