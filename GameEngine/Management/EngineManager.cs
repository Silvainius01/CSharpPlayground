using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace GameEngine
{
    internal interface ILayeredObject
    {
        Dictionary<int, int> Layers { get; } 

        void OnLayerChanged(int systemID, int? prevLayer, bool added);
    }

    internal class LayeredSystem<T> where T : ILayeredObject
    {
        public readonly int id;
        public List<int> layerIDs = new List<int>();
        public Dictionary<int, HashSet<T>> objDict = new Dictionary<int, HashSet<T>>();

        public int Count { get { return layerIDs.Count; } }

        public HashSet<T> this[int index]
        {
            get { return objDict[index]; }
            set { objDict[index] = value; }
        }

        public LayeredSystem()
        {
            id = this.GetHashCode();
        }

        /// <summary>
        /// Sets the layer of the object within this sytem. Returns true if the object was added, false if just modified.
        /// </summary>
        public bool SetObjLayer(T obj, int layer)
        {
            // Remove entity from it's current layer
            int? prevLayer = null;
            if (obj.Layers.ContainsKey(id))
            {
                int currLayer = obj.Layers[id];
                if (objDict.ContainsKey(currLayer) && objDict[currLayer].Contains(obj))
                {
                    prevLayer = currLayer;
                    objDict[currLayer].Remove(obj);
                }
            }
            else obj.Layers.Add(id, layer);

            // If the new layer doesn't exist
            if (!objDict.ContainsKey(layer))
            {
                objDict.Add(layer, new HashSet<T>());
                layerIDs.Add(layer);
                layerIDs.Sort();
            }
            objDict[layer].Add(obj);
            obj.OnLayerChanged(id, prevLayer, prevLayer == null);
            return prevLayer == null;
        }
    }

    public class EngineManager
    {
        private static ulong __nextId__ = 0;
        internal static ulong NextId { get { return __nextId__++; } }

        static LayeredSystem<Entity> entityLayers = new LayeredSystem<Entity>();
        internal static Dictionary<int, List<Component>> newComponents = new Dictionary<int, List<Component>>();

        public static void FrameUpdate()
        {
            TimeManager.Update();
            EntityUpdate();
        }

        internal static void SetEntityLayer(Entity ent, int layer)
        {
            entityLayers.SetObjLayer(ent, layer);
        }
        internal static void RegisterNewComponent(Component c)
        {
            if(!newComponents.ContainsKey(c.entity.Layer))
                newComponents.Add(c.entity.Layer, new List<Component>());
            newComponents[c.entity.Layer].Add(c);
            c.Awake();
        }

        internal static void EntityUpdate()
        {
            for(int i = 0; i < entityLayers.Count; ++i)
            {
                int layer = entityLayers.layerIDs[i];

                // Update new components on this layer
                foreach (var c in newComponents[layer])
                    c.Start();
                newComponents[layer].Clear();

                // Update entities on this layer
                foreach (var ent in entityLayers[layer])
                    ent.Update();
            }
        }
    }
}