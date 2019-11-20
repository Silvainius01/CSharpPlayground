using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine
{
    /// <summary>
    /// Base class for entities, does not contain a definition for position.
    /// </summary>
    public class Entity : ILayeredObject
    {
        public readonly ulong Id;
        public bool Enabled { get; private set; }
        public string Name { get; private set; }

        internal Dictionary<Type, Component> components = new Dictionary<Type, Component>();

        private int __layer__;
        public int Layer
        {
            get { return __layer__; }
            set
            {
                EngineManager.SetEntityLayer(this, value);
                __layer__ = value;
            }
        }

        public Dictionary<int, int> Layers { get; } = new Dictionary<int, int>();

        public Entity(bool enabled = true, int layer = 0)
        {
            Id = EngineManager.NextId;
            Enabled = enabled;
            Name = $"Entity {Id}";
            Layer = layer;
        }
        public Entity(string name, bool enabled = true, int layer = 0)
        {
            Id = EngineManager.NextId;
            Enabled = enabled;
            Name = name;
            Layer = layer;
        }

        public void SetName(string name)
        {
            Name = name;
        }
        public void SetEnabled(bool enabled)
        {
            Enabled = enabled;
        }

        internal void Update()
        {
            foreach (var c in components.Values)
                c.Update();
        }

        public T AddComponent<T>() where T : Component
        {
            T c = (T)Activator.CreateInstance(typeof(T));
            c.entity = this;
            components.Add(typeof(T), c);
            EngineManager.RegisterNewComponent(c);
            return c;
        }
        public T AddComponent<T>(params object[] args) where T : Component
        {
            T c = (T)Activator.CreateInstance(typeof(T), args);
            c.entity = this;
            components.Add(typeof(T), c);
            EngineManager.RegisterNewComponent(c);
            return c;
        }
        public T GetComponent<T>() where T : Component
        {
            if (components.ContainsKey(typeof(T)))
                return components[typeof(T)] as T;
            return null;
        }

        public void OnLayerChanged(int systemID, int? prevLayer, bool added)
        {
            foreach (var kvp in components)
                EngineManager.RegisterNewComponent(kvp.Value);
        }
    }
}
