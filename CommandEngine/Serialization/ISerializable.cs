using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Windows.Markup;

namespace CommandEngine.Serialization
{
    // TODO: one stop serializtion shop.

    internal interface ISerializable
    {
        public SerializedObject GetSerializedObject()
        {
            Type t = this.GetType();
            SerializedObject serialized = new SerializedObject(t);

            foreach (var field in t.GetRuntimeFields())
            {
                if (field.GetCustomAttribute(typeof(DontSerialize)) is not null)
                    continue;

                serialized.values.Add(field.Name, field.GetValue(this));
            }

            foreach (var property in t.GetRuntimeProperties())
            {
                if (!property.CanWrite || property.GetCustomAttribute(typeof(DontSerialize)) is not null)
                    continue;
                serialized.values.Add(property.Name, property.GetValue(this));
            }

            return serialized;
        }
    }

    internal class SerializedObject
    {
        public Type Type { get; set; }
        public Dictionary<string, object?> values = new Dictionary<string, object?>();

        internal SerializedObject(Type type) { Type = type; }

        public bool TryDeserialize<T>(out T result)
        {
            result = default(T);

            if (Type == typeof(T))
            {
                foreach (var field in Type.GetFields())
                {
                    if (values.ContainsKey(field.Name))
                        field.SetValue(result, values[field.Name]);
                }
                foreach (var property in Type.GetProperties())
                {
                    if (values.ContainsKey(property.Name))
                        property.SetValue(result, values[property.Name]);
                }
                return true;
            }

            return false;
        }

        public static SerializedObject GetSerialized(ISerializable serializable) => serializable.GetSerializedObject();
    }

    public class DontSerialize : Attribute
    {

    }
}
